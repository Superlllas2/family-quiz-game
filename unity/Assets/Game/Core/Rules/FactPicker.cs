using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>The winner's fact (D87): a random fact not shown before; once every fact was shown, any fact.</summary>
    public static class FactPicker
    {
        public static FactDef Pick(IReadOnlyList<FactDef> facts, IReadOnlyCollection<string> seen, IRandom random)
        {
            if (facts == null || facts.Count == 0) return null;
            var unseen = new List<FactDef>();
            foreach (var f in facts)
            {
                if (!seen.Contains(f.Id)) unseen.Add(f);
            }

            var group = unseen.Count > 0 ? unseen : new List<FactDef>(facts);
            return group[random.Next(group.Count)];
        }
    }
}
