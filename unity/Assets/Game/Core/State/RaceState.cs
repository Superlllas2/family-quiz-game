using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.State
{
    /// <summary>The final race (docs/02_show_flow.md, "The final race"). Created when the race's first segment starts.</summary>
    public sealed class RaceState
    {
        private readonly Dictionary<string, int> _headStarts = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _positions = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _scoresBefore = new Dictionary<string, int>();
        private readonly Dictionary<string, long> _answerMs = new Dictionary<string, long>();
        private readonly List<string> _placement = new List<string>();

        /// <summary>Step each player started on.</summary>
        public IReadOnlyDictionary<string, int> HeadStarts => _headStarts;

        internal Dictionary<string, int> MutableHeadStarts => _headStarts;

        /// <summary>Step each player is on now.</summary>
        public IReadOnlyDictionary<string, int> Positions => _positions;

        internal Dictionary<string, int> MutablePositions => _positions;

        /// <summary>Total score of each player before the race (the first race tie break).</summary>
        public IReadOnlyDictionary<string, int> ScoresBefore => _scoresBefore;

        internal Dictionary<string, int> MutableScoresBefore => _scoresBefore;

        /// <summary>
        /// Total answer time in the race per player (the second race tie break): the clamped ms of each answer, and the
        /// player's whole window for a statement they did not answer.
        /// </summary>
        public IReadOnlyDictionary<string, long> AnswerMs => _answerMs;

        internal Dictionary<string, long> MutableAnswerMs => _answerMs;

        /// <summary>Logged at the start of the race (docs/02_show_flow.md, "Check").</summary>
        public bool LastPlaceCanWin { get; internal set; }

        /// <summary>The statement of the current race question, or null before the first one.</summary>
        public RaceStatementDef Statement { get; internal set; }

        /// <summary>1 based number of the current statement in the race; 0 before the first.</summary>
        public int StatementNumber { get; internal set; }

        /// <summary>True once a player reached race.finishSteps or race.maxQuestions statements were played.</summary>
        public bool Finished { get; internal set; }

        /// <summary>Set when the race finishes: every player from first to last by the race order (winner first).</summary>
        public IReadOnlyList<string> Placement => _placement;

        internal List<string> MutablePlacement => _placement;

        public int PositionOf(string playerId)
        {
            return _positions.TryGetValue(playerId, out var p) ? p : 0;
        }
    }
}
