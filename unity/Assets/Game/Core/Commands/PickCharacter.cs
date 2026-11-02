namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone message "character" (PROTOCOL.md section 3).</summary>
    public sealed class PickCharacter : IGameCommand
    {
        public PickCharacter(string conn, string inputId, int characterId)
        {
            Conn = conn;
            InputId = inputId;
            CharacterId = characterId;
        }

        public string Conn { get; }
        public string InputId { get; }
        public int CharacterId { get; }
    }
}
