namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// Last call cut this player's deadline (docs/03_rules.md). Net resends the answer screen to this player with the
    /// same phaseId and the new endsInMs; the phone does not restart its ms clock.
    /// </summary>
    public sealed class LastCallStarted : IGameEvent
    {
        public LastCallStarted(string playerId, long phaseId, long deadlineMs)
        {
            PlayerId = playerId;
            PhaseId = phaseId;
            DeadlineMs = deadlineMs;
        }

        public string PlayerId { get; }
        public long PhaseId { get; }
        public long DeadlineMs { get; }
    }
}
