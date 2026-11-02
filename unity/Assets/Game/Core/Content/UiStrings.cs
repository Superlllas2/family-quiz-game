using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// content/ui_strings.json: the TV app's own texts, one key per text in both languages (D65).
    /// </summary>
    public sealed class UiStrings
    {
        public const string FileName = "ui_strings.json";

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
            foreach (var pair in strings)
            {
                if (pair.Value == null || string.IsNullOrWhiteSpace(pair.Value.Ru) || string.IsNullOrWhiteSpace(pair.Value.En))
                    throw new ContentException(FileName, pair.Key + ": must have non empty ru and en");
            }

            return new UiStrings(strings);
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
