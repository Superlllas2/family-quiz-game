namespace FamilyQuiz.Core.State
{
    /// <summary>phaseEnded.reason values (docs/15_schemas.md, "Allowed values").</summary>
    public static class PhaseEndReasons
    {
        public const string Timeout = "timeout";
        public const string AllDone = "allDone";
        public const string Cancelled = "cancelled";
        public const string Abandoned = "abandoned";
        public const string Skip = "skip";
    }
}
