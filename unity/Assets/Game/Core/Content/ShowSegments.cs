namespace FamilyQuiz.Core.Content
{
    /// <summary>The segment names of content/show_scripts.json (docs/15_schemas.md).</summary>
    public static class ShowSegments
    {
        public const string Intro = "intro";
        public const string Round = "round";
        public const string FinalIntro = "finalIntro";
        public const string FinalRace = "finalRace";
        public const string Results = "results";
        public const string Poll = "poll";
        public const string MiniGame = "miniGame";

        public static bool IsKnown(string segment)
        {
            switch (segment)
            {
                case Intro:
                case Round:
                case FinalIntro:
                case FinalRace:
                case Results:
                case Poll:
                case MiniGame:
                    return true;
                default:
                    return false;
            }
        }
    }
}
