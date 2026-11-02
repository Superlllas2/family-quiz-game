namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// Reveal scored the question: every PlayerState.Turn now has Correct and Points, and scores and streaks are
    /// updated (ScoreChanged follows per player who earned points). Raised right after PhaseStarted(Reveal).
    /// </summary>
    public sealed class Revealed : IGameEvent
    {
        public Revealed(long phaseId, string questionId, int correctDisplayIndex)
        {
            PhaseId = phaseId;
            QuestionId = questionId;
            CorrectDisplayIndex = correctDisplayIndex;
        }

        public long PhaseId { get; }
        public string QuestionId { get; }
        public int CorrectDisplayIndex { get; }
    }
}
