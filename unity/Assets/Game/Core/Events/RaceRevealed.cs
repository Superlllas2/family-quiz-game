namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A race statement was revealed (raised right after PhaseStarted(RaceReveal)): every PlayerState.Turn has Correct,
    /// GameState.Race.Positions hold the new steps and Race.Finished says whether this was the last statement.
    /// </summary>
    public sealed class RaceRevealed : IGameEvent
    {
        public RaceRevealed(long phaseId, string statementId, bool answer, bool finished)
        {
            PhaseId = phaseId;
            StatementId = statementId;
            Answer = answer;
            Finished = finished;
        }

        public long PhaseId { get; }
        public string StatementId { get; }
        public bool Answer { get; }
        public bool Finished { get; }
    }
}
