using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Picks the question for a slot (D75, Day 2): multiple choice only, from the whole bank. The slot's difficulty comes
    /// from rounds.difficultyOrder; a slot whose rounds.formatOrder format is not multipleChoice takes a medium multiple
    /// choice question instead. Unseen in this session first, then seen; never a question already used in this game.
    /// Within a group the game's seeded random picks.
    /// </summary>
    public static class QuestionPicker
    {
        public static QuestionDef Pick(IReadOnlyList<QuestionDef> bank, RoundsConfig rounds, int slot,
            IReadOnlyCollection<string> seen, ICollection<string> usedInGame, IRandom random)
        {
            string difficulty = SlotDifficulty(rounds, slot);
            var unseen = new List<QuestionDef>();
            var seenOnes = new List<QuestionDef>();
            foreach (var q in bank)
            {
                if (q.Format != QuestionDef.MultipleChoice || q.Difficulty != difficulty || usedInGame.Contains(q.Id)) continue;
                if (Contains(seen, q.Id)) seenOnes.Add(q);
                else unseen.Add(q);
            }

            var group = unseen.Count > 0 ? unseen : seenOnes;
            if (group.Count == 0)
                throw new InvalidOperationException("questions.json has no unused multipleChoice question of difficulty " + difficulty);
            return group[random.Next(group.Count)];
        }

        /// <summary>The difficulty a slot draws (D75): its rounds.difficultyOrder entry, or medium for a non multiple choice slot.</summary>
        public static string SlotDifficulty(RoundsConfig rounds, int slot)
        {
            return rounds.FormatOrder[slot] == QuestionDef.MultipleChoice ? rounds.DifficultyOrder[slot] : Difficulties.Medium;
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
