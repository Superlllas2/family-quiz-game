using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// content/ui_strings.json: the TV app's own texts, one key per text in both languages (D65).
    /// </summary>
    public sealed class UiStrings
    {
        public const string FileName = "ui_strings.json";

        /// <summary>docs/15_schemas.md: dotted lower camel case, tv.* or screen.*.</summary>
        private static readonly Regex KeyPattern = new Regex("^(tv|screen)(\\.[a-z][a-zA-Z0-9]*)+$");

        private static readonly Regex PlaceholderPattern = new Regex("\\{[a-zA-Z]+\\}");

        private readonly Dictionary<string, LocalizedText> _strings;

        private UiStrings(Dictionary<string, LocalizedText> strings)
        {
            _strings = strings;
        }

        public static UiStrings Parse(string json)
        {
            Dictionary<string, LocalizedText> strings;
            try
            {
                strings = JsonConvert.DeserializeObject<Dictionary<string, LocalizedText>>(json);
            }
            catch (JsonException e)
            {
                throw new ContentException(FileName, e.Message, e);
            }

            if (strings == null) throw new ContentException(FileName, "file is empty");
            var errors = new ContentErrors(FileName);
            foreach (var pair in strings)
            {
                errors.Check(KeyPattern.IsMatch(pair.Key), pair.Key,
                    "key must start with tv. or screen. and use dotted lower camel case");
                if (pair.Value == null || string.IsNullOrWhiteSpace(pair.Value.Ru) || string.IsNullOrWhiteSpace(pair.Value.En))
                    errors.Add(pair.Key, "must have non empty ru and en");
                else
                    errors.Check(Placeholders(pair.Value.Ru) == Placeholders(pair.Value.En), pair.Key,
                        "placeholders differ between ru and en");
            }

            errors.ThrowIfAny();
            return new UiStrings(strings);
        }

        /// <summary>The {name} style placeholders of a text, sorted and joined, to compare ru with en.</summary>
        private static string Placeholders(string text)
        {
            var found = new List<string>();
            foreach (Match m in PlaceholderPattern.Matches(text)) found.Add(m.Value);
            found.Sort(StringComparer.Ordinal);
            return string.Join(",", found);
        }

        public bool Has(string key)
        {
            return _strings.ContainsKey(key);
        }

        /// <summary>The text for a key in a language. A missing key returns the key itself so it shows up.</summary>
        public string Get(string key, string lang)
        {
            return _strings.TryGetValue(key, out var text) ? text.Get(lang) : key;
        }

        /// <summary>Get, then fill the {placeholders} with the given values.</summary>
        public string Format(string key, string lang, IReadOnlyDictionary<string, string> values)
        {
            var text = Get(key, lang);
            foreach (var pair in values) text = text.Replace("{" + pair.Key + "}", pair.Value);
            return text;
        }
    }
}
