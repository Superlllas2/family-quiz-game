using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>The final race (docs/02_show_flow.md, "The final race"; docs/03_rules.md, "Final race", "Winner and ties").</summary>
    public static class RaceRules
    {
        /// <summary>
        /// floor(maxHeadStart x rankFromBottom / (players - 1)) with rankFromBottom = players - rank and the shared rank,
        /// so tied players share the higher head start of their group. A player alone in the game is the leader and
        /// starts on maxHeadStart.
        /// </summary>
        public static int HeadStart(int rank, int players, int maxHeadStart)
        {
            if (players <= 1) return maxHeadStart;
            int fromBottom = Math.Max(0, players - rank);
            return (int)Math.Floor(maxHeadStart * (double)fromBottom / (players - 1));
        }

        /// <summary>
        /// docs/02_show_flow.md "Check": with no statement limit (D157) last place can always still win, so this is
        /// always true. Kept as a function so the race start keeps logging it.
        /// </summary>
        public static bool LastPlaceCanWin(IEnumerable<int> headStarts, RaceConfig race)
        {
            return true;
        }

        /// <summary>
        /// The next statement (D155): an unseen one picked by the seeded random generator; when none is left, the least
        /// recently seen one. Never one already used in this race. Null when every statement was used.
        /// </summary>
        public static RaceStatementDef PickStatement(IReadOnlyList<RaceStatementDef> statements, IReadOnlyCollection<string> seen,
            ICollection<string> usedInGame, IRandom random)
        {
            var unseen = new List<RaceStatementDef>();
            RaceStatementDef oldest = null;
            long oldestStamp = long.MaxValue;
            foreach (var s in statements)
            {
                if (usedInGame.Contains(s.Id)) continue;
                long? stamp = State.SeenList.LastSeenIn(seen, s.Id);
                if (!stamp.HasValue)
                {
                    unseen.Add(s);
                }
                else if (stamp.Value < oldestStamp)
                {
                    oldest = s;
                    oldestStamp = stamp.Value;
                }
            }

            if (unseen.Count > 0) return unseen[random.Next(unseen.Count)];
            return oldest;
        }

        /// <summary>
        /// The new step after a statement (D157): a right answer moves race.stepsPerCorrect forward, never past the finish;
        /// a wrong one race.stepsPerWrong back, never below 0; a pass or no answer stays.
        /// </summary>
        public static int Move(int position, RaceOutcome outcome, RaceConfig race)
        {
            switch (outcome)
            {
                case RaceOutcome.Right:
                    return Math.Min(race.FinishSteps, position + race.StepsPerCorrect);
                case RaceOutcome.Wrong:
                    return Math.Max(0, position - race.StepsPerWrong);
                default:
                    return position;
            }
        }

        /// <summary>The race ends when a player reached race.finishSteps (D157: no statement limit).</summary>
        public static bool IsFinished(IEnumerable<int> positions, RaceConfig race)
        {
            return positions.Any(p => p >= race.FinishSteps);
        }

        /// <summary>
        /// Every player from first to last (D158): furthest first; ties by the faster total answer time in the race, then
        /// the higher total score before the race, then a seeded random pick. The first one is the race (and game) winner.
        /// </summary>
        public static List<string> Order(IReadOnlyList<string> playerIds, IReadOnlyDictionary<string, int> positions,
            IReadOnlyDictionary<string, int> scoresBefore, IReadOnlyDictionary<string, long> answerMs, IRandom random)
        {
            // A seeded shuffle, then a stable sort: players equal on every key stay in the random order.
            var shuffled = playerIds.Count < 2
                ? playerIds.ToList()
                : OptionShuffle.DisplayOrder(playerIds.Count, random).Select(i => playerIds[i]).ToList();
            return shuffled
                .OrderByDescending(id => Get(positions, id))
                .ThenBy(id => answerMs.TryGetValue(id, out var ms) ? ms : long.MaxValue)
                .ThenByDescending(id => Get(scoresBefore, id))
                .ToList();
        }

        private static int Get(IReadOnlyDictionary<string, int> map, string id)
        {
            return map.TryGetValue(id, out var v) ? v : 0;
        }
    }
}
