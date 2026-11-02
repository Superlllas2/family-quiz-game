namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A player locked an answer in the Answer phase (TV chip pops; log event input). Choice is the display index;
    /// Ms is clamped to 0..T. Correct and Points are what the answer will score at Reveal.
    /// </summary>
    public sealed class AnswerLocked : IGameEvent
    {
        public AnswerLocked(string playerId, long phaseId, string kind, int choice, long ms, bool correct, int points)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
            Kind = kind;
            Choice = choice;
            Ms = ms;
            Correct = correct;
            Points = points;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }
        public string Kind { get; }
        public int Choice { get; }
        public long Ms { get; }
        public bool Correct { get; }
        public int Points { get; }
    }
}
