using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One power up hit the TV shows (docs/03_rules.md, "Using"; D145: "Аня → Слизь → Соня"): in Answer every effect
    /// that landed on the player, in Reveal and Scores every one used on them that lands at the next Answer, in the
    /// order they were used. Plain C#, so it is tested without the engine.
    /// </summary>
    public sealed class PowerUpHit
    {
        private PowerUpHit(string powerUp, string attackerName, string targetName)
        {
            PowerUp = powerUp;
            AttackerName = attackerName;
            TargetName = targetName;
        }

        public string PowerUp { get; }
        public string AttackerName { get; }
        public string TargetName { get; }

        /// <summary>The hits on this player to show in this phase; empty when there are none.</summary>
        public static IReadOnlyList<PowerUpHit> All(PlayerState player, GamePhase phase)
        {
            var hits = new List<PowerUpHit>();
            if (player == null) return hits;
            string target = player.Name ?? string.Empty;
            switch (phase)
            {
                case GamePhase.Answer:
                    if (player.Turn != null)
                        foreach (var e in player.Turn.Effects)
                            hits.Add(new PowerUpHit(e.PowerUp, e.AttackerName ?? string.Empty, target));
                    break;
                case GamePhase.Reveal:
                case GamePhase.Scores:
                    foreach (var e in player.PendingEffects)
                        hits.Add(new PowerUpHit(e.PowerUp, e.AttackerName ?? string.Empty, target));
                    break;
            }

            return hits;
        }
    }
}
