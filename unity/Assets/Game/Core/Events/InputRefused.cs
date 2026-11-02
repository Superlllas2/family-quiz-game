namespace FamilyQuiz.Core.Events
{
    /// <summary>A seated player's input was refused (log event inputRefused). Raised next to the InputAcked with ok false.</summary>
    public sealed class InputRefused : IGameEvent
    {
        public InputRefused(string playerId, string kind, long phaseId, string reason)
        {
            PlayerId = playerId;
            Kind = kind;
            PhaseId = phaseId;
            Reason = reason;
        }

        public string PlayerId { get; }
        public string Kind { get; }
        public long PhaseId { get; }
        public string Reason { get; }
    }
}
