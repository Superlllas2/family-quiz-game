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
    /// Runs the show on the injected clock (docs/02_show_flow.md, Day 2 scope from D75): Lobby, StartCountdown, then
    /// GameState.QuestionCount x (QuestionShow, Answer, Reveal, Scores), Results, then the Lobby again (D76). Every duration comes from
    /// config, with staging.extraSeconds added where a phase has one. A phase that ends on its timer ends at its
    /// scheduled time and the next one starts at that same time, so on a fake clock every phase lasts its budget exactly.
    /// </summary>
    public sealed class ShowDirector
    {
        private readonly GameState _state;
        private readonly GameConfig _config;
        private readonly GameContent _content;
        private readonly IClock _clock;
        private readonly Func<int> _newSeed;
        private readonly HashSet<string> _usedInGame = new HashSet<string>();
        private IRandom _random;
        private int _questionIndex;

        /// <summary>phaseId of the Answer phase that closed last: an input naming it gets "closed", not "stale".</summary>
        private long _closedAnswerPhaseId;

        public ShowDirector(GameState state, GameContent content, IClock clock, Func<int> newSeed)
        {
            _state = state;
            _content = content;
            _config = content.Config;
            _clock = clock;
            _newSeed = newSeed;
        }

        private long Now => _clock.NowMs;

        /// <summary>Seconds from config to whole milliseconds.</summary>
        public static long ToMs(double seconds)
        {
            return (long)Math.Round(seconds * 1000.0, MidpointRounding.AwayFromZero);
        }

        public void OpenLobby(List<IGameEvent> output)
        {
            BeginUntimed(GamePhase.Lobby, Now, output);
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

        // ---------------------------------------------------------------- answers

        /// <summary>
        /// Why a choice input is refused, or null when it is accepted. Order: wrong phaseId (stale, or closed for the
        /// Answer phase that just closed), a phase or kind that takes no choice (invalid), no turn or already locked
        /// (invalid), a bad choice or no ms (invalid), at or after the deadline plus net.inputGraceMs (closed, D73).
        /// </summary>
        public string CheckChoice(PlayerState player, SubmitAnswer cmd)
        {
            if (cmd.PhaseId != _state.PhaseId)
                return _closedAnswerPhaseId != 0 && cmd.PhaseId == _closedAnswerPhaseId ? AckReasons.Closed : AckReasons.Stale;
            if (_state.Phase != GamePhase.Answer || cmd.Kind != InputKinds.Choice) return AckReasons.Invalid;
            var turn = player.Turn;
            if (turn == null || turn.Locked) return AckReasons.Invalid;
            int options = _state.Question.DisplayOrder.Length;
            if (!cmd.Choice.HasValue || cmd.Choice.Value < 0 || cmd.Choice.Value >= options || !cmd.Ms.HasValue)
                return AckReasons.Invalid;
            if (Now >= turn.DeadlineMs + _config.Net.InputGraceMs) return AckReasons.Closed;
            return null;
        }

        /// <summary>Locks a choice that <see cref="CheckChoice"/> accepted. The end check runs after the command.</summary>
        public void LockChoice(PlayerState player, SubmitAnswer cmd, List<IGameEvent> output)
        {
            var turn = player.Turn;
            var question = _state.Question;
            int choice = cmd.Choice.Value;
            turn.InputId = cmd.InputId;
            turn.Choice = choice;
            turn.RawMs = cmd.Ms.Value;
            turn.Ms = Scoring.ClampMs(cmd.Ms.Value, turn.WindowMs);
            turn.LockedAtMs = Now;
            bool correct = choice == question.CorrectDisplayIndex;
            int points = Scoring.MultipleChoice(_config.Scoring, correct, turn.Ms.Value, turn.WindowMs, question.Multiplier);
            output.Add(new AnswerLocked(player.PlayerId, _state.PhaseId, InputKinds.Choice, choice, turn.Ms.Value, correct, points));
        }

        // ---------------------------------------------------------------- the clock

        /// <summary>
        /// Rules that react to a changed state, run after every command and every tick (D76): the Lobby starts the
        /// countdown once every connected player is ready and players.min or more are connected; the countdown stops
        /// (back to the Lobby) once that is no longer true (a Not ready, a join, a disconnect, a leave); in Answer, the phase ends at once when every connected player has locked in
        /// or reached their deadline plus grace (D73), else last call may cut the one remaining player (D74).
        /// </summary>
        public void Reevaluate(List<IGameEvent> output)
        {
            long now = Now;
            if (_state.Phase == GamePhase.Lobby && EveryoneReady())
            {
                EndPhase(PhaseEndReasons.AllDone, now, output);
                BeginTimed(GamePhase.StartCountdown, now, ToMs(_config.Lobby.StartCountdownSeconds), 0, output);
                return;
            }

            if (_state.Phase == GamePhase.StartCountdown && !EveryoneReady())
            {
                CancelCountdown(now, output);
                return;
            }

            if (_state.Phase != GamePhase.Answer) return;
            RaiseTimeouts(now, output);
            if (AnswerDueAt(out var reason) <= now)
            {
                EndAnswer(reason, now, output);
                return;
            }

            CheckLastCall(now, output);
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
                        long due = AnswerDueAt(out var reason);
                        RaiseTimeouts(Math.Min(now, due), output);
                        if (due > now) return;
                        EndAnswer(reason, due, output);
                        break;
                    default:
                        if (!_state.PhaseEndsAtMs.HasValue || _state.PhaseEndsAtMs.Value > now) return;
                        EndTimedPhase(_state.PhaseEndsAtMs.Value, output);
                        break;
                }
            }
        }

        /// <summary>
        /// When Answer is done (D73): every connected player locked in (reason allDone, at the last lock in; at once when
        /// nobody is connected) or reached their deadline plus net.inputGraceMs (reason timeout, at the latest of those).
        /// </summary>
        private long AnswerDueAt(out string reason)
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
        /// PlayerTimedOut, once per player per Answer phase, for every player (connected or not) who has not locked in
        /// and whose deadline plus net.inputGraceMs is at or before the given time; earliest first.
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

            if (open != 1 || !anyLocked || remaining.Turn.LastCall || now >= remaining.Turn.DeadlineMs) return;
            var turn = remaining.Turn;
            turn.LastCall = true;
            turn.DeadlineMs = Math.Min(turn.DeadlineMs, now + ToMs(_config.Timers.LastCall));
            _state.PhaseEndsAtMs = LatestDeadlinePlusGrace();
            output.Add(new LastCallStarted(remaining.PlayerId, _state.PhaseId, turn.DeadlineMs));
        }

        /// <summary>
        /// The latest moment the Answer can end (D73): the latest deadline plus grace over the players who have not
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
            EndPhase(PhaseEndReasons.Timeout, at, output);
            switch (phase)
            {
                case GamePhase.StartCountdown:
                    StartGame(at, output);
                    break;
                case GamePhase.QuestionShow:
                    BeginAnswer(at, output);
                    break;
                case GamePhase.Reveal:
                    BeginScores(at, output);
                    break;
                case GamePhase.Scores:
                    if (_questionIndex + 1 < _state.QuestionCount) BeginQuestionShow(at, output);
                    else BeginResults(at, output);
                    break;
                case GamePhase.Results:
                    EndGame(at, output);
                    break;
            }
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
            _state.QuestionCount = _config.Rounds.QuestionsPerRound;
            _state.Question = null;
            _state.WinnerId = null;
            _state.MutableMostCorrectIds.Clear();
            _usedInGame.Clear();
            _questionIndex = -1;

            var ids = new List<string>();
            foreach (var p in _state.Players)
            {
                // Scores were reset when the last game ended (D76); a game always starts from 0.
                p.Score = 0;
                p.Streak = 0;
                p.CorrectCount = 0;
                p.Turn = null;
                p.CharacterPicked = true;
                p.Ready = false;
                ids.Add(p.PlayerId);
            }

            output.Add(new GameStarted(_state.GameNumber, _state.Seed, GameStarted.ModeStandard, ids, _state.GameLanguage));
            foreach (var p in _state.Players) output.Add(new PlayerUpdated(p.PlayerId));
            BeginQuestionShow(at, output);
        }

        private void BeginQuestionShow(long at, List<IGameEvent> output)
        {
            _questionIndex++;
            var rounds = _config.Rounds;
            int slot = _questionIndex % rounds.QuestionsPerRound;
            int roundIndex = _questionIndex / rounds.QuestionsPerRound;
            var def = QuestionPicker.Pick(_content.Questions, rounds, slot, _state.SeenQuestionIds, _usedInGame, _random);
            _usedInGame.Add(def.Id);
            _state.MutableSeenQuestionIds.Add(def.Id);
            var order = OptionShuffle.DisplayOrder(def.Options.Count, _random);
            _state.Question = new QuestionInPlay(def, _questionIndex + 1, slot, roundIndex, rounds.Multipliers[roundIndex], order);
            foreach (var p in _state.Players) p.Turn = null;

            long extra = ToMs(_config.Staging.ExtraSeconds.QuestionShow);
            BeginTimed(GamePhase.QuestionShow, at, ToMs(_config.Timers.QuestionShow) + extra, extra, output);
        }

        private void BeginAnswer(long at, List<IGameEvent> output)
        {
            var question = _state.Question;
            long baseWindow = ToMs(_config.Timers.Answer.MultipleChoice);
            _state.Phase = GamePhase.Answer;
            _state.PhaseId++;
            _state.PhaseStartedAtMs = at;
            _state.PhaseBudgetMs = baseWindow;
            _state.PhaseStagingExtraMs = 0;
            question.AnswerPhaseId = _state.PhaseId;
            question.AnswerStartedAtMs = at;
            question.BaseWindowMs = baseWindow;
            foreach (var p in _state.Players)
            {
                long window = (long)Math.Round(_config.Timers.Answer.MultipleChoice * 1000.0 * p.TimerMultiplier,
                    MidpointRounding.AwayFromZero);
                p.Turn = new PlayerTurn
                {
                    WindowMs = window,
                    DeadlineMs = at + window,
                    RankBefore = Ranking.SharedRank(_state, p)
                };
            }

            _state.PhaseEndsAtMs = LatestDeadlinePlusGrace();
            output.Add(new PhaseStarted(GamePhase.Answer, _state.PhaseId, baseWindow, 0, _state.PhaseEndsAtMs));
        }

        private void EndAnswer(string reason, long at, List<IGameEvent> output)
        {
            _state.Question.AnswerClosed = true;
            _closedAnswerPhaseId = _state.PhaseId;
            EndPhase(reason, at, output);
            BeginReveal(at, output);
        }

        private void BeginReveal(long at, List<IGameEvent> output)
        {
            long extra = ToMs(_config.Staging.ExtraSeconds.Reveal);
            BeginTimed(GamePhase.Reveal, at, ToMs(_config.Timers.Reveal) + extra, extra, output);

            var question = _state.Question;
            var deltas = new Dictionary<string, int>();
            foreach (var p in _state.Players)
            {
                var turn = p.Turn;
                if (turn == null) continue;
                bool correct = turn.Locked && turn.Choice.Value == question.CorrectDisplayIndex;
                int points = correct
                    ? Scoring.MultipleChoice(_config.Scoring, true, turn.Ms ?? 0, turn.WindowMs, question.Multiplier)
                    : 0;
                turn.Correct = correct;
                turn.Points = points;
                turn.Revealed = true;
                p.Score += points;
                p.Streak = correct ? p.Streak + 1 : 0;
                if (correct) p.CorrectCount++;
                deltas[p.PlayerId] = points;
            }

            output.Add(new Revealed(_state.PhaseId, question.Def.Id, question.CorrectDisplayIndex));
            foreach (var p in _state.Players)
            {
                if (deltas.TryGetValue(p.PlayerId, out var delta) && delta != 0)
                    output.Add(new ScoreChanged(p.PlayerId, delta, p.Score, Ranking.SharedRank(_state, p)));
            }

            foreach (var p in _state.Players) output.Add(new PlayerUpdated(p.PlayerId));
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

        private void BeginResults(long at, List<IGameEvent> output)
        {
            _state.Question = null;
            foreach (var p in _state.Players) p.Turn = null;
            _state.WinnerId = PickWinner();
            _state.MutableMostCorrectIds.Clear();
            _state.MutableMostCorrectIds.AddRange(MostCorrect());
            BeginTimed(GamePhase.Results, at, ToMs(_config.Timers.Results), 0, output);
        }

        private void EndGame(long at, List<IGameEvent> output)
        {
            _state.GameRunning = false;
            output.Add(new GameEnded(_state.GameNumber, GameEnded.ReasonFinished, _state.WinnerId, at - _state.GameStartedAtMs));

            // Back to the Lobby (D76): scores, streaks and correct counts go to 0; seats, characters, languages and
            // timer multipliers stay. Ready marks were cleared when the game started.
            foreach (var p in _state.Players)
            {
                p.Score = 0;
                p.Streak = 0;
                p.CorrectCount = 0;
                p.Turn = null;
                p.Ready = false;
                output.Add(new PlayerUpdated(p.PlayerId));
            }

            BeginUntimed(GamePhase.Lobby, at, output);
        }

        /// <summary>Day 2 winner (no race yet): the highest score; ties broken by the game's seeded random.</summary>
        private string PickWinner()
        {
            var best = new List<PlayerState>();
            foreach (var p in _state.Players)
            {
                if (best.Count == 0 || p.Score > best[0].Score)
                {
                    best.Clear();
                    best.Add(p);
                }
                else if (p.Score == best[0].Score)
                {
                    best.Add(p);
                }
            }

            if (best.Count == 0) return null;
            return best.Count == 1 ? best[0].PlayerId : best[_random.Next(best.Count)].PlayerId;
        }

        /// <summary>"Most correct": every player with the highest correct count; nobody when nobody got one right.</summary>
        private List<string> MostCorrect()
        {
            int most = 0;
            foreach (var p in _state.Players) most = Math.Max(most, p.CorrectCount);
            var ids = new List<string>();
            if (most == 0) return ids;
            foreach (var p in _state.Players)
            {
                if (p.CorrectCount == most) ids.Add(p.PlayerId);
            }

            return ids;
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
