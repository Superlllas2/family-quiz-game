namespace FamilyQuiz.Core
{
    /// <summary>The two languages (PROTOCOL.md section 3, join.lang).</summary>
    public static class Languages
    {
        public const string Russian = "ru";
        public const string English = "en";

        /// <summary>The game language defaults to Russian (D34, decisions/0002_spec_review.md).</summary>
        public const string Default = Russian;

        public static bool IsSupported(string lang)
        {
            return lang == Russian || lang == English;
        }
    }
}
