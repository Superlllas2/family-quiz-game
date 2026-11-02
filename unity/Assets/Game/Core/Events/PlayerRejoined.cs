namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A rejoin with a valid token: the seat now belongs to Conn. Net answers it with welcome and snapshot.
    /// IsRefresh is true when the rejoin came from the seat's current conn (the phone refreshing its screen).
    /// </summary>
    public sealed class PlayerRejoined : IGameEvent
    {
        public PlayerRejoined(string playerId, string conn, long awayMs, bool isRefresh)
        {
            PlayerId = playerId;
            Conn = conn;
            AwayMs = awayMs;
            IsRefresh = isRefresh;
        }

        public string PlayerId { get; }
        public string Conn { get; }
        public long AwayMs { get; }
        public bool IsRefresh { get; }
    }
}
