namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "powerUps.targeting" and the TV settings panel (docs/03_rules.md, "Targeting").</summary>
    public static class TargetingModes
    {
        /// <summary>Any other player.</summary>
        public const string Free = "free";

        /// <summary>Only players with a better shared rank.</summary>
        public const string AimUp = "aimUp";

        public static bool IsValid(string mode)
        {
            return mode == Free || mode == AimUp;
        }
    }
}
