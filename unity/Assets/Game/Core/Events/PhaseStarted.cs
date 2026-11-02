using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A phase started (log event phaseStarted). Scores starts with the same phaseId as Reveal (D28).
    /// Details (question, deadlines) are in GameState; listeners read them there.
    /// </summary>
    public sealed class PhaseStarted : IGameEvent
    {
        public PhaseStarted(GamePhase phase, long phaseId, long budgetMs = 0, long stagingExtraMs = 0, long? endsAtMs = null)
        {
            Phase = phase;
            PhaseId = phaseId;
            BudgetMs = budgetMs;
            StagingExtraMs = stagingExtraMs;
            EndsAtMs = endsAtMs;
        }

        public GamePhase Phase { get; }
        public long PhaseId { get; }

        /// <summary>The budget in ms with the staging extra included; for Answer the base window before multipliers. 0 for untimed phases.</summary>
        public long BudgetMs { get; }

        public long StagingExtraMs { get; }

        /// <summary>Core clock time the phase ends at the latest, or null (Lobby, PlayAgain).</summary>
        public long? EndsAtMs { get; }
    }
}
