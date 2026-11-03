using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Telemetry
{
    /// <summary>
    /// The seen questions, race statements and winner facts on disk (docs/02_show_flow.md, "Seen questions"), so they survive the next
    /// game, Back to lobby and app restarts until "Reset seen questions". One JSON file:
    /// {"questions": [ids], "race": [ids], "facts": [ids]} (a file without "facts" loads with none). Load it before the
    /// Game is built; then, as a listener, it saves every list on
    /// every SeenChanged (written to a temp file, then moved over the old one, so a crash never leaves half a file).
    /// Never throws into the game: I/O errors go to the error callback.
    /// </summary>
    public sealed class SeenFile
    {
        public const string FileName = "seen.json";

        private readonly string _path;
        private readonly Action<string> _onError;
        private Game _game;

        public SeenFile(string path, Action<string> onError = null)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _onError = onError ?? (_ => { });
        }

        public string Path => _path;

        /// <summary>The saved lists; empty ones when the file is missing or broken (a broken file is reported).</summary>
        public (List<string> Questions, List<string> Race, List<string> Facts) Load()
        {
            var questions = new List<string>();
            var race = new List<string>();
            var facts = new List<string>();
            try
            {
                if (!File.Exists(_path)) return (questions, race, facts);
                var root = JObject.Parse(File.ReadAllText(_path, Encoding.UTF8));
                questions.AddRange(Ids(root["questions"]));
                race.AddRange(Ids(root["race"]));
                facts.AddRange(Ids(root["facts"]));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                _onError("cannot read " + _path + " (" + e.Message + "); starting with no seen questions");
            }

            return (questions, race, facts);
        }

        /// <summary>The game whose seen lists are saved from now on.</summary>
        public void Attach(Game game)
        {
            _game = game;
        }

        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (_game == null || events == null) return;
            foreach (var e in events)
            {
                if (!(e is SeenChanged)) continue;
                Save(_game.State.SeenQuestionIds, _game.State.SeenRaceIds, _game.State.SeenFactIds);
                return; // one save per batch is enough: it writes the whole lists
            }
        }

        public void Save(IEnumerable<string> questions, IEnumerable<string> race, IEnumerable<string> facts = null)
        {
            try
            {
                var root = new JObject
                {
                    ["questions"] = new JArray(questions.OrderBy(id => id, StringComparer.Ordinal)),
                    ["race"] = new JArray(race.OrderBy(id => id, StringComparer.Ordinal)),
                    ["facts"] = new JArray((facts ?? Enumerable.Empty<string>()).OrderBy(id => id, StringComparer.Ordinal))
                };
                var folder = System.IO.Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
                var temp = _path + ".tmp";
                File.WriteAllText(temp, root.ToString(Formatting.Indented), new UTF8Encoding(false));
                if (File.Exists(_path)) File.Delete(_path);
                File.Move(temp, _path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                _onError("cannot save " + _path + " (" + e.Message + ")");
            }
        }

        private static IEnumerable<string> Ids(JToken token)
        {
            if (!(token is JArray array)) yield break;
            foreach (var item in array)
            {
                if (item.Type == JTokenType.String) yield return (string)item;
            }
        }
    }
}
