namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone message "lobby": ready or notReady (PROTOCOL.md section 3, D76).</summary>
    public sealed class LobbyAction : IGameCommand
    {
        public LobbyAction(string conn, string inputId, string action)
        {
            Conn = conn;
            InputId = inputId;
            Action = action;
        }

        public string Conn { get; }
        public string InputId { get; }
        public string Action { get; }
    }
}
