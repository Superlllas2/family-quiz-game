using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>The topic vote (docs/02_show_flow.md, "Rules inside a round", 1): which doors are offered and which one wins.</summary>
    public static class TopicPicker
    {
        /// <summary>
        /// The doors for a round, in door order. The last door is always the mixed topic (topics.mixedTopicId). The others
        /// (topics.doors - 1): the candidates are the topics not chosen earlier in this game; among the candidates with at
        /// least topics.minUnseenToOffer unseen questions, the seeded random generator chooses. If fewer qualify, every
        /// qualifying one is taken and the free doors are filled with the other candidates that have the most unseen
        /// questions, then with topics chosen earlier in this game, most unseen first; equal counts in seeded random order.
        /// Fewer doors only when there are fewer topics than doors.
        /// </summary>
        /// <param name="chosenEarlier">Topic ids chosen in earlier rounds of this game (the mixed topic may be among them).</param>
        /// <param name="seen">Question ids already shown (saved across games).</param>
        public static List<TopicDef> Doors(IReadOnlyList<TopicDef> topics, IReadOnlyList<QuestionDef> bank, TopicsConfig config,
            ICollection<string> chosenEarlier, IReadOnlyCollection<string> seen, IRandom random)
        {
            int free = config.Doors - 1;
            var regular = topics.Where(t => !t.Mixed && t.Id != config.MixedTopicId).ToList();
            var candidates = regular.Where(t => !chosenEarlier.Contains(t.Id)).ToList();
            var qualified = candidates.Where(t => UnseenCount(t.Id, bank, seen) >= config.MinUnseenToOffer).ToList();

            var doors = new List<TopicDef>();
            if (qualified.Count >= free)
            {
                // Choose free of them with the seeded random generator, in the order drawn.
                var pool = new List<TopicDef>(qualified);
                while (doors.Count < free)
                {
                    int i = random.Next(pool.Count);
                    doors.Add(pool[i]);
                    pool.RemoveAt(i);
                }
            }
            else
            {
                doors.AddRange(Shuffled(qualified, random));
                var others = candidates.Where(t => !qualified.Contains(t)).ToList();
                doors.AddRange(MostUnseenFirst(others, bank, seen, random).Take(free - doors.Count));
                if (doors.Count < free)
                {
                    var earlier = regular.Where(t => chosenEarlier.Contains(t.Id)).ToList();
                    doors.AddRange(MostUnseenFirst(earlier, bank, seen, random).Take(free - doors.Count));
                }
            }

            var mixed = topics.FirstOrDefault(t => t.Id == config.MixedTopicId);
            if (mixed != null) doors.Add(mixed);
            return doors;
        }

        /// <summary>Questions of a topic (any format or difficulty) not in the seen list.</summary>
        public static int UnseenCount(string topicId, IReadOnlyList<QuestionDef> bank, IReadOnlyCollection<string> seen)
        {
            int n = 0;
            foreach (var q in bank)
            {
                if (q.Topic == topicId && !Contains(seen, q.Id)) n++;
            }

            return n;
        }

        /// <summary>
        /// The winning door: majority. A tie is decided by the lowest ranked player (the highest rank number) among those
        /// who voted for one of the tied doors; several of them on that rank: one picked with the seeded random generator.
        /// Nobody voted: the seeded random generator picks one of the doors.
        /// </summary>
        /// <param name="votes">playerId to door index, in the order the votes came in.</param>
        /// <param name="rankOf">The shared rank of a player (docs/03_rules.md, "Ranks").</param>
        public static int ResolveVote(IReadOnlyDictionary<string, int> votes, int doorCount, Func<string, int> rankOf, IRandom random)
        {
            var counts = new int[doorCount];
            foreach (var door in votes.Values)
            {
                if (door >= 0 && door < doorCount) counts[door]++;
            }

            int most = counts.Max();
            if (most == 0) return random.Next(doorCount);
            var tied = new HashSet<int>();
            for (int d = 0; d < doorCount; d++)
            {
                if (counts[d] == most) tied.Add(d);
            }

            if (tied.Count == 1) return tied.First();

            // Voters for the tied doors in a fixed order (by playerId), so the seeded pick does not depend on vote order.
            var voters = votes.Where(v => tied.Contains(v.Value)).OrderBy(v => v.Key, StringComparer.Ordinal).ToList();
            int lowest = voters.Max(v => rankOf(v.Key));
            var deciders = voters.Where(v => rankOf(v.Key) == lowest).ToList();
            return deciders.Count == 1 ? deciders[0].Value : deciders[random.Next(deciders.Count)].Value;
        }

        private static List<TopicDef> MostUnseenFirst(List<TopicDef> topics, IReadOnlyList<QuestionDef> bank,
            IReadOnlyCollection<string> seen, IRandom random)
        {
            // A seeded shuffle, then a stable sort by unseen count: equal counts keep the random order.
            return Shuffled(topics, random).OrderByDescending(t => UnseenCount(t.Id, bank, seen)).ToList();
        }

        private static List<T> Shuffled<T>(List<T> items, IRandom random)
        {
            if (items.Count < 2) return new List<T>(items);
            var order = OptionShuffle.DisplayOrder(items.Count, random);
            return order.Select(i => items[i]).ToList();
        }

        private static bool Contains(IReadOnlyCollection<string> set, string id)
        {
            if (set is ICollection<string> collection) return collection.Contains(id);
            return set.Contains(id);
        }
    }
}
