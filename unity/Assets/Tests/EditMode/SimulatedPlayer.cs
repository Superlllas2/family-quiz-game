using System;
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
    /// clock exactly there, then calls <see cref="Act"/>.
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
            return _plannedAtMs;
        }

        /// <summary>The input to send now, or null. Call after moving the clock to <see cref="PlannedAtMs"/>.</summary>
        public SubmitAnswer Act(Game game)
        {
            var at = PlannedAtMs(game);
            if (!at.HasValue || game.NowMs < at.Value) return null;
            _plannedAtMs = null;
            var state = game.State;
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
