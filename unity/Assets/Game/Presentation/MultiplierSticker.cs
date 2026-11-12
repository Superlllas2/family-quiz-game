namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Whether the TV shows a round multiplier (the question card's sticker, the vote banner's): only above 1. Every
    /// multiplier is 1 for now (D151), so no sticker or text shows anywhere. Plain C#, so it is tested without the engine.
    /// </summary>
    public static class MultiplierSticker
    {
        public static bool Shows(double multiplier)
        {
            return multiplier > 1.0001;
        }
    }
}
