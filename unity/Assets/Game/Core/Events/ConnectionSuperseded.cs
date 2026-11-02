namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A newer socket took a seat whose older socket is still open (two tabs with the same token).
    /// Net closes OldConn with close code 4002 (PROTOCOL.md section 5, step 6).
    /// </summary>
    public sealed class ConnectionSuperseded : IGameEvent
    {
        public ConnectionSuperseded(string playerId, string oldConn, string newConn)
        {
            PlayerId = playerId;
            OldConn = oldConn;
            NewConn = newConn;
        }

        public string PlayerId { get; }
        public string OldConn { get; }
        public string NewConn { get; }
    }
}
