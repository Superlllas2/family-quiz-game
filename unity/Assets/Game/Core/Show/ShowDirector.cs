using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Show
{
    /// <summary>
    /// Runs the show on the injected clock (docs/02_show_flow.md): Lobby, StartCountdown, then the segments of the
    /// Standard show script (content/show_scripts.json): Intro (first game of the session only), rounds of TopicVote,
    /// questions x (QuestionShow, Answer, Reveal, Scores) and RoundEnd, FinalIntro, the race as RaceQuestion and
    /// RaceReveal per statement, Results, the Poll, then back to the Lobby (D76). Power ups are drawn at Reveal, used in
    /// the pick window (Reveal and Scores, one phaseId) and land when the next Answer starts. Host line triggers are
    /// raised as HostLineRequested; Dialogue picks the lines. Every duration comes from config, with staging.extraSeconds added where a phase has one. A phase that
    /// ends on its timer ends at its scheduled time and the next one starts at that same time, so on a fake clock every
    /// phase lasts its budget exactly.
    /// </summary>
    public sealed class ShowDirector
    {
        private readonly GameState _state;
        private readonly GameConfig _config;
        private readonly GameContent _content;
        private readonly IClock _clock;
        private readonly Func<int> _newSeed;
        private readonly HashSet<string> _usedInGame = new HashSet<string>();
        private readonly HashSet<string> _usedRaceInGame = new HashSet<string>();
        private readonly HashSet<string> _chosenTopics = new HashSet<string>();
        private readonly List<string> _segments = new List<string>();
        private IRandom _random;
        private int _segmentIndex;
        private int _roundIndex;
        private int _questionIndex;

        /// <summary>phaseId of the input phase (TopicVote, Answer, RaceQuestion) that closed last: an input naming it gets "closed", not "stale".</summary>
        private long _closedInputPhaseId;

        /// <summary>phaseId of the pick window (Reveal and Scores) that closed last: a power up naming it gets "closed".</summary>
        private long _closedPickPhaseId;

        public ShowDirector(GameState state, GameContent content, IClock clock, Func<int> newSeed)
        {
            _state = state;
            _content = content;
            _config = content.Config;
            _clock = clock;
            _newSeed = newSeed;
        }

        private long Now => _clock.NowMs;

        /// <summary>The question bank (D130): the refined questions and the topics a vote can offer from them.</summary>
        private QuestionBank Bank => _content.Bank;

        /// <summary>Seconds from config to whole milliseconds.</summary>
        public static long ToMs(double seconds)
        {
            return (long)Math.Round(seconds * 1000.0, MidpointRounding.AwayFromZero);
        }

        public void OpenLobby(List<IGameEvent> output)
        {
            BeginUntimed(GamePhase.Lobby, Now, output);
        }

        // ---------------------------------------------------------------- seen lists

        /// <summary>The seen lists saved by an earlier session (docs/02_show_flow.md, "Seen questions").</summary>
        public void LoadSeen(IEnumerable<string> questions, IEnumerable<string> race, IEnumerable<string> facts = null)
        {
            if (facts != null)
                foreach (var id in facts) _state.MutableSeenFactIds.Add(id);
            if (questions != null)
                foreach (var id in questions) _state.MutableSeenQuestionIds.Add(id);
            if (race != null)
                foreach (var id in race) _state.MutableSeenRaceIds.Add(id);
        }

        /// <summary>"Reset seen questions" (TV settings panel): clears every seen list at once (questions, race, facts).</summary>
        public void ResetSeen(List<IGameEvent> output)
        {
            _state.MutableSeenQuestionIds.Clear();
            _state.MutableSeenRaceIds.Clear();
            _state.MutableSeenFactIds.Clear();
            output.Add(new SeenChanged(true));
            output.Add(new SettingsChanged(SettingsChanged.SourceTv, null, SettingsChanged.SettingResetSeen, "true"));
        }

        // ---------------------------------------------------------------- lobby actions

        /// <summary>
        /// PROTOCOL.md, "lobby actions" (D76): ready and notReady in the Lobby or the countdown; ready only once the
        /// player picked a character. Taps never start or stop the countdown themselves: <see cref="Reevaluate"/> does.
        /// </summary>
        public bool CanLobbyAction(PlayerState player, string action)
        {
            bool lobbyOrCountdown = _state.Phase == GamePhase.Lobby || _state.Phase == GamePhase.StartCountdown;
            switch (action)
            {
                case LobbyActions.Ready:
                    return lobbyOrCountdown && player.CharacterPicked;
                case LobbyActions.NotReady:
                    return lobbyOrCountdown;
                default:
                    return false;
            }
        }

        /// <summary>Sets the ready mark for a lobby action that <see cref="CanLobbyAction"/> allowed; no change, no event.</summary>
        public void ApplyLobbyAction(PlayerState player, string action, List<IGameEvent> output)
        {
            bool ready = action == LobbyActions.Ready;
            if (player.Ready == ready) return;
            player.Ready = ready;
            output.Add(new ReadyChanged(player.PlayerId, ready));
        }

        /// <summary>Every connected player is ready and players.min or more are connected (D76).</summary>
        private bool EveryoneReady()
        {
            int connected = 0;
            foreach (var p in _state.Players)
            {
                if (!p.Connected) continue;
                if (!p.Ready) return false;
                connected++;
            }

            return connected >= _config.Players.Min;
        }

        // ---------------------------------------------------------------- inputs

        /// <summary>
        /// Why an input is refused, or null when it is accepted. Order: wrong phaseId (stale, or closed for the input
        /// phase that just closed), a phase or kind that takes no such input, no turn, already answered, or a bad field
        /// (invalid), at or after the deadline plus net.inputGraceMs (closed, D73).
        /// </summary>
        public string CheckInput(PlayerState player, SubmitAnswer cmd)
        {
            if (cmd.PhaseId != _state.PhaseId)
                return _closedInputPhaseId != 0 && cmd.PhaseId == _closedInputPhaseId ? AckReasons.Closed : AckReasons.Stale;
            switch (_state.Phase)
            {
                case GamePhase.TopicVote:
                    return CheckVote(player, cmd);
                case GamePhase.Answer:
                    return CheckAnswer(player, cmd);
                case GamePhase.RaceQuestion:
                    return CheckRaceAnswer(player, cmd);
                default:
                    return AckReasons.Invalid;
            }
        }

        private string CheckVote(PlayerState player, SubmitAnswer cmd)
        {
            var round = _state.Round;
            if (cmd.Kind != InputKinds.Vote || round == null || round.Votes.ContainsKey(player.PlayerId)) return AckReasons.Invalid;
            if (!cmd.Door.HasValue || cmd.Door.Value < 0 || cmd.Door.Value >= round.Doors.Count) return AckReasons.Invalid;
            return null;
        }

        private string CheckAnswer(PlayerState player, SubmitAnswer cmd)
        {
            var question = _state.Question;
            var turn = player.Turn;
            if (question == null || turn == null || turn.Locked || !cmd.Ms.HasValue) return AckReasons.Invalid;
            if (question.IsClosestValue)
            {
                if (cmd.Kind != InputKinds.Value || !cmd.Value.HasValue) return AckReasons.Invalid;
                double v = cmd.Value.Value;
                var range = question.Def.Range;
                if (double.IsNaN(v) || double.IsInfinity(v) || v < range[0] || v > range[1]) return AckReasons.Invalid;
            }
            else
            {
                if (cmd.Kind != InputKinds.Choice) return AckReasons.Invalid;
                if (!cmd.Choice.HasValue || cmd.Choice.Value < 0 || cmd.Choice.Value >= question.DisplayOrder.Length)
                    return AckReasons.Invalid;
            }

            return Now >= turn.DeadlineMs + _config.Net.InputGraceMs ? AckReasons.Closed : null;
        }

        private string CheckRaceAnswer(PlayerState player, SubmitAnswer cmd)
        {
            var turn = player.Turn;
            if (cmd.Kind != InputKinds.TrueFalse || turn == null || turn.Locked || !cmd.Answer.HasValue || !cmd.Ms.HasValue)
                return AckReasons.Invalid;
            return Now >= turn.DeadlineMs + _config.Net.InputGraceMs ? AckReasons.Closed : null;
        }

        /// <summary>Applies an input that <see cref="CheckInput"/> accepted. The end checks run after the command.</summary>
        public void LockInput(PlayerState player, SubmitAnswer cmd, List<IGameEvent> output)
        {
            switch (_state.Phase)
            {
                case GamePhase.TopicVote:
                    _state.Round.MutableVotes[player.PlayerId] = cmd.Door.Value;
                    output.Add(new VoteCast(player.PlayerId, _state.PhaseId, cmd.Door.Value));
                    break;
                case GamePhase.Answer:
                    LockAnswer(player, cmd, output);
                    break;
                case GamePhase.RaceQuestion:
                    LockRaceAnswer(player, cmd, output);
                    break;
            }
        }

        private void LockAnswer(PlayerState player, SubmitAnswer cmd, List<IGameEvent> output)
        {
            var turn = player.Turn;
            var question = _state.Question;
            Lock(turn, cmd);
            if (question.IsClosestValue)
            {
                var def = question.Def;
                double value = cmd.Value.Value;
                turn.Value = value;
                int points = Scoring.ClosestValue(_config.Scoring, _config.ClosestValue, value, def.Answer.Value, def.Tolerance.Value,
                    question.Multiplier);
                bool correct = Scoring.ClosestValueCorrect(value, def.Answer.Value, def.Tolerance.Value);
                output.Add(new AnswerLocked(player.PlayerId, _state.PhaseId, InputKinds.Value, -1, turn.Ms.Value, correct, points,
                    value: value));
            }
            else
            {
                int choice = cmd.Choice.Value;
                turn.Choice = choice;
                bool correct = choice == question.CorrectDisplayIndex;
                int points = Scoring.MultipleChoice(_config.Scoring, correct, turn.Ms.Value, turn.WindowMs, question.Multiplier);
                output.Add(new AnswerLocked(player.PlayerId, _state.PhaseId, InputKinds.Choice, choice, turn.Ms.Value, correct, points));
            }
        }

        private void LockRaceAnswer(PlayerState player, SubmitAnswer cmd, List<IGameEvent> output)
        {
            var turn = player.Turn;
            Lock(turn, cmd);
            bool answer = cmd.Answer.Value;
            turn.TrueFalse = answer;
            bool correct = answer == _state.Race.Statement.Answer;
            output.Add(new AnswerLocked(player.PlayerId, _state.PhaseId, InputKinds.TrueFalse, -1, turn.Ms.Value, correct, 0,
                trueFalse: answer));
        }

        private void Lock(PlayerTurn turn, SubmitAnswer cmd)
        {
            turn.InputId = cmd.InputId;
            turn.Locked = true;
            turn.RawMs = cmd.Ms.Value;
            turn.Ms = Scoring.ClampMs(cmd.Ms.Value, turn.WindowMs);
            turn.LockedAtMs = Now;
        }

        // ---------------------------------------------------------------- power ups (docs/03_rules.md, "Power ups")

        /// <summary>
        /// The pick window is open: Reveal or Scores of a round question, power ups on in this game, and not the last
        /// question of the last round (there the pick is hidden, docs/03_rules.md "Using").
        /// </summary>
        public bool PickWindowOpen
        {
            get
            {
                if (_state.Phase != GamePhase.Reveal && _state.Phase != GamePhase.Scores) return false;
                var question = _state.Question;
                if (question == null || _state.Settings.DrawableTypes.Count == 0) return false;
                return !IsLastQuestionOfGame(question);
            }
        }

        /// <summary>The game question number a power up used now lands on: the next question.</summary>
        public int LandsOnQuestion => (_state.Question?.Number ?? 0) + 1;

        private bool IsLastQuestionOfGame(QuestionInPlay question)
        {
            return question.RoundIndex >= _state.RoundCount - 1 && question.Slot >= _state.QuestionCount - 1;
        }

        /// <summary>
        /// Why a power up use is refused, or null: wrong phaseId (stale, or closed for the pick window that just closed),
        /// no open pick window or a bad field (invalid), not holding that power up (noPowerUp), a target that is the
        /// player, not seated or not allowed by the targeting mode (invalid), a target already hit for the next question
        /// or immune (immune).
        /// </summary>
        public string CheckPowerUp(PlayerState player, UsePowerUp cmd)
        {
            if (cmd.PhaseId != _state.PhaseId)
                return _closedPickPhaseId != 0 && cmd.PhaseId == _closedPickPhaseId ? AckReasons.Closed : AckReasons.Stale;
            if (!PickWindowOpen || !PowerUpTypes.IsValid(cmd.PowerUp)) return AckReasons.Invalid;
            if (player.HeldPowerUp == null || player.HeldPowerUp != cmd.PowerUp) return AckReasons.NoPowerUp;
            var target = _state.FindById(cmd.Target);
            if (target == null || target == player) return AckReasons.Invalid;
            if (!Targeting.AllowedByRank(_state.Settings.Targeting, Ranking.SharedRank(_state, player), Ranking.SharedRank(_state, target)))
                return AckReasons.Invalid;
            if (Targeting.Blocked(target, LandsOnQuestion, _config.PowerUps)) return AckReasons.Immune;
            return null;
        }

        /// <summary>Uses a power up that <see cref="CheckPowerUp"/> accepted: it waits on the target for the next Answer.</summary>
        public void ApplyPowerUp(PlayerState player, UsePowerUp cmd, List<IGameEvent> output)
        {
            var target = _state.FindById(cmd.Target);
            player.HeldPowerUp = null;
            player.UsedPowerUpInPhaseId = _state.PhaseId;
            player.UsedPowerUp = cmd.PowerUp;
            player.UsedPowerUpTarget = target.PlayerId;
            target.MutablePendingEffects.Add(new PendingEffect(cmd.PowerUp, player.PlayerId, player.Name));
            output.Add(new PowerUpUsed(player.PlayerId, cmd.PowerUp, target.PlayerId, _state.PhaseId));
            output.Add(new PlayerUpdated(player.PlayerId));
            output.Add(new HostLineRequested(HostTriggers.PowerUpUsed, player.Name, target.Name, _state.Question.Number));
        }

        /// <summary>
        /// Why an effectCleared input is refused, or null: wrong phaseId (stale, or closed for the Answer that just
        /// closed), not in Answer, no effect on this player in this Answer, already cleared, another effect, no ms (invalid).
        /// </summary>
        public string CheckClearEffect(PlayerState player, ClearEffect cmd)
        {
            if (cmd.PhaseId != _state.PhaseId)
                return _closedInputPhaseId != 0 && cmd.PhaseId == _closedInputPhaseId ? AckReasons.Closed : AckReasons.Stale;
            var turn = player.Turn;
            if (_state.Phase != GamePhase.Answer || turn == null || turn.Effect == null || turn.EffectCleared) return AckReasons.Invalid;
            if (cmd.Effect != turn.Effect || !cmd.Ms.HasValue || cmd.Ms.Value < 0) return AckReasons.Invalid;
            return null;
        }

        public void ApplyClearEffect(PlayerState player, ClearEffect cmd, List<IGameEvent> output)
        {
            player.Turn.EffectCleared = true;
            output.Add(new EffectCleared(player.PlayerId, cmd.Effect, cmd.Ms.Value));
        }

        /// <summary>At Reveal, after scoring: each correct player not holding a power up may draw one (docs/03_rules.md, "Earning").</summary>
        private void DrawPowerUps(QuestionInPlay question, List<IGameEvent> output)
        {
            if (IsLastQuestionOfGame(question)) return; // nothing to use it on: unused power ups are lost after this question
            var types = _state.Settings.DrawableTypes;
            if (types.Count == 0) return;
            int players = _state.Players.Count;
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null || !turn.Correct || p.HeldPowerUp != null) continue; // powerUps.maxHeld is 1
                double chance = PowerUpDealer.Chance(turn.RankBefore, players, _config.PowerUps.DrawChanceLeader);
                var drawn = PowerUpDealer.Draw(_random, chance, types);
                if (drawn == null) continue;
                p.HeldPowerUp = drawn;
                output.Add(new PowerUpDrawn(p.PlayerId, drawn, turn.RankBefore, chance));
            }
        }

        /// <summary>When an Answer phase starts: every power up waiting on a player lands on them (docs/03_rules.md, "Using").</summary>
        private void ApplyEffects(QuestionInPlay question, List<IGameEvent> output)
        {
            foreach (var p in _state.Players)
            {
                if (p.PendingEffects.Count == 0 || p.Turn == null) continue;
                foreach (var effect in p.PendingEffects)
                {
                    p.Turn.Effect = effect.PowerUp;
                    p.Turn.EffectFrom = effect.AttackerName;
                    output.Add(new PowerUpApplied(p.PlayerId, effect.AttackerId, effect.AttackerName, effect.PowerUp, _state.PhaseId));
                }

                p.LastHitQuestion = question.Number;
                p.MutablePendingEffects.Clear();
            }
        }

        // ---------------------------------------------------------------- the poll

        /// <summary>Why a poll is refused, or null: not in the Poll phase (closed), no answers object or a second poll (invalid).</summary>
        public string CheckPoll(PlayerState player, SubmitPoll cmd)
        {
            if (_state.Phase != GamePhase.Poll) return AckReasons.Closed;
            if (cmd.Answers == null || _state.SubmittedPoll(player.PlayerId)) return AckReasons.Invalid;
            return null;
        }

        public void ApplyPoll(PlayerState player, SubmitPoll cmd, List<IGameEvent> output)
        {
            _state.MutablePollSubmitted.Add(player.PlayerId);
            output.Add(new PollSubmitted(player.PlayerId, PollAnswers.Clean(_content.Poll, cmd.Answers)));
        }

        /// <summary>Every connected player submitted; false when nobody is connected (the poll then runs to its timer).</summary>
        private bool EveryConnectedPlayerSubmittedPoll()
        {
            bool any = false;
            foreach (var p in _state.Players)
            {
                if (!p.Connected) continue;
                any = true;
                if (!_state.SubmittedPoll(p.PlayerId)) return false;
            }

            return any;
        }

        private void BeginPoll(long at, List<IGameEvent> output)
        {
            _state.MutablePollSubmitted.Clear();
            BeginTimed(GamePhase.Poll, at, ToMs(_config.Poll.MaxSeconds), 0, output);
        }

        // ---------------------------------------------------------------- back to lobby

        /// <summary>
        /// "Back to lobby" (docs/02_show_flow.md): from any phase everyone returns to the Lobby with seats and characters
        /// kept and ready cleared; a running game is abandoned (phaseEnded abandoned, gameEnded backToLobby).
        /// </summary>
        public void BackToLobby(List<IGameEvent> output)
        {
            long now = Now;
            output.Add(new SettingsChanged(SettingsChanged.SourceTv, null, SettingsChanged.SettingBackToLobby, "true"));
            if (_state.Phase == GamePhase.Lobby) return;
            if (_state.Question != null && _state.Phase == GamePhase.Answer) _state.Question.AnswerClosed = true;
            EndPhase(PhaseEndReasons.Abandoned, now, output);
            if (_state.GameRunning)
            {
                _state.GameRunning = false;
                output.Add(new GameEnded(_state.GameNumber, GameEnded.ReasonBackToLobby, null, now - _state.GameStartedAtMs));
            }

            ReturnToLobby(now, output);
        }

        /// <summary>
        /// New game from the pause menu (decisions/0018, D107): the running game is abandoned (gameEnded reason restart)
        /// and everyone returns to the Lobby as with Back to lobby; then every connected player with a character is
        /// marked ready, so <see cref="Reevaluate"/> starts the countdown at once when players.min or more are connected.
        /// Nothing happens in the Lobby.
        /// </summary>
        public void Restart(List<IGameEvent> output)
        {
            if (_state.Phase == GamePhase.Lobby) return;
            long now = Now;
            output.Add(new SettingsChanged(SettingsChanged.SourceTv, null, SettingsChanged.SettingNewGame, "true"));
            if (_state.Question != null && _state.Phase == GamePhase.Answer) _state.Question.AnswerClosed = true;
            EndPhase(PhaseEndReasons.Abandoned, now, output);
            if (_state.GameRunning)
            {
                _state.GameRunning = false;
                output.Add(new GameEnded(_state.GameNumber, GameEnded.ReasonRestart, null, now - _state.GameStartedAtMs));
            }

            ReturnToLobby(now, output);
            foreach (var p in _state.Players)
            {
                if (!p.Connected || !p.CharacterPicked || p.Ready) continue;
                p.Ready = true;
                output.Add(new ReadyChanged(p.PlayerId, true));
            }
        }

        // ---------------------------------------------------------------- question feedback (decisions/0018)

        /// <summary>
        /// The rating card is up: question feedback is on and this is the Reveal or Scores of a round question (the race
        /// has its own phases, so it never gets a card).
        /// </summary>
        public bool RatingOpen =>
            _state.FeedbackEnabled && _state.Question != null &&
            (_state.Phase == GamePhase.Reveal || _state.Phase == GamePhase.Scores);

        /// <summary>
        /// Why a rating tap is refused, or null: wrong phaseId (stale, or closed for the Scores that just ended), no card
        /// up, an item not in feedback.items or stars that are not a whole number from 1 to feedback.stars (invalid).
        /// </summary>
        public string CheckRating(PlayerState player, RateQuestion cmd)
        {
            if (cmd.PhaseId != _state.PhaseId)
                return _closedPickPhaseId != 0 && cmd.PhaseId == _closedPickPhaseId ? AckReasons.Closed : AckReasons.Stale;
            if (!RatingOpen) return AckReasons.Invalid;
            if (cmd.Item == null || !_config.Feedback.Items.Contains(cmd.Item)) return AckReasons.Invalid;
            if (!cmd.Stars.HasValue) return AckReasons.Invalid;
            double stars = cmd.Stars.Value;
            if (stars < 1 || stars > _config.Feedback.Stars || Math.Abs(stars - Math.Round(stars)) > 1e-9) return AckReasons.Invalid;
            return null;
        }

        /// <summary>Keeps a rating that <see cref="CheckRating"/> accepted; a later tap on the same item replaces it.</summary>
        public void ApplyRating(PlayerState player, RateQuestion cmd, List<IGameEvent> output)
        {
            var question = _state.Question;
            int stars = (int)Math.Round(cmd.Stars.Value);
            question.SetRating(player.PlayerId, cmd.Item, stars);
            var turn = player.Turn;
            bool? correct = turn != null && turn.Locked ? turn.Correct : (bool?)null;
            var def = question.Def;
            output.Add(new QuestionRated(player.PlayerId, def.Id, def.Topic, def.Format, def.Difficulty, cmd.Item, stars, correct,
                _state.PhaseId));
        }

        // ---------------------------------------------------------------- the clock

        /// <summary>True in the phases where players answer with personal deadlines: Answer and RaceQuestion.</summary>
        private bool InTurnPhase => _state.Phase == GamePhase.Answer || _state.Phase == GamePhase.RaceQuestion;

        /// <summary>
        /// Rules that react to a changed state, run after every command and every tick (D76): the Lobby starts the
        /// countdown once every connected player is ready and players.min or more are connected; the countdown stops
        /// (back to the Lobby) once that is no longer true (a Not ready, a join, a disconnect, a leave); TopicVote ends
        /// at once when every connected player voted; Answer and RaceQuestion end at once when every connected player
        /// has locked in or reached their deadline plus grace (D73), else last call may cut the one remaining player in
        /// Answer (D74; the race has no last call).
        /// </summary>
        public void Reevaluate(List<IGameEvent> output)
        {
            long now = Now;
            if (_state.Phase == GamePhase.Lobby && EveryoneReady())
            {
                EndPhase(PhaseEndReasons.AllDone, now, output);
                BeginTimed(GamePhase.StartCountdown, now, ToMs(_config.Lobby.StartCountdownSeconds), 0, output);
                output.Add(new HostLineRequested(HostTriggers.StartCountdown));
                return;
            }

            if (_state.Phase == GamePhase.StartCountdown && !EveryoneReady())
            {
                CancelCountdown(now, output);
                return;
            }

            if (_state.Phase == GamePhase.TopicVote)
            {
                if (EveryConnectedPlayerVoted()) EndTopicVote(PhaseEndReasons.AllDone, now, output);
                return;
            }

            if (_state.Phase == GamePhase.Poll)
            {
                if (EveryConnectedPlayerSubmittedPoll())
                {
                    EndPhase(PhaseEndReasons.AllDone, now, output);
                    NextSegment(now, output);
                }

                return;
            }

            if (!InTurnPhase) return;
            RaiseTimeouts(now, output);
            if (TurnPhaseDueAt(out var reason) <= now)
            {
                EndTurnPhase(reason, now, output);
                return;
            }

            if (_state.Phase == GamePhase.Answer) CheckLastCall(now, output);
            _state.PhaseEndsAtMs = LatestDeadlinePlusGrace();
        }

        /// <summary>Ends every phase that is due by the clock, each at its scheduled time, and starts the next.</summary>
        public void Advance(List<IGameEvent> output)
        {
            long now = Now;
            while (true)
            {
                switch (_state.Phase)
                {
                    case GamePhase.Lobby:
                        return;
                    case GamePhase.Answer:
                    case GamePhase.RaceQuestion:
                        long due = TurnPhaseDueAt(out var reason);
                        RaiseTimeouts(Math.Min(now, due), output);
                        if (due > now) return;
                        EndTurnPhase(reason, due, output);
                        break;
                    default:
                        if (!_state.PhaseEndsAtMs.HasValue || _state.PhaseEndsAtMs.Value > now) return;
                        EndTimedPhase(_state.PhaseEndsAtMs.Value, output);
                        break;
                }
            }
        }

        /// <summary>Every connected player voted; false when nobody is connected (the vote then runs to its timer).</summary>
        private bool EveryConnectedPlayerVoted()
        {
            var round = _state.Round;
            bool any = false;
            foreach (var p in _state.Players)
            {
                if (!p.Connected) continue;
                any = true;
                if (!round.Votes.ContainsKey(p.PlayerId)) return false;
            }

            return any;
        }

        /// <summary>
        /// When Answer or RaceQuestion is done (D73): every connected player locked in (reason allDone, at the last lock
        /// in; at once when nobody is connected) or reached their deadline plus net.inputGraceMs (reason timeout, at the
        /// latest of those).
        /// </summary>
        private long TurnPhaseDueAt(out string reason)
        {
            long grace = _config.Net.InputGraceMs;
            long latestOpen = long.MinValue;
            long lastLock = _state.PhaseStartedAtMs;
            bool anyOpen = false;
            foreach (var p in _state.Players)
            {
                if (!p.Connected || p.Turn == null) continue;
                if (p.Turn.Locked)
                {
                    lastLock = Math.Max(lastLock, p.Turn.LockedAtMs ?? lastLock);
                }
                else
                {
                    anyOpen = true;
                    latestOpen = Math.Max(latestOpen, p.Turn.DeadlineMs + grace);
                }
            }

            if (!anyOpen && HasConnectedTurn())
            {
                reason = PhaseEndReasons.AllDone;
                return lastLock;
            }

            if (!anyOpen)
            {
                // Nobody is connected (for example the laptop lost the relay and every phone counts as away): there is
                // nobody to end the phase early, so it runs to the latest deadline plus grace of the players who have
                // not locked in. The core clock is paused while the relay is down, so the question waits.
                foreach (var p in _state.Players)
                {
                    if (p.Turn == null || p.Turn.Locked) continue;
                    anyOpen = true;
                    latestOpen = Math.Max(latestOpen, p.Turn.DeadlineMs + grace);
                }

                if (!anyOpen)
                {
                    reason = PhaseEndReasons.AllDone;
                    return lastLock;
                }
            }

            reason = PhaseEndReasons.Timeout;
            return latestOpen;
        }

        private bool HasConnectedTurn()
        {
            foreach (var p in _state.Players)
            {
                if (p.Connected && p.Turn != null) return true;
            }

            return false;
        }

        /// <summary>
        /// PlayerTimedOut, once per player per Answer or RaceQuestion phase, for every player (connected or not) who has
        /// not locked in and whose deadline plus net.inputGraceMs is at or before the given time; earliest first.
        /// </summary>
        private void RaiseTimeouts(long upTo, List<IGameEvent> output)
        {
            long grace = _config.Net.InputGraceMs;
            List<PlayerState> due = null;
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null || turn.Locked || turn.TimedOut || turn.DeadlineMs + grace > upTo) continue;
                (due ?? (due = new List<PlayerState>())).Add(p);
            }

            if (due == null) return;
            // Stable: equal deadlines keep the join order.
            foreach (var p in due.OrderBy(x => x.Turn.DeadlineMs))
            {
                p.Turn.TimedOut = true;
                output.Add(new PlayerTimedOut(p.PlayerId, _state.PhaseId));
            }
        }

        private void CheckLastCall(long now, List<IGameEvent> output)
        {
            PlayerState remaining = null;
            int open = 0;
            bool anyLocked = false;
            foreach (var p in _state.Players)
            {
                if (!p.Connected || p.Turn == null) continue;
                if (p.Turn.Locked)
                {
                    anyLocked = true;
                }
                else
                {
                    open++;
                    remaining = p;
                }
            }

            if (!_config.Timers.LastCallEnabled) return; // decisions/0020, D119
            if (open != 1 || !anyLocked || remaining.Turn.LastCall || now >= remaining.Turn.DeadlineMs) return;
            var turn = remaining.Turn;
            turn.LastCall = true;
            turn.DeadlineMs = Math.Min(turn.DeadlineMs, now + ToMs(_config.Timers.LastCall));
            _state.PhaseEndsAtMs = LatestDeadlinePlusGrace();
            output.Add(new LastCallStarted(remaining.PlayerId, _state.PhaseId, turn.DeadlineMs));
            output.Add(new HostLineRequested(HostTriggers.LastCall));
        }

        /// <summary>
        /// The latest moment the turn phase can end (D73): the latest deadline plus grace over the players who have not
        /// locked in, connected or not. When everyone has locked in it stays where it was (the phase ends at once).
        /// </summary>
        private long LatestDeadlinePlusGrace()
        {
            long? latest = null;
            foreach (var p in _state.Players)
            {
                if (p.Turn == null || p.Turn.Locked) continue;
                long end = p.Turn.DeadlineMs + _config.Net.InputGraceMs;
                if (!latest.HasValue || end > latest.Value) latest = end;
            }

            return latest ?? _state.PhaseEndsAtMs ?? _state.PhaseStartedAtMs;
        }

        // ---------------------------------------------------------------- phase changes

        private void EndTimedPhase(long at, List<IGameEvent> output)
        {
            var phase = _state.Phase;
            if (phase == GamePhase.TopicVote)
            {
                EndTopicVote(PhaseEndReasons.Timeout, at, output);
                return;
            }

            EndPhase(PhaseEndReasons.Timeout, at, output);
            switch (phase)
            {
                case GamePhase.StartCountdown:
                    StartGame(at, output);
                    break;
                case GamePhase.Intro:
                case GamePhase.RoundEnd:
                case GamePhase.FinalIntro:
                case GamePhase.Poll:
                    NextSegment(at, output);
                    break;
                case GamePhase.Results:
                    FinishGame(at, output);
                    NextSegment(at, output);
                    break;
                case GamePhase.QuestionShow:
                    BeginAnswer(at, output);
                    break;
                case GamePhase.Reveal:
                    BeginScores(at, output);
                    break;
                case GamePhase.Scores:
                    _closedPickPhaseId = _state.PhaseId;
                    if (_questionIndex + 1 < _state.QuestionCount) BeginQuestionShow(at, output);
                    else BeginRoundEnd(at, output);
                    break;
                case GamePhase.RaceReveal:
                    if (_state.Race.Finished) NextSegment(at, output);
                    else BeginRaceQuestion(at, output);
                    break;
            }
        }

        private void EndTurnPhase(string reason, long at, List<IGameEvent> output)
        {
            var phase = _state.Phase;
            if (_state.Question != null && phase == GamePhase.Answer) _state.Question.AnswerClosed = true;
            _closedInputPhaseId = _state.PhaseId;
            EndPhase(reason, at, output);
            if (phase == GamePhase.Answer) BeginReveal(at, output);
            else BeginRaceReveal(at, output);
        }

        private void CancelCountdown(long at, List<IGameEvent> output)
        {
            EndPhase(PhaseEndReasons.Cancelled, at, output);
            BeginUntimed(GamePhase.Lobby, at, output);
        }

        private void StartGame(long at, List<IGameEvent> output)
        {
            _state.GameNumber++;
            _state.Seed = _newSeed();
            _random = new SeededRandom(_state.Seed);
            _state.GameStartedAtMs = at;
            _state.GameRunning = true;
            // The TV settings panel's power up settings apply from the next game start (docs/02_show_flow.md).
            _state.Settings = _state.PendingSettings.Clone();
            _state.QuestionCount = _config.Rounds.QuestionsPerRound;
            _state.Question = null;
            _state.Round = null;
            _state.Race = null;
            _state.WinnerId = null;
            _state.MutablePlacement.Clear();
            _state.MutableMostCorrectIds.Clear();
            _state.MutableBestComebackIds.Clear();
            _state.MutableRanksAfterRound2.Clear();
            _usedInGame.Clear();
            _usedRaceInGame.Clear();
            _chosenTopics.Clear();
            _roundIndex = -1;
            _questionIndex = -1;

            // The Standard script; the next game from the Lobby starts at the first round (no intro, docs/15_schemas.md).
            _segments.Clear();
            foreach (var segment in _content.Scripts.Mode(ShowScripts.Standard))
            {
                if (segment == ShowSegments.Intro && _state.GameNumber > 1) continue;
                _segments.Add(segment);
            }

            _segmentIndex = -1;
            _state.RoundCount = _segments.Count(s => s == ShowSegments.Round);

            var ids = new List<string>();
            foreach (var p in _state.Players)
            {
                // Scores were reset when the last game ended (D76); a game always starts from 0.
                p.Score = 0;
                p.Streak = 0;
                p.CorrectCount = 0;
                p.Stats = new PlayerStats();
                p.Turn = null;
                p.CharacterPicked = true;
                p.Ready = false;
                ClearPowerUps(p);
                ids.Add(p.PlayerId);
            }

            output.Add(new GameStarted(_state.GameNumber, _state.Seed, GameStarted.ModeStandard, ids, _state.GameLanguage,
                _state.Settings.DrawableTypes, _state.Settings.Targeting));
            foreach (var p in _state.Players) output.Add(new PlayerUpdated(p.PlayerId));
            NextSegment(at, output);
        }

        /// <summary>Starts the next segment of the show script, or ends the game after the last one.</summary>
        private void NextSegment(long at, List<IGameEvent> output)
        {
            while (true)
            {
                _segmentIndex++;
                if (_segmentIndex >= _segments.Count)
                {
                    if (_state.GameRunning) FinishGame(at, output);
                    ReturnToLobby(at, output);
                    return;
                }

                switch (_segments[_segmentIndex])
                {
                    case ShowSegments.Intro:
                        BeginTimed(GamePhase.Intro, at, ToMs(_config.Timers.Intro), 0, output);
                        output.Add(new HostLineRequested(HostTriggers.GameStart));
                        return;
                    case ShowSegments.Round:
                        BeginTopicVote(at, output);
                        return;
                    case ShowSegments.FinalIntro:
                    {
                        bool isNew = SetUpRace();
                        BeginTimed(GamePhase.FinalIntro, at, ToMs(_config.Timers.FinalIntro), 0, output);
                        if (isNew) RaiseRaceStarted(output);
                        output.Add(new HostLineRequested(HostTriggers.FinalStart));
                        return;
                    }
                    case ShowSegments.FinalRace:
                    {
                        bool isNew = SetUpRace();
                        if (isNew) RaiseRaceStarted(output);
                        BeginRaceQuestion(at, output);
                        return;
                    }
                    case ShowSegments.Results:
                        BeginResults(at, output);
                        return;
                    case ShowSegments.Poll:
                        // The poll comes after Results; the game itself already ended. Off in config (or no
                        // questions): passed over.
                        if (!_config.Poll.Enabled || _content.Poll.Count == 0) continue;
                        if (_state.GameRunning) FinishGame(at, output);
                        BeginPoll(at, output);
                        return;
                    default:
                        // miniGame (after game night 1) is not built yet: passed over.
                        continue;
                }
            }
        }

        // ---------------------------------------------------------------- rounds

        private void BeginTopicVote(long at, List<IGameEvent> output)
        {
            _roundIndex++;
            _questionIndex = -1;
            _state.Question = null;
            foreach (var p in _state.Players) p.Turn = null;
            var doors = TopicPicker.Doors(Bank.Topics, Bank.Questions, _config.Topics, _chosenTopics,
                _state.SeenQuestionIds, _random);
            var round = new RoundState(_roundIndex, _config.Rounds.Multipliers[_roundIndex], doors);
            _state.Round = round;
            // Votes get the same network grace as answers (D84): the timer shows timers.topicVote, and the phase waits
            // net.inputGraceMs past it unless everyone has voted.
            long vote = ToMs(_config.Timers.TopicVote);
            round.VoteDeadlineMs = at + vote;
            _state.Phase = GamePhase.TopicVote;
            _state.PhaseId++;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = vote;
            _state.PhaseStagingExtraMs = 0;
            _state.PhaseEndsAtMs = round.VoteDeadlineMs + _config.Net.InputGraceMs;
            output.Add(new PhaseStarted(GamePhase.TopicVote, _state.PhaseId, vote, 0, _state.PhaseEndsAtMs));
            output.Add(new HostLineRequested(HostTriggers.RoundStart));
        }

        private void EndTopicVote(string reason, long at, List<IGameEvent> output)
        {
            var round = _state.Round;
            // Votes of players who left the game no longer count.
            var votes = round.Votes.Where(v => _state.FindById(v.Key) != null).ToDictionary(v => v.Key, v => v.Value);
            var counts = new int[round.Doors.Count];
            foreach (var d in votes.Values) counts[d]++;
            int most = counts.Length > 0 ? counts.Max() : 0;
            bool nobodyVoted = most == 0;
            bool tie = !nobodyVoted && counts.Count(c => c == most) > 1;

            int door = TopicPicker.ResolveVote(votes, round.Doors.Count, RankOf, _random);
            round.ChosenDoor = door;
            foreach (var v in votes)
            {
                if (v.Value != door) _state.FindById(v.Key).Stats.LostVotes++;
            }

            _chosenTopics.Add(round.Topic.Id);
            output.Add(new TopicChosen(round.Index + 1, door, round.Topic.Id, tie, nobodyVoted));

            _closedInputPhaseId = _state.PhaseId;
            EndPhase(reason, at, output);
            BeginQuestionShow(at, output);
        }

        private int RankOf(string playerId)
        {
            var player = _state.FindById(playerId);
            return player == null ? int.MaxValue : Ranking.SharedRank(_state, player);
        }

        private void BeginQuestionShow(long at, List<IGameEvent> output)
        {
            _questionIndex++;
            var rounds = _config.Rounds;
            var round = _state.Round;
            int slot = _questionIndex;
            var topic = round.Topic;
            string topicId = topic.Mixed ? null : topic.Id;
            var def = QuestionPicker.Pick(Bank.Questions, rounds.FormatOrder[slot], rounds.DifficultyOrder[slot], topicId,
                _state.SeenQuestionIds, _usedInGame, _random, out int group);
            if (group != QuestionPicker.GroupUnseenTopic)
                output.Add(new QuestionFallback(round.Index + 1, slot + 1, topic.Id, def.Id, group));
            _usedInGame.Add(def.Id);
            _state.MutableSeenQuestionIds.Add(def.Id);
            output.Add(new SeenChanged(false));

            var order = def.Format == QuestionDef.MultipleChoice ? OptionShuffle.DisplayOrder(def.Options.Count, _random) : new int[0];
            int number = round.Index * rounds.QuestionsPerRound + slot + 1;
            _state.Question = new QuestionInPlay(def, number, slot, round.Index, round.Multiplier, order) { PickGroup = group };
            foreach (var p in _state.Players) p.Turn = null;

            long extra = ToMs(_config.Staging.ExtraSeconds.QuestionShow);
            BeginTimed(GamePhase.QuestionShow, at, ToMs(_config.Timers.QuestionShow) + extra, extra, output);
        }

        private void BeginAnswer(long at, List<IGameEvent> output)
        {
            var question = _state.Question;
            double seconds = question.IsClosestValue ? _config.Timers.Answer.ClosestValue : _config.Timers.Answer.MultipleChoice;
            long baseWindow = ToMs(seconds);
            question.AnswerStartedAtMs = at;
            question.BaseWindowMs = baseWindow;
            BeginTurnPhase(GamePhase.Answer, at, seconds, output);
            question.AnswerPhaseId = _state.PhaseId;
            ApplyEffects(question, output);
        }

        /// <summary>Answer or RaceQuestion: a fresh turn per player with their personal window and deadline.</summary>
        private void BeginTurnPhase(GamePhase phase, long at, double windowSeconds, List<IGameEvent> output)
        {
            long baseWindow = ToMs(windowSeconds);
            _state.Phase = phase;
            _state.PhaseId++;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = baseWindow;
            _state.PhaseStagingExtraMs = 0;
            foreach (var p in _state.Players)
            {
                long window = (long)Math.Round(windowSeconds * 1000.0 * p.TimerMultiplier, MidpointRounding.AwayFromZero);
                p.Turn = new PlayerTurn
                {
                    WindowMs = window,
                    DeadlineMs = at + window,
                    RankBefore = Ranking.SharedRank(_state, p)
                };
            }

            _state.PhaseEndsAtMs = LatestDeadlinePlusGrace();
            output.Add(new PhaseStarted(phase, _state.PhaseId, baseWindow, 0, _state.PhaseEndsAtMs));
        }

        private void BeginReveal(long at, List<IGameEvent> output)
        {
            long extra = ToMs(_config.Staging.ExtraSeconds.Reveal);
            BeginTimed(GamePhase.Reveal, at, ToMs(_config.Timers.Reveal) + extra, extra, output);

            var question = _state.Question;
            if (question.IsClosestValue) ScoreClosestValue(question);
            else ScoreMultipleChoice(question);

            var deltas = new Dictionary<string, int>();
            var roundPoints = _state.Round.MutablePoints;
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null) continue;
                turn.Revealed = true;
                p.Score += turn.Points;
                p.Streak = turn.Correct ? p.Streak + 1 : 0;
                if (turn.Correct) p.CorrectCount++;
                CountStats(p, turn);
                if (turn.LastCall) p.Stats.LastCalls++;
                if (question.IsClosestValue && turn.Distance.HasValue && question.Def.Tolerance > 0)
                    p.Stats.WorstMiss = Math.Max(p.Stats.WorstMiss, turn.Distance.Value / question.Def.Tolerance.Value);
                roundPoints[p.PlayerId] = (roundPoints.TryGetValue(p.PlayerId, out var sum) ? sum : 0) + turn.Points;
                deltas[p.PlayerId] = turn.Points;
            }

            output.Add(new Revealed(_state.PhaseId, question.Def.Id, question.CorrectDisplayIndex));
            foreach (var p in _state.Players)
            {
                if (deltas.TryGetValue(p.PlayerId, out var delta) && delta != 0)
                    output.Add(new ScoreChanged(p.PlayerId, delta, p.Score, Ranking.SharedRank(_state, p)));
            }

            DrawPowerUps(question, output);
            output.AddRange(RevealTriggers.Fired(_state, _config, _random));
            foreach (var p in _state.Players) output.Add(new PlayerUpdated(p.PlayerId));
        }

        private void ScoreMultipleChoice(QuestionInPlay question)
        {
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null) continue;
                bool correct = turn.Locked && turn.Choice == question.CorrectDisplayIndex;
                turn.Correct = correct;
                turn.Points = correct
                    ? Scoring.MultipleChoice(_config.Scoring, true, turn.Ms ?? 0, turn.WindowMs, question.Multiplier)
                    : 0;
            }
        }

        /// <summary>
        /// Distance points per guess, plus closestValue.closestBonus times the multiplier for the closest guess or guesses
        /// (ties: all of them), even when their distance score is 0. No answer: 0 and no bonus (docs/03_rules.md).
        /// </summary>
        private void ScoreClosestValue(QuestionInPlay question)
        {
            var def = question.Def;
            double? best = null;
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null || !turn.Value.HasValue) continue;
                turn.Distance = Scoring.Distance(turn.Value.Value, def.Answer.Value);
                if (!best.HasValue || turn.Distance.Value < best.Value) best = turn.Distance.Value;
            }

            int bonus = Scoring.ClosestBonus(_config.ClosestValue, question.Multiplier);
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null) continue;
                if (!turn.Value.HasValue)
                {
                    turn.Correct = false;
                    turn.Points = 0;
                    continue;
                }

                int points = Scoring.ClosestValue(_config.Scoring, _config.ClosestValue, turn.Value.Value, def.Answer.Value,
                    def.Tolerance.Value, question.Multiplier);
                turn.Correct = Scoring.ClosestValueCorrect(turn.Value.Value, def.Answer.Value, def.Tolerance.Value);
                turn.Closest = turn.Distance.Value == best.Value;
                turn.Bonus = turn.Closest ? bonus : 0;
                turn.Points = points + turn.Bonus;
            }
        }

        private void BeginScores(long at, List<IGameEvent> output)
        {
            // Scores keeps Reveal's phaseId (D28): the power up pick spans both.
            long extra = ToMs(_config.Staging.ExtraSeconds.Scores);
            long budget = ToMs(_config.Timers.Scores) + extra;
            _state.Phase = GamePhase.Scores;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = budget;
            _state.PhaseStagingExtraMs = extra;
            _state.PhaseEndsAtMs = at + budget;
            output.Add(new PhaseStarted(GamePhase.Scores, _state.PhaseId, budget, extra, _state.PhaseEndsAtMs));
        }

        private void BeginRoundEnd(long at, List<IGameEvent> output)
        {
            _state.Question = null;
            foreach (var p in _state.Players) p.Turn = null;
            // "Best comeback" counts from the end of round 2 (docs/03_rules.md).
            if (_roundIndex == 1)
            {
                _state.MutableRanksAfterRound2.Clear();
                foreach (var p in _state.Players) _state.MutableRanksAfterRound2[p.PlayerId] = Ranking.SharedRank(_state, p);
            }

            // Unused power ups are lost at the end of the last round (docs/03_rules.md, "Using").
            if (_roundIndex >= _state.RoundCount - 1)
            {
                foreach (var p in _state.Players)
                {
                    if (p.HeldPowerUp == null && p.PendingEffects.Count == 0) continue;
                    ClearPowerUps(p);
                    output.Add(new PlayerUpdated(p.PlayerId));
                }
            }

            long extra = ToMs(_config.Staging.ExtraSeconds.RoundEnd);
            BeginTimed(GamePhase.RoundEnd, at, ToMs(_config.Timers.RoundEnd) + extra, extra, output);
            output.Add(new HostLineRequested(HostTriggers.RoundEnd));
        }

        // ---------------------------------------------------------------- the final race

        /// <summary>Creates the race with head starts by shared rank; false when it already exists.</summary>
        private bool SetUpRace()
        {
            if (_state.Race != null) return false;
            var race = new RaceState();
            var cfg = _config.Race;
            int players = _state.Players.Count;
            foreach (var p in _state.Players)
            {
                int start = RaceRules.HeadStart(Ranking.SharedRank(_state, p), players, cfg.MaxHeadStart);
                race.MutableHeadStarts[p.PlayerId] = start;
                race.MutablePositions[p.PlayerId] = start;
                race.MutableScoresBefore[p.PlayerId] = p.Score;
                race.MutableAnswerMs[p.PlayerId] = 0;
            }

            race.LastPlaceCanWin = RaceRules.LastPlaceCanWin(race.HeadStarts.Values, cfg);
            _state.Round = null;
            _state.Race = race;
            return true;
        }

        private void RaiseRaceStarted(List<IGameEvent> output)
        {
            var race = _state.Race;
            output.Add(new RaceStarted(new Dictionary<string, int>(race.MutableHeadStarts), race.LastPlaceCanWin));
        }

        private void BeginRaceQuestion(long at, List<IGameEvent> output)
        {
            var race = _state.Race;
            var statement = RaceRules.PickStatement(_content.Race, _state.SeenRaceIds, _usedRaceInGame, _random);
            race.StatementNumber++;
            race.Statement = statement;
            _usedRaceInGame.Add(statement.Id);
            _state.MutableSeenRaceIds.Add(statement.Id);
            output.Add(new SeenChanged(false));
            BeginTurnPhase(GamePhase.RaceQuestion, at, _config.Race.Answer, output);
        }

        private void BeginRaceReveal(long at, List<IGameEvent> output)
        {
            BeginTimed(GamePhase.RaceReveal, at, ToMs(_config.Race.Reveal), 0, output);
            var race = _state.Race;
            var cfg = _config.Race;
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null) continue;
                bool correct = turn.Locked && turn.TrueFalse == race.Statement.Answer;
                turn.Correct = correct;
                turn.Revealed = true;
                // Total answer time (second race tie break): the answer's ms, or the whole window without an answer.
                long ms = turn.Locked ? turn.Ms ?? turn.WindowMs : turn.WindowMs;
                race.MutableAnswerMs[p.PlayerId] = (race.AnswerMs.TryGetValue(p.PlayerId, out var sum) ? sum : 0) + ms;
                int from = race.PositionOf(p.PlayerId);
                race.MutablePositions[p.PlayerId] = RaceRules.Move(from, correct, cfg);
                p.Stats.RaceSteps += race.PositionOf(p.PlayerId) - from;
                CountStats(p, turn);
            }

            var seated = _state.Players.Select(p => p.PlayerId).ToList();
            race.Finished = RaceRules.IsFinished(seated.Select(race.PositionOf), race.StatementNumber, cfg);
            if (race.Finished)
            {
                race.MutablePlacement.Clear();
                race.MutablePlacement.AddRange(RaceRules.Order(seated, race.Positions, race.ScoresBefore, race.AnswerMs, _random));
            }

            output.Add(new RaceRevealed(_state.PhaseId, race.Statement.Id, race.Statement.Answer, race.Finished));
        }

        // ---------------------------------------------------------------- results

        private void BeginResults(long at, List<IGameEvent> output)
        {
            _state.Question = null;
            _state.Round = null;
            foreach (var p in _state.Players) p.Turn = null;

            var placement = _state.MutablePlacement;
            placement.Clear();
            var race = _state.Race;
            if (race != null && race.Finished)
            {
                placement.AddRange(race.Placement.Where(id => _state.FindById(id) != null));
            }
            else
            {
                // No race in this script: by score, ties in seeded random order.
                var ids = _state.Players.Select(p => p.PlayerId).ToList();
                var shuffled = ids.Count < 2 ? ids : OptionShuffle.DisplayOrder(ids.Count, _random).Select(i => ids[i]).ToList();
                placement.AddRange(shuffled.OrderByDescending(id => _state.FindById(id).Score));
            }

            _state.WinnerId = placement.Count > 0 ? placement[0] : null;
            _state.MutableMostCorrectIds.Clear();
            _state.MutableMostCorrectIds.AddRange(Awards.MostCorrect(_state.Players));
            _state.MutableBestComebackIds.Clear();
            _state.MutableBestComebackIds.AddRange(Awards.BestComeback(_state.RanksAfterRound2, placement));
            _state.MutableTags.Clear();
            foreach (var pair in Tags.Assign(_state.Players, _state.WinnerId, _random)) _state.MutableTags[pair.Key] = pair.Value;
            _state.WinnerFact = _state.WinnerId != null ? FactPicker.Pick(_content.Facts, _state.SeenFactIds, _random) : null;
            if (_state.WinnerFact != null)
            {
                _state.MutableSeenFactIds.Add(_state.WinnerFact.Id);
                output.Add(new SeenChanged(false));
            }

            BeginTimed(GamePhase.Results, at, ToMs(_config.Timers.Results), 0, output);
            var winner = _state.WinnerId != null ? _state.FindById(_state.WinnerId) : null;
            if (winner != null) output.Add(new HostLineRequested(HostTriggers.RaceWinner, winner.Name));
        }

        /// <summary>One revealed answer (question or race statement) in the player's stats for the funny tags (D86).</summary>
        private void CountStats(PlayerState p, PlayerTurn turn)
        {
            var stats = p.Stats;
            if (!turn.Locked)
            {
                stats.Unanswered++;
                return;
            }

            stats.Answered++;
            long ms = Scoring.ClampMs(turn.Ms ?? turn.WindowMs, turn.WindowMs);
            if (turn.WindowMs > 0) stats.WindowShareSum += (double)ms / turn.WindowMs;
            if (turn.Correct) return;
            stats.Wrong++;
            if (ms < _config.Scoring.FastAnswerMs) stats.HastyWrong++;
        }

        /// <summary>The game is over at the end of Results (log gameEnded, reason finished); the Poll may follow.</summary>
        private void FinishGame(long at, List<IGameEvent> output)
        {
            _state.GameRunning = false;
            output.Add(new GameEnded(_state.GameNumber, GameEnded.ReasonFinished, _state.WinnerId, at - _state.GameStartedAtMs));
        }

        /// <summary>
        /// Back to the Lobby (D76), after the Poll, after Results with the poll off, or by Back to lobby: scores, streaks,
        /// correct counts and power ups go to 0 and ready marks are cleared; seats, characters, languages and timer
        /// multipliers stay.
        /// </summary>
        private void ReturnToLobby(long at, List<IGameEvent> output)
        {
            _state.Question = null;
            _state.Round = null;
            _state.MutablePollSubmitted.Clear();
            foreach (var p in _state.Players)
            {
                p.Score = 0;
                p.Streak = 0;
                p.CorrectCount = 0;
                p.Turn = null;
                p.Ready = false;
                ClearPowerUps(p);
                output.Add(new PlayerUpdated(p.PlayerId));
            }

            BeginUntimed(GamePhase.Lobby, at, output);
        }

        private static void ClearPowerUps(PlayerState p)
        {
            p.HeldPowerUp = null;
            p.MutablePendingEffects.Clear();
            p.LastHitQuestion = null;
            p.UsedPowerUpInPhaseId = 0;
            p.UsedPowerUp = null;
            p.UsedPowerUpTarget = null;
        }

        private void EndPhase(string reason, long at, List<IGameEvent> output)
        {
            output.Add(new PhaseEnded(_state.Phase, _state.PhaseId, reason, at - _state.PhaseStartedAtMs));
        }

        private void BeginTimed(GamePhase phase, long at, long budgetMs, long stagingExtraMs, List<IGameEvent> output)
        {
            _state.Phase = phase;
            _state.PhaseId++;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = budgetMs;
            _state.PhaseStagingExtraMs = stagingExtraMs;
            _state.PhaseEndsAtMs = at + budgetMs;
            output.Add(new PhaseStarted(phase, _state.PhaseId, budgetMs, stagingExtraMs, _state.PhaseEndsAtMs));
        }

        private void BeginUntimed(GamePhase phase, long at, List<IGameEvent> output)
        {
            _state.Phase = phase;
            _state.PhaseId++;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = 0;
            _state.PhaseStagingExtraMs = 0;
            _state.PhaseEndsAtMs = null;
            output.Add(new PhaseStarted(phase, _state.PhaseId));
        }
    }
}
