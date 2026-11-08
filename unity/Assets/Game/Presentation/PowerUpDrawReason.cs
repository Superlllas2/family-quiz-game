namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Why a player drew a power up, as the TV says it (D145; docs/03_rules.md, "Earning"): always for a right answer,
    /// and a player behind the leader also had the better chance (the leader draws at powerUps.drawChanceLeader, last
    /// place always). Plain C#.
    /// </summary>
    public static class PowerUpDrawReason
    {
        /// <summary>The localization key of the reason, from the player's rank before the question (1 = leader, ties share it).</summary>
        public static string Key(int rankBefore)
        {
            return rankBefore <= 1 ? "tv.powerUps.drawLeader" : "tv.powerUps.drawBehind";
        }
    }
}
