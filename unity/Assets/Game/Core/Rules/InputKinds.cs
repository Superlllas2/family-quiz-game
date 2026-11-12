namespace FamilyQuiz.Core.Rules
{
    /// <summary>The "input" kinds (PROTOCOL.md section 3).</summary>
    public static class InputKinds
    {
        public const string Vote = "vote";
        public const string Choice = "choice";
        public const string Value = "value";
        public const string TrueFalse = "trueFalse";

        /// <summary>The Пас button in RaceQuestion (D157): locks the turn with no answer and no move.</summary>
        public const string Pass = "pass";
        public const string PowerUp = "powerUp";
        public const string EffectCleared = "effectCleared";
        public const string Rate = "rate";
    }
}
