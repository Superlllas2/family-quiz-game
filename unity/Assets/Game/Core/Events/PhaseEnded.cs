using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    /// <summary>A phase ended (log event phaseEnded). Reason is one of <see cref="PhaseEndReasons"/>.</summary>
    public sealed class PhaseEnded : IGameEvent
    {
        public PhaseEnded(GamePhase phase, long phaseId, string reason, long durationMs)
        {
            Phase = phase;
            PhaseId = phaseId;
            Reason = reason;
            DurationMs = durationMs;
        }

        public GamePhase Phase { get; }
        public long PhaseId { get; }
        public string Reason { get; }
        public long DurationMs { get; }
    }
}
