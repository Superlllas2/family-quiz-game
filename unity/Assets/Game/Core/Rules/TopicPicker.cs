using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>The topic vote (docs/02_show_flow.md, "Rules inside a round", 1; D153, D154): which doors are offered and which one wins.</summary>
    public static class TopicPicker
    {
        /// <summary>What the game remembers for the next vote (D154).</summary>
        public sealed class History
        {
            /// <summary>Topic ids offered on a door earlier in this game.</summary>
            public ICollection<string> OfferedInGame { get; set; } = new HashSet<string>();

            /// <summary>Categories of the topics chosen earlier in this game.</summary>
            public ICollection<string> ChosenCategories { get; set; } = new HashSet<string>();

            /// <summary>Every topic offered before, oldest offer first (saved across games and evenings).</summary>
            public IReadOnlyCollection<string> OfferedEver { get; set; } = new SeenList();
        }

        /// <summary>
        /// The doors for a round, in door order (D154). The last door is always the mixed topic (topics.mixedTopicId). The
        /// others (topics.doors - 1) take their shelf in the order screen, knowledge, life. For each, the candidates are
        /// the shelf's topics not already on a door of this vote that pass every rule: at least topics.minUnseenToOffer
        /// unseen questions (rule 0), not offered earlier in this game (1), category not chosen earlier in this game (2),
        /// at most one Russian topic among the doors (3), the doors not all young and not all old (4). The topic offered
        /// longest ago wins (never offered counts as longest ago); equal ones in seeded random order. With no candidate the
        /// rules are dropped from the last (4, 3, 2, 1, then 0); a shelf with no topic left takes one from any shelf.
        /// Fewer doors only when there are fewer topics than doors.
        /// </summary>
        public static List<TopicDef> Doors(IReadOnlyList<TopicDef> topics, IReadOnlyList<QuestionDef> bank, TopicsConfig config,
            History history, IReadOnlyCollection<string> seenQuestions, IRandom random)
        {
            int free = config.Doors - 1;
            var regular = topics.Where(t => !t.Mixed && t.Id != config.MixedTopicId).ToList();
            var doors = new List<TopicDef>();
            for (int i = 0; i < free; i++)
            {
                string shelf = TopicShelves.InDoorOrder[i % TopicShelves.InDoorOrder.Count];
                bool lastRegularDoor = i == free - 1 && free >= 2;
                var onShelf = regular.Where(t => t.Shelf == shelf && !doors.Contains(t)).ToList();
                var pick = Best(onShelf, doors, lastRegularDoor, bank, config, history, seenQuestions, random)
                           ?? Best(regular.Where(t => !doors.Contains(t)).ToList(), doors, lastRegularDoor, bank, config, history,
                               seenQuestions, random);
                if (pick == null) break;
                doors.Add(pick);
            }

            var mixed = topics.FirstOrDefault(t => t.Id == config.MixedTopicId);
            if (mixed != null) doors.Add(mixed);
            return doors;
        }

        /// <summary>The best topic of the pool under the rules, dropping rules from the last until one passes; null for an empty pool.</summary>
        private static TopicDef Best(List<TopicDef> pool, List<TopicDef> doors, bool lastRegularDoor, IReadOnlyList<QuestionDef> bank,
            TopicsConfig config, History history, IReadOnlyCollection<string> seenQuestions, IRandom random)
        {
            if (pool.Count == 0) return null;
            // A seeded shuffle, then a stable sort by the last offer: equal ones keep the random order.
            var ordered = Shuffled(pool, random)
                .OrderBy(t => SeenList.LastSeenIn(history.OfferedEver, t.Id) ?? -1L)
                .ToList();
            for (int rules = 4; rules >= -1; rules--)
            {
                foreach (var t in ordered)
                {
                    if (Passes(t, rules, doors, lastRegularDoor, bank, config, history, seenQuestions)) return t;
                }
            }

            return null;
        }

        /// <summary>True when the topic passes rules 0 to <paramref name="upTo"/> (-1: no rule).</summary>
        private static bool Passes(TopicDef t, int upTo, List<TopicDef> doors, bool lastRegularDoor, IReadOnlyList<QuestionDef> bank,
            TopicsConfig config, History history, IReadOnlyCollection<string> seenQuestions)
        {
            if (upTo >= 0 && UnseenCount(t.Id, bank, seenQuestions) < config.MinUnseenToOffer) return false;
            if (upTo >= 1 && history.OfferedInGame.Contains(t.Id)) return false;
            if (upTo >= 2 && t.Category != null && history.ChosenCategories.Contains(t.Category)) return false;
            if (upTo >= 3 && t.Russian && doors.Any(d => d.Russian)) return false;
            if (upTo >= 4 && lastRegularDoor && SameOneSidedLean(doors, t)) return false;
            return true;
        }

        /// <summary>True when every door so far and the candidate lean the same way, young or old (rule 4).</summary>
        private static bool SameOneSidedLean(List<TopicDef> doors, TopicDef candidate)
        {
            string lean = candidate.Lean;
            if (lean != TopicShelves.LeanYoung && lean != TopicShelves.LeanOld) return false;
            return doors.Count > 0 && doors.All(d => d.Lean == lean);
        }

        /// <summary>Questions of a topic (any format or difficulty) not in the seen list.</summary>
        public static int UnseenCount(string topicId, IReadOnlyList<QuestionDef> bank, IReadOnlyCollection<string> seen)
        {
            int n = 0;
            foreach (var q in bank)
            {
                if (q.Topic == topicId && !QuestionPicker.IsSeen(q, seen)) n++;
            }

            return n;
        }

        /// <summary>
        /// The winning door (D153): majority. A tie: the seeded random generator picks among the tied doors. Nobody voted:
        /// among all doors.
        /// </summary>
        /// <param name="votes">playerId to door index, in the order the votes came in.</param>
        public static int ResolveVote(IReadOnlyDictionary<string, int> votes, int doorCount, IRandom random)
        {
            var tied = TiedDoors(votes, doorCount);
            return tied.Count == 1 ? tied[0] : tied[random.Next(tied.Count)];
        }

        /// <summary>The doors with the most votes, in door order; every door when nobody voted.</summary>
        public static List<int> TiedDoors(IReadOnlyDictionary<string, int> votes, int doorCount)
        {
            var counts = new int[doorCount];
            foreach (var door in votes.Values)
            {
                if (door >= 0 && door < doorCount) counts[door]++;
            }

            int most = doorCount > 0 ? counts.Max() : 0;
            var tied = new List<int>();
            for (int d = 0; d < doorCount; d++)
            {
                if (counts[d] == most) tied.Add(d);
            }

            return tied;
        }

        private static List<T> Shuffled<T>(List<T> items, IRandom random)
        {
            if (items.Count < 2) return new List<T>(items);
            var order = OptionShuffle.DisplayOrder(items.Count, random);
            return order.Select(i => items[i]).ToList();
        }
    }
}
