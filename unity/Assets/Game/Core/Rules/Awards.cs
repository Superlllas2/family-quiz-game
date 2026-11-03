using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>The two awards on the Results screen (docs/03_rules.md, "Winner and ties"). Ties: every tied player gets it.</summary>
    public static class Awards
    {
        /// <summary>"Most correct": the most correct answers in rounds; nobody when nobody got one right.</summary>
        public static List<string> MostCorrect(IReadOnlyList<PlayerState> players)
        {
            int most = 0;
            foreach (var p in players)
            {
                if (p.CorrectCount > most) most = p.CorrectCount;
            }

            var ids = new List<string>();
            if (most == 0) return ids;
            foreach (var p in players)
            {
                if (p.CorrectCount == most) ids.Add(p.PlayerId);
            }

            return ids;
        }

        /// <summary>
        /// "Best comeback": the biggest rank gain from the end of round 2 (shared rank by score) to the end of the race
        /// (the 1 based place in the final order). Only a gain above 0 counts; players without a round 2 rank are left out.
        /// </summary>
        /// <param name="ranksAfterRound2">playerId to shared rank when round 2 ended.</param>
        /// <param name="placement">Every player in final order, winner first.</param>
        public static List<string> BestComeback(IReadOnlyDictionary<string, int> ranksAfterRound2, IReadOnlyList<string> placement)
        {
            int best = 0;
            var gains = new Dictionary<string, int>();
            for (int i = 0; i < placement.Count; i++)
            {
                if (!ranksAfterRound2.TryGetValue(placement[i], out var before)) continue;
                int gain = before - (i + 1);
                gains[placement[i]] = gain;
                if (gain > best) best = gain;
            }

            var ids = new List<string>();
            if (best <= 0) return ids;
            foreach (var id in placement)
            {
                if (gains.TryGetValue(id, out var g) && g == best) ids.Add(id);
            }

            return ids;
        }
    }
}
