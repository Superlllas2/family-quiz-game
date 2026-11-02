namespace FamilyQuiz.Core.Content
{
    /// <summary>Question difficulties (docs/15_schemas.md, questions.json "difficulty").</summary>
    public static class Difficulties
    {
        public const string Easy = "easy";
        public const string Medium = "medium";
        public const string Hard = "hard";

        public static bool IsValid(string difficulty)
        {
            return difficulty == Easy || difficulty == Medium || difficulty == Hard;
        }
    }
}
