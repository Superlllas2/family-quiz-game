namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone message "join" (PROTOCOL.md section 3).</summary>
    public sealed class PlayerJoin : IGameCommand
    {
        public PlayerJoin(string conn, string name, string lang)
        {
            Conn = conn;
            Name = name;
            Lang = lang;
        }

        public string Conn { get; }
        public string Name { get; }
        public string Lang { get; }
    }
}
