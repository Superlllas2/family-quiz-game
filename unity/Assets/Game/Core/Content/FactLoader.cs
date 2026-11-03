using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/facts.json (docs/15_schemas.md, tools/validate_content.mjs).</summary>
    public static class FactLoader
    {
        public const string FileName = "facts.json";

        public static IReadOnlyList<FactDef> Parse(string json)
        {
            var errors = new ContentErrors(FileName);
            var items = JsonItems.Convert<FactDef>(JsonItems.ParseArray(FileName, json), errors);
            var list = new List<FactDef>();
            var ids = new HashSet<string>();
            foreach (var (f, _, id) in items)
            {
                errors.Check(ids.Add(f.Id), id, "id is used twice");
                errors.CheckText(id, "text", f.Text, ContentLimits.FactTextMax);
                errors.Check(!string.IsNullOrWhiteSpace(f.Source), id, "source is missing");
                list.Add(f);
            }

            errors.Check(list.Count >= 1, "-", "needs at least one fact");
            errors.ThrowIfAny();
            return list;
        }
    }
}
