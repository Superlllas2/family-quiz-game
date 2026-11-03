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
        /// Both conditions of docs/02_show_flow.md "Check": (leader's head start - last place's head start) is less than
        /// race.maxQuestions, and (race.finishSteps - last place's head start) is at most race.maxQuestions.
        /// </summary>
        public static bool LastPlaceCanWin(IEnumerable<int> headStarts, RaceConfig race)
        {
            var list = headStarts.ToList();
            if (list.Count == 0) return true;
            int leader = list.Max();
            int last = list.Min();
            return leader - last < race.MaxQuestions && race.FinishSteps - last <= race.MaxQuestions;
        }

        /// <summary>
        /// The next statement: unseen first, then seen, never one already used in this race; the seeded random generator
        /// picks within the group. Null when every statement was used.
        /// </summary>
        public static RaceStatementDef PickStatement(IReadOnlyList<RaceStatementDef> statements, IReadOnlyCollection<string> seen,
            ICollection<string> usedInGame, IRandom random)
        {
            var unseen = new List<RaceStatementDef>();
            var seenOnes = new List<RaceStatementDef>();
            foreach (var s in statements)
            {
                if (usedInGame.Contains(s.Id)) continue;
                if (seen.Contains(s.Id)) seenOnes.Add(s);
                else unseen.Add(s);
            }

            var group = unseen.Count > 0 ? unseen : seenOnes;
            return group.Count == 0 ? null : group[random.Next(group.Count)];
        }

        /// <summary>The new step after a statement: one race.stepsPerCorrect move for a right answer, never past the finish.</summary>
        public static int Move(int position, bool correct, RaceConfig race)
        {
            if (!correct) return position;
            return Math.Min(race.FinishSteps, position + race.StepsPerCorrect);
        }

        /// <summary>The race ends when a player reached race.finishSteps or race.maxQuestions statements were played.</summary>
        public static bool IsFinished(IEnumerable<int> positions, int statementsPlayed, RaceConfig race)
        {
            return statementsPlayed >= race.MaxQuestions || positions.Any(p => p >= race.FinishSteps);
        }

        /// <summary>
        /// Every player from first to last: furthest first; ties by the higher total score before the race, then the
        /// faster total answer time in the race, then a seeded random pick. The first one is the race (and game) winner.
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
                .ThenByDescending(id => Get(scoresBefore, id))
                .ThenBy(id => answerMs.TryGetValue(id, out var ms) ? ms : long.MaxValue)
                .ToList();
        }

        private static int Get(IReadOnlyDictionary<string, int> map, string id)
        {
            return map.TryGetValue(id, out var v) ? v : 0;
        }
    }
}
