namespace FamilyQuiz.Core.Events
{
    /// <summary>The reasons of the "refused" message (PROTOCOL.md section 4).</summary>
    public static class RefusalReasons
    {
        public const string RoomFull = "roomFull";
        public const string NameTaken = "nameTaken";
        public const string BadName = "badName";
        public const string GameStarted = "gameStarted";
        public const string BadToken = "badToken";
        public const string BadVersion = "badVersion";
    }
}
