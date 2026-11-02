using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    public sealed class PhaseStarted : IGameEvent
    {
        public PhaseStarted(GamePhase phase, long phaseId)
        {
            Phase = phase;
            PhaseId = phaseId;
        }

        public GamePhase Phase { get; }
        public long PhaseId { get; }
    }
}
