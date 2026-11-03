using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Power up draws (docs/03_rules.md, "Earning"): after a correct answer in rounds, chance by the shared rank before
    /// the question, chance = drawChanceLeader + (1 - drawChanceLeader) x (rank - 1) / (players - 1); no draws with one
    /// player; the type with equal chance among the enabled ones; seeded.
    /// </summary>
    public static class PowerUpDealer
    {
        /// <summary>Resolution of the roll: the random gives a whole number below this, compared with chance x this.</summary>
        private const int RollScale = 1000000;

        /// <summary>The draw chance, 0 to 1. With one player (or none), 0.</summary>
        public static double Chance(int rank, int players, double drawChanceLeader)
        {
            if (players < 2) return 0;
            double share = (double)(Math.Max(1, Math.Min(rank, players)) - 1) / (players - 1);
            double chance = drawChanceLeader + (1 - drawChanceLeader) * share;
            return Math.Max(0, Math.Min(1, chance));
        }

        /// <summary>
        /// One draw: the type drawn, or null (no luck, a zero chance or no type enabled). The chance is rolled only when it
        /// is above 0 and a type is enabled; a second number picks the type only on a hit.
        /// </summary>
        public static string Draw(IRandom random, double chance, IReadOnlyList<string> types)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (chance <= 0 || types == null || types.Count == 0) return null;
            if (random.Next(RollScale) >= chance * RollScale) return null;
            return types.Count == 1 ? types[0] : types[random.Next(types.Count)];
        }
    }
}
