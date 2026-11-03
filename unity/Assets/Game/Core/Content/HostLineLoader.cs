using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/host_lines.json (docs/15_schemas.md, tools/validate_content.mjs).</summary>
    public static class HostLineLoader
    {
        public const string FileName = "host_lines.json";

        public static IReadOnlyList<HostLineDef> Parse(string json)
        {
            var errors = new ContentErrors(FileName);
            var items = JsonItems.Convert<HostLineDef>(JsonItems.ParseArray(FileName, json), errors);
            var list = new List<HostLineDef>();
            var ids = new HashSet<string>();
            foreach (var (l, _, id) in items)
            {
                errors.Check(ids.Add(l.Id), id, "id is used twice");
                errors.Check(HostIds.IsValid(l.Host), id, "host must be konstantin or ilja");
                errors.Check(HostTriggers.IsValid(l.Trigger), id, "trigger \"" + l.Trigger + "\" is not a known trigger");
                errors.CheckText(id, "text", l.Text, ContentLimits.HostLineTextMax);
                errors.Check(l.Weight > 0, id, "weight must be more than 0");
                list.Add(l);
            }

            foreach (var host in HostIds.All)
            {
                errors.Check(list.Exists(l => l.Host == host && l.Trigger == HostTriggers.GameStart), host,
                    "needs a gameStart line (the Intro plays one per host)");
            }

            errors.ThrowIfAny();
            return list;
        }
    }
}
