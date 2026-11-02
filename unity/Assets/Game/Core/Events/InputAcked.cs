namespace FamilyQuiz.Core.Events
{
    /// <summary>The answer to a character, input, lobby, leave or poll message. Reason is null when ok and new.</summary>
    public sealed class InputAcked : IGameEvent
    {
        public InputAcked(string conn, string inputId, bool ok, string reason)
        {
            Conn = conn;
            InputId = inputId;
            Ok = ok;
            Reason = reason;
        }

        public string Conn { get; }
        public string InputId { get; }
        public bool Ok { get; }
        public string Reason { get; }
    }
}
