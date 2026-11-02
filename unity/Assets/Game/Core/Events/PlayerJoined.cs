namespace FamilyQuiz.Core.Events
{
    /// <summary>A new player took a seat. Net answers the conn with welcome and snapshot.</summary>
    public sealed class PlayerJoined : IGameEvent
    {
        public PlayerJoined(string playerId, string conn)
        {
            PlayerId = playerId;
            Conn = conn;
        }

        public string PlayerId { get; }
        public string Conn { get; }
    }
}
