using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Picks the question for a round slot (docs/02_show_flow.md, "Rules inside a round", 2). The slot's format and
    /// difficulty come from rounds.formatOrder and rounds.difficultyOrder. The question comes from the first of these
    /// groups that has one: 1 unseen from the chosen topic (any topic for the mixed door), 2 unseen from any topic, 3 seen
    /// from the chosen topic, 4 seen from any topic. Never a question already used in this game. Within the group the
    /// game's seeded random picks.
    /// </summary>
    public static class QuestionPicker
    {
        public const int GroupUnseenTopic = 1;
        public const int GroupUnseenAny = 2;
        public const int GroupSeenTopic = 3;
        public const int GroupSeenAny = 4;

        /// <param name="topicId">The chosen topic, or null for the mixed door (any topic).</param>
        /// <param name="group">The group the question came from (1 to 4).</param>
        public static QuestionDef Pick(IReadOnlyList<QuestionDef> bank, string format, string difficulty, string topicId,
            IReadOnlyCollection<string> seen, ICollection<string> usedInGame, IRandom random, out int group)
        {
            var groups = new List<QuestionDef>[4];
            for (int i = 0; i < groups.Length; i++) groups[i] = new List<QuestionDef>();
            foreach (var q in bank)
            {
                if (q.Format != format || q.Difficulty != difficulty || usedInGame.Contains(q.Id)) continue;
                bool inTopic = topicId == null || q.Topic == topicId;
                bool isSeen = Contains(seen, q.Id);
                if (!isSeen && inTopic) groups[0].Add(q);
                if (!isSeen) groups[1].Add(q);
                if (isSeen && inTopic) groups[2].Add(q);
                if (isSeen) groups[3].Add(q);
            }

            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].Count == 0) continue;
                group = i + 1;
                return groups[i][random.Next(groups[i].Count)];
            }

            throw new InvalidOperationException("questions.json has no unused " + format + " question of difficulty " + difficulty);
        }

        private static bool Contains(IReadOnlyCollection<string> set, string id)
        {
            if (set is ICollection<string> collection) return collection.Contains(id);
            foreach (var s in set)
            {
                if (s == id) return true;
            }

            return false;
        }
    }
}
