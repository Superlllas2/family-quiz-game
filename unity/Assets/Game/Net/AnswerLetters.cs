namespace FamilyQuiz.Net
{
    /// <summary>
    /// The letters of the four answer positions in display order, the same in both languages
    /// (docs/04_formats.md, docs/06_phone_screens.md).
    /// </summary>
    public static class AnswerLetters
    {
        public const string Letters = "ABCD";

        /// <summary>The letter of a display index (0 to 3), or an empty string outside that range.</summary>
        public static string For(int displayIndex)
        {
            return displayIndex >= 0 && displayIndex < Letters.Length ? Letters[displayIndex].ToString() : "";
        }
    }
}
