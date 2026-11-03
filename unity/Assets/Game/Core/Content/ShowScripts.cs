using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// content/show_scripts.json: the segments of each mode, in order (docs/15_schemas.md). Standard is the only mode
    /// for game night 1; its rounds must match rounds.count (one multiplier per round, docs/03_rules.md "Modes"). The
    /// other modes are checked for known segment names only.
    /// </summary>
    public sealed class ShowScripts
    {
        public const string FileName = "show_scripts.json";
        public const string Standard = "standard";

        private readonly Dictionary<string, IReadOnlyList<string>> _modes;

        private ShowScripts(Dictionary<string, IReadOnlyList<string>> modes)
        {
            _modes = modes;
        }

        /// <summary>The segments of a mode, or null when the file has no such mode.</summary>
        public IReadOnlyList<string> Mode(string mode)
        {
            return _modes.TryGetValue(mode, out var segments) ? segments : null;
        }

        public static ShowScripts Parse(string json, GameConfig config)
        {
            var root = JsonItems.ParseObject(FileName, json);
            var errors = new ContentErrors(FileName);
            var modes = new Dictionary<string, IReadOnlyList<string>>();
            foreach (var pair in root)
            {
                List<string> segments;
                try
                {
                    segments = pair.Value.ToObject<List<string>>();
                }
                catch (JsonException)
                {
                    segments = null;
                }

                if (segments == null || segments.Count == 0)
                {
                    errors.Add(pair.Key, "must be a non empty list of segment names");
                    continue;
                }

                foreach (var s in segments) errors.Check(ShowSegments.IsKnown(s), pair.Key, "unknown segment \"" + s + "\"");
                modes[pair.Key] = segments;
            }

            if (!modes.TryGetValue(Standard, out var standard))
            {
                errors.Add(Standard, "the standard mode is missing");
            }
            else
            {
                int rounds = Count(standard, ShowSegments.Round);
                errors.Check(rounds == config.Rounds.Count, Standard,
                    "has " + rounds + " rounds, must match rounds.count (" + config.Rounds.Count + ")");
                errors.Check(Count(standard, ShowSegments.FinalRace) <= 1, Standard, "may hold at most one finalRace");
                errors.Check(Count(standard, ShowSegments.Results) == 1, Standard, "must hold exactly one results");
            }

            errors.ThrowIfAny();
            return new ShowScripts(modes);
        }

        private static int Count(IReadOnlyList<string> segments, string name)
        {
            int n = 0;
            foreach (var s in segments)
            {
                if (s == name) n++;
            }

            return n;
        }
    }
}
