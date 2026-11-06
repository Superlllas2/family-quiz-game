using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    /// <summary>The pause menu opened (decisions/0018, D105): the composition root stops the core clock; phones show paused.</summary>
    public sealed class GamePaused : IGameEvent
    {
        public GamePaused(GamePhase phase, long phaseId)
        {
            Phase = phase;
            PhaseId = phaseId;
        }

        public GamePhase Phase { get; }
        public long PhaseId { get; }
    }
}
