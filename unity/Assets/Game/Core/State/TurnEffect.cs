namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// A power up that landed on a player when an Answer phase started (docs/03_rules.md, "Using"). Up to
    /// powerUps.maxHitsPerQuestion land at once; the phone shows them one after another and clears them in order (D145).
    /// </summary>
    public sealed class TurnEffect
    {
        internal TurnEffect(string powerUp, string attackerName)
        {
            PowerUp = powerUp;
            AttackerName = attackerName;
        }

        public string PowerUp { get; }

        /// <summary>The attacker's name, kept even if they left.</summary>
        public string AttackerName { get; }

        /// <summary>True once the phone reported this effect cleared (input effectCleared).</summary>
        public bool Cleared { get; internal set; }
    }
}
