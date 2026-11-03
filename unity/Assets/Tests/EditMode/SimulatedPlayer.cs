using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// A simulated player for headless games. It exists only in test code; the game has no bots (D32).
    /// Profiles from docs/13_architecture.md: answer time uniform in [MinMs, MaxMs] for votes, answers and race answers,
    /// right with the given accuracy. A time past the player's own deadline (last call included), or past the end of the
    /// vote, means no input. The driver asks <see cref="PlannedAtMs"/> for the moment of the next input, moves the fake
    /// clock exactly there, then calls <see cref="Act"/>. With power ups on it uses a held power up in the pick window
    /// (on a random available target), clears a sabotage before answering, and answers the poll.
    /// </summary>
    public sealed class SimulatedPlayer
    {
        public enum Style
        {
            /// <summary>Answer after a time drawn from the profile.</summary>
            Profile,

            /// <summary>Use the whole window: answer at the personal deadline, vote 1 ms before the vote ends.</summary>
            FullWindow,

            /// <summary>Never send anything.</summary>
            Never
        }

        private readonly IRandom _random;
        private long _plannedPhaseId = -1;
        private long? _plannedAtMs;
        private long _delayMs;
        private bool _right;
        private int _inputs;

        /// <summary>The planned extra input of this phase: a power up pick, an effect clear or the poll.</summary>
        private long _extraPhaseId = -1;
        private long? _extraAtMs;

        public SimulatedPlayer(string profile, string conn, string name, long minMs, long maxMs, double accuracy, int seed,
            Style style = Style.Profile)
        {
            Profile = profile;
            Conn = conn;
            Name = name;
            MinMs = minMs;
            MaxMs = maxMs;
            Accuracy = accuracy;
            Behaviour = style;
            _random = new SeededRandom(seed);
        }

        /// <summary>Uses held power ups in the pick window (the simulation's power up setting, docs/13_architecture.md).</summary>
        public bool UsePowerUps { get; set; } = true;

        public string Profile { get; }
        public string Conn { get; }
        public string Name { get; }
        public long MinMs { get; }
        public long MaxMs { get; }
        public double Accuracy { get; }
        public Style Behaviour { get; }

        /// <summary>The six players of docs/13_architecture.md: 2 fast, 2 normal, 2 slow.</summary>
        public static SimulatedPlayer[] SixProfiles(int seed, Style style = Style.Profile)
        {
            return new[]
            {
                new SimulatedPlayer("fast", "c1", "Fast A", 1000, 3000, 0.9, seed * 10 + 1, style),
                new SimulatedPlayer("fast", "c2", "Fast B", 1000, 3000, 0.9, seed * 10 + 2, style),
                new SimulatedPlayer("normal", "c3", "Normal A", 3000, 8000, 0.6, seed * 10 + 3, style),
                new SimulatedPlayer("normal", "c4", "Normal B", 3000, 8000, 0.6, seed * 10 + 4, style),
                new SimulatedPlayer("slow", "c5", "Slow A", 7000, 14000, 0.3, seed * 10 + 5, style),
                new SimulatedPlayer("slow", "c6", "Slow B", 7000, 14000, 0.3, seed * 10 + 6, style)
            };
        }

        /// <summary>
        /// Plans the input for the current phase once per phaseId and returns its clock time, or null when there is
        /// nothing (more) to send in this phase.
        /// </summary>
        public long? PlannedAtMs(Game game)
        {
            var extra = PlannedExtraAtMs(game);
            var answer = PlannedAnswerAtMs(game);
            if (extra.HasValue && answer.HasValue) return Math.Min(extra.Value, answer.Value);
            return extra ?? answer;
        }

        /// <summary>
        /// The time of the extra input of this phase, or null: a power up in Reveal or Scores (0.3 to 5 s after Reveal
        /// starts, inside the 7 s window), the effect clear in Answer (1 to 3 s after the options appear), the poll
        /// (3 to 30 s after it opens).
        /// </summary>
        private long? PlannedExtraAtMs(Game game)
        {
            var state = game.State;
            var me = state.FindByConn(Conn);
            if (me == null || Behaviour == Style.Never) return null;
            bool pick = (state.Phase == GamePhase.Reveal || state.Phase == GamePhase.Scores) && UsePowerUps && game.CanPickPowerUp(me);
            bool clear = state.Phase == GamePhase.Answer && me.Turn != null && me.Turn.Effect != null && !me.Turn.EffectCleared;
            bool poll = state.Phase == GamePhase.Poll && !state.SubmittedPoll(me.PlayerId);
            if (!pick && !clear && !poll)
            {
                if (state.PhaseId != _extraPhaseId) _extraAtMs = null;
                return null;
            }

            if (_extraPhaseId != state.PhaseId)
            {
                _extraPhaseId = state.PhaseId;
                long start = state.Phase == GamePhase.Scores ? state.PhaseStartedAtMs : state.PhaseStartedAtMs;
                long delay = pick ? 300 + _random.Next(4700) : clear ? 1000 + _random.Next(2001) : 3000 + _random.Next(27001);
                _extraAtMs = start + delay;
                if (pick && state.Phase == GamePhase.Scores && _extraAtMs >= state.PhaseEndsAtMs) _extraAtMs = state.PhaseEndsAtMs - 1;
            }

            return _extraAtMs;
        }

        private long? PlannedAnswerAtMs(Game game)
        {
            var state = game.State;
            bool inputPhase = state.Phase == GamePhase.TopicVote || state.Phase == GamePhase.Answer || state.Phase == GamePhase.RaceQuestion;
            if (!inputPhase || Behaviour == Style.Never) return null;
            var me = state.FindByConn(Conn);
            if (me == null) return null;
            if (_plannedPhaseId != state.PhaseId)
            {
                _plannedPhaseId = state.PhaseId;
                _delayMs = MinMs + _random.Next((int)(MaxMs - MinMs + 1));
                _right = _random.Next(1000) < Accuracy * 1000;
                long start = state.PhaseStartedAtMs;
                if (state.Phase == GamePhase.TopicVote)
                {
                    long end = state.PhaseEndsAtMs.Value;
                    long at = Behaviour == Style.FullWindow ? end - 1 : start + _delayMs;
                    _plannedAtMs = at < end ? at : (long?)null; // too slow: no vote
                }
                else
                {
                    if (Behaviour == Style.FullWindow) _delayMs = me.Turn.WindowMs;
                    _plannedAtMs = start + _delayMs;
                }
            }

            if (!_plannedAtMs.HasValue) return null;
            // Answer phases: a time past the own deadline (last call may have moved it) means no answer.
            if (state.Phase != GamePhase.TopicVote && (me.Turn == null || me.Turn.Locked || _plannedAtMs.Value > me.Turn.DeadlineMs))
                return null;
            // Under a sabotage the answer waits until it is cleared (the clock keeps running).
            if (state.Phase == GamePhase.Answer && me.Turn.Effect != null && !me.Turn.EffectCleared) return null;
            return _plannedAtMs;
        }

        /// <summary>The input to send now, or null. Call after moving the clock to <see cref="PlannedAtMs"/>.</summary>
        public IGameCommand Act(Game game)
        {
            var state = game.State;
            var extraAt = PlannedExtraAtMs(game);
            if (extraAt.HasValue && game.NowMs >= extraAt.Value)
            {
                _extraAtMs = null;
                return Extra(game);
            }

            var at = PlannedAnswerAtMs(game);
            if (!at.HasValue || game.NowMs < at.Value) return null;
            _plannedAtMs = null;
            string inputId = Conn + "-" + ++_inputs;
            switch (state.Phase)
            {
                case GamePhase.TopicVote:
                    return new SubmitAnswer(Conn, inputId, state.PhaseId, InputKinds.Vote, null, null,
                        door: _random.Next(state.Round.Doors.Count));
                case GamePhase.RaceQuestion:
                    bool truth = state.Race.Statement.Answer;
                    return new SubmitAnswer(Conn, inputId, state.PhaseId, InputKinds.TrueFalse, null, _delayMs,
                        answer: _right ? truth : !truth);
                default:
                    var question = state.Question;
                    if (question.IsClosestValue)
                        return new SubmitAnswer(Conn, inputId, state.PhaseId, InputKinds.Value, null, _delayMs, value: Guess(question));
                    int correct = question.CorrectDisplayIndex;
                    int choice = _right ? correct : (correct + 1 + _random.Next(ProtocolLimits.ChoiceCount - 1)) % ProtocolLimits.ChoiceCount;
                    return new SubmitAnswer(Conn, inputId, state.PhaseId, InputKinds.Choice, choice, _delayMs);
            }
        }

        private IGameCommand Extra(Game game)
        {
            var state = game.State;
            var me = state.FindByConn(Conn);
            string inputId = Conn + "-" + ++_inputs;
            switch (state.Phase)
            {
                case GamePhase.Answer:
                    // The effect took the planned time to clear; an answer planned before then comes right after it.
                    long cleared = game.NowMs - state.PhaseStartedAtMs;
                    if (_plannedAtMs.HasValue && _plannedAtMs.Value < game.NowMs + 1) _plannedAtMs = game.NowMs + 1;
                    if (_delayMs < cleared + 1) _delayMs = cleared + 1;
                    return new ClearEffect(Conn, inputId, state.PhaseId, me.Turn.Effect, cleared);
                case GamePhase.Poll:
                    var answers = new Dictionary<string, object>();
                    foreach (var q in game.Content.Poll)
                    {
                        if (q.IsChoice) answers[q.Id] = (long)_random.Next(q.Options.Count);
                        else if (_random.Next(2) == 0) answers[q.Id] = Name + " liked it";
                    }

                    return new SubmitPoll(Conn, inputId, answers);
                default:
                    var targets = game.PowerUpTargets(me).Where(t => t.Available).ToList();
                    var target = targets[_random.Next(targets.Count)].Target;
                    return new UsePowerUp(Conn, inputId, state.PhaseId, me.HeldPowerUp, target.PlayerId);
            }
        }

        /// <summary>A right guess lands within half the tolerance of the answer; a wrong one anywhere on the slider.</summary>
        private double Guess(QuestionInPlay question)
        {
            var def = question.Def;
            double min = def.Range[0], max = def.Range[1], step = def.Step.Value;
            double guess = _right
                ? def.Answer.Value + (_random.Next(1001) / 1000.0 - 0.5) * def.Tolerance.Value
                : min + _random.Next(1001) / 1000.0 * (max - min);
            guess = min + Math.Round((guess - min) / step) * step;
            return Math.Max(min, Math.Min(max, guess));
        }
    }
}
