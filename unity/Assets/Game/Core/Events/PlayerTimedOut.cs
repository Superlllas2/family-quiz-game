namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A player who has not locked in reached their personal deadline plus net.inputGraceMs: they are done for this
    /// Answer phase (D73) and a later input gets "closed". Raised once per player per Answer phase, connected or not,
    /// while the phase runs or in the same batch just before PhaseEnded(Answer). Net refreshes the other players'
    /// waiting.pending lists; the TV updates its answered count.
    /// </summary>
    public sealed class PlayerTimedOut : IGameEvent
    {
        public PlayerTimedOut(string playerId, long phaseId)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }
    }
}
