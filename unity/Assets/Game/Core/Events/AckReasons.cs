namespace FamilyQuiz.Core.Events
{
    /// <summary>The reasons of the "ack" message (PROTOCOL.md section 4).</summary>
    public static class AckReasons
    {
        public const string Stale = "stale";
        public const string Closed = "closed";
        public const string Duplicate = "duplicate";
        public const string Invalid = "invalid";
        public const string Immune = "immune";
        public const string NoPowerUp = "noPowerUp";
        public const string CharacterTaken = "characterTaken";
    }
}
