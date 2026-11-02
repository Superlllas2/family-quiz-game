namespace FamilyQuiz.Core.Events
{
    /// <summary>A join or rejoin failed. Reason is one of <see cref="RefusalReasons"/>.</summary>
    public sealed class JoinRefused : IGameEvent
    {
        public JoinRefused(string conn, string reason)
        {
            Conn = conn;
            Reason = reason;
        }

        public string Conn { get; }
        public string Reason { get; }
    }
}
