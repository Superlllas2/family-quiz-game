namespace FamilyQuiz.Core.State
{
    /// <summary>A power up used on a player in a pick window; it lands at the start of the next Answer phase (docs/03_rules.md).</summary>
    public sealed class PendingEffect
    {
        internal PendingEffect(string powerUp, string attackerId, string attackerName)
        {
            PowerUp = powerUp;
            AttackerId = attackerId;
            AttackerName = attackerName;
        }

        public string PowerUp { get; }
        public string AttackerId { get; }

        /// <summary>Kept so the effect still names the attacker if they leave before it lands.</summary>
        public string AttackerName { get; }
    }
}
