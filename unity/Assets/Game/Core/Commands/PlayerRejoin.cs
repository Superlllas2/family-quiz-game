namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone message "rejoin" (PROTOCOL.md sections 3 and 5).</summary>
    public sealed class PlayerRejoin : IGameCommand
    {
        public PlayerRejoin(string conn, string token)
        {
            Conn = conn;
            Token = token;
        }

        public string Conn { get; }
        public string Token { get; }
    }
}
