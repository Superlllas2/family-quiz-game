using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Who a power up can hit (docs/03_rules.md, "Targeting" and "Caps"). Free: any other player. aimUp: only players with
    /// a better shared rank (tied players cannot target each other; a leader cannot attack). A target that already has
    /// powerUps.maxHitsPerQuestion effects waiting for the next question, or is immune (hit within the last
    /// powerUps.immunityQuestions questions), is listed but not available.
    /// </summary>
    public static class Targeting
    {
        /// <summary>True when the targeting mode lets an attacker of this rank aim at a target of that rank.</summary>
        public static bool AllowedByRank(string mode, int attackerRank, int targetRank)
        {
            return mode != TargetingModes.AimUp || targetRank < attackerRank;
        }

        /// <summary>
        /// True when the target is immune for the question the effect would land on: hit on question q, immune for
        /// questions q + 1 to q + immunityQuestions.
        /// </summary>
        public static bool Immune(int? lastHitQuestion, int landsOnQuestion, int immunityQuestions)
        {
            return lastHitQuestion.HasValue && landsOnQuestion - lastHitQuestion.Value <= immunityQuestions;
        }

        /// <summary>True when the cap or immunity keeps the target from being hit on the question the effect would land on.</summary>
        public static bool Blocked(PlayerState target, int landsOnQuestion, PowerUpsConfig config)
        {
            return target.PendingEffects.Count >= config.MaxHitsPerQuestion
                   || Immune(target.LastHitQuestion, landsOnQuestion, config.ImmunityQuestions);
        }

        /// <summary>The players this attacker may aim at under the mode, in join order, each with whether they can be hit now.</summary>
        public static List<(PlayerState Target, bool Available)> Targets(GameState state, PlayerState attacker, string mode,
            int landsOnQuestion, PowerUpsConfig config)
        {
            var list = new List<(PlayerState, bool)>();
            int attackerRank = Ranking.SharedRank(state, attacker);
            foreach (var p in state.Players)
            {
                if (p == attacker) continue;
                if (!AllowedByRank(mode, attackerRank, Ranking.SharedRank(state, p))) continue;
                list.Add((p, !Blocked(p, landsOnQuestion, config)));
            }

            return list;
        }
    }
}
