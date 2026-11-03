namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A used power up landed on its target at the start of an Answer phase. Net sends the target's phone the "effect"
    /// message (PROTOCOL.md section 4); the TV shows the attacker's name next to the target's chip.
    /// </summary>
    public sealed class PowerUpApplied : IGameEvent
    {
        public PowerUpApplied(string targetId, string attackerId, string attackerName, string powerUp, long phaseId)
        {
            TargetId = targetId;
            AttackerId = attackerId;
            AttackerName = attackerName;
            PowerUp = powerUp;
            PhaseId = phaseId;
        }

        public string TargetId { get; }
        public string AttackerId { get; }
        public string AttackerName { get; }
        public string PowerUp { get; }

        /// <summary>The Answer phase it landed in.</summary>
        public long PhaseId { get; }
    }
}
