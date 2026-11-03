namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A player used their power up on a target in the pick window (Reveal or Scores, one phaseId; log event powerUpUsed).
    /// The effect lands when the next Answer phase starts (<see cref="PowerUpApplied"/>).
    /// </summary>
    public sealed class PowerUpUsed : IGameEvent
    {
        public PowerUpUsed(string playerId, string powerUp, string targetId, long phaseId)
        {
            PlayerId = playerId;
            PowerUp = powerUp;
            TargetId = targetId;
            PhaseId = phaseId;
        }

        /// <summary>The attacker.</summary>
        public string PlayerId { get; }

        public string PowerUp { get; }
        public string TargetId { get; }
        public long PhaseId { get; }
    }
}
