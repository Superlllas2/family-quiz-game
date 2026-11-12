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

            // The race has no statement limit (D157); it reuses statements only after every one was played in it, so the
            // file needs at least enough for the leader-less path to the finish: race.finishSteps.
            errors.Check(list.Count >= config.Race.FinishSteps, "-",
                "needs at least race.finishSteps (" + config.Race.FinishSteps + ") statements for one race, has " + list.Count);
            errors.ThrowIfAny();
            return list;
        }
    }
}
