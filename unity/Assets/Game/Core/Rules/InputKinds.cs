namespace FamilyQuiz.Core.Rules
{
    /// <summary>The "input" kinds (PROTOCOL.md section 3).</summary>
    public static class InputKinds
    {
        public const string Vote = "vote";
        public const string Choice = "choice";
        public const string Value = "value";
        public const string TrueFalse = "trueFalse";
        public const string Prediction = "prediction";
        public const string PowerUp = "powerUp";
        public const string EffectCleared = "effectCleared";
    }
}
