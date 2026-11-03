using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/race.json (docs/15_schemas.md, tools/validate_content.mjs).</summary>
    public static class RaceLoader
    {
        public const string FileName = "race.json";

        public static IReadOnlyList<RaceStatementDef> Parse(string json, GameConfig config)
        {
            var errors = new ContentErrors(FileName);
            var items = JsonItems.Convert<RaceStatementDef>(JsonItems.ParseArray(FileName, json), errors);
            var list = new List<RaceStatementDef>();
            var ids = new HashSet<string>();
            foreach (var (s, _, id) in items)
            {
                errors.Check(ids.Add(s.Id), id, "id is used twice");
                errors.CheckText(id, "statement", s.Statement, ContentLimits.RaceStatementMax);
                errors.Check(!string.IsNullOrWhiteSpace(s.Source), id, "source is missing");
                list.Add(s);
            }

            // One race never repeats a statement: the file needs race.maxQuestions of them.
            errors.Check(list.Count >= config.Race.MaxQuestions, "-",
                "needs at least race.maxQuestions (" + config.Race.MaxQuestions + ") statements for one race, has " + list.Count);
            errors.ThrowIfAny();
            return list;
        }
    }
}
