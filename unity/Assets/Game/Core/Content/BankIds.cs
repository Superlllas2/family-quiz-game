namespace FamilyQuiz.Core.Content
{
    /// <summary>The question banks (decisions/0020, D116): the full v4 bank and the curated test bank.</summary>
    public static class BankIds
    {
        /// <summary>content/questions.json: 3000 questions in 60 topics.</summary>
        public const string Full = "full";

        /// <summary>content/questions_refined.json: about 10 refined questions in each of 12 topics.</summary>
        public const string Curated = "curated";

        public static bool IsValid(string id)
        {
            return id == Full || id == Curated;
        }
    }
}
