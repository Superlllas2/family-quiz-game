namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone message "leave": frees the seat at once (PROTOCOL.md section 5).</summary>
    public sealed class PlayerLeave : IGameCommand
    {
        public PlayerLeave(string conn, string inputId)
        {
            Conn = conn;
            InputId = inputId;
        }

        public string Conn { get; }
        public string InputId { get; }
    }
}
