using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>The topic shelves and leans of topics.json (docs/15_schemas.md, D154).</summary>
    public static class TopicShelves
    {
        public const string Screen = "screen";
        public const string Knowledge = "knowledge";
        public const string Life = "life";

        /// <summary>The shelves in door order: door 1 screen, door 2 knowledge, door 3 life.</summary>
        public static readonly IReadOnlyList<string> InDoorOrder = new[] { Screen, Knowledge, Life };

        public const string LeanYoung = "young";
        public const string LeanOld = "old";
        public const string LeanAll = "all";

        public static bool IsShelf(string shelf)
        {
            return shelf == Screen || shelf == Knowledge || shelf == Life;
        }

        public static bool IsLean(string lean)
        {
            return lean == LeanYoung || lean == LeanOld || lean == LeanAll;
        }
    }
}
