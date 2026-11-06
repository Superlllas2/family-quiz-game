using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Events
{
    /// <summary>The game goes on after a pause (decisions/0018, D105): the core clock runs again; phones get their screen back.</summary>
    public sealed class GameResumed : IGameEvent
    {
        public GameResumed(GamePhase phase, long phaseId)
        {
            Phase = phase;
            PhaseId = phaseId;
        }

        public GamePhase Phase { get; }
        public long PhaseId { get; }
    }
}
