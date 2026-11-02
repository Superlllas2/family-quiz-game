namespace FamilyQuiz.Core.Events
{
    /// <summary>A seat was freed. Reason is "left" (tapped Leave) or "seatExpired" (docs/15_schemas.md).</summary>
    public sealed class PlayerLeft : IGameEvent
    {
        public const string ReasonLeft = "left";
        public const string ReasonSeatExpired = "seatExpired";

        public PlayerLeft(string playerId, string name, string conn, string reason)
        {
            PlayerId = playerId;
            Name = name;
            Conn = conn;
            Reason = reason;
        }

        public string PlayerId { get; }
        public string Name { get; }

        /// <summary>The conn the player last had, or null.</summary>
        public string Conn { get; }

        public string Reason { get; }
    }
}
