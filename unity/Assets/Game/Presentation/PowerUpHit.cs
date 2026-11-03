using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The power up a player's marker shows on the TV (docs/03_rules.md, "Using"): in Answer the effect that landed on
    /// them, in Reveal and Scores the newest one used on them that lands at the next Answer. Plain C#, so it is tested
    /// without the engine.
    /// </summary>
    public sealed class PowerUpHit
    {
        private PowerUpHit(string powerUp, string attackerName)
        {
            PowerUp = powerUp;
            AttackerName = attackerName;
        }

        public string PowerUp { get; }
        public string AttackerName { get; }

        /// <summary>The hit to show next to the player in this phase, or null.</summary>
        public static PowerUpHit For(PlayerState player, GamePhase phase)
        {
            if (player == null) return null;
            switch (phase)
            {
                case GamePhase.Answer:
                    var turn = player.Turn;
                    return turn != null && turn.Effect != null ? new PowerUpHit(turn.Effect, turn.EffectFrom ?? string.Empty) : null;
                case GamePhase.Reveal:
                case GamePhase.Scores:
                    var pending = player.PendingEffects;
                    if (pending.Count == 0) return null;
                    var last = pending[pending.Count - 1];
                    return new PowerUpHit(last.PowerUp, last.AttackerName ?? string.Empty);
                default:
                    return null;
            }
        }
    }
}
