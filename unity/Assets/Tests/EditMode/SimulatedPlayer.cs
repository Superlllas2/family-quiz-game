using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// A simulated player for headless games. It exists only in test code; the game has no bots (D32).
    /// Profiles from docs/13_architecture.md: answer time uniform in [MinMs, MaxMs], right with the given accuracy.
    /// A time past the player's own deadline (last call included) means no answer.
    /// </summary>
    public sealed class SimulatedPlayer
    {
        private readonly IRandom _random;
        private long _plannedAnswerPhaseId;
        private long _delayMs;
        private bool _right;
        private bool _sent;
        private int _inputs;

        public SimulatedPlayer(string profile, string conn, string name, long minMs, long maxMs, double accuracy, int seed)
        {
            Profile = profile;
            Conn = conn;
            Name = name;
            MinMs = minMs;
            MaxMs = maxMs;
            Accuracy = accuracy;
            _random = new SeededRandom(seed);
        }

        public string Profile { get; }
        public string Conn { get; }
        public string Name { get; }
        public long MinMs { get; }
        public long MaxMs { get; }
        public double Accuracy { get; }

        /// <summary>The input to send now, or null. Called once per frame.</summary>
        public SubmitAnswer Act(Game game)
        {
            var state = game.State;
            if (state.Phase != GamePhase.Answer) return null;
            var me = state.FindByConn(Conn);
            if (me?.Turn == null || me.Turn.Locked) return null;

            if (_plannedAnswerPhaseId != state.PhaseId)
            {
                _plannedAnswerPhaseId = state.PhaseId;
                _delayMs = MinMs + _random.Next((int)(MaxMs - MinMs + 1));
                _right = _random.Next(1000) < Accuracy * 1000;
                _sent = false;
            }

            if (_sent) return null;
            long startedAt = state.Question.AnswerStartedAtMs;
            if (startedAt + _delayMs > me.Turn.DeadlineMs)
            {
                _sent = true; // too slow for this window: no answer
                return null;
            }

            if (game.NowMs < startedAt + _delayMs) return null;
            _sent = true;
            int correct = state.Question.CorrectDisplayIndex;
            int choice = _right ? correct : (correct + 1 + _random.Next(ProtocolLimits.ChoiceCount - 1)) % ProtocolLimits.ChoiceCount;
            return new SubmitAnswer(Conn, Conn + "-" + ++_inputs, state.PhaseId, InputKinds.Choice, choice, _delayMs);
        }
    }
}
