using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Dialogue
{
    /// <summary>
    /// The host line shuffle bags on disk, so lines do not repeat across evenings (docs/15_schemas.md, "Selection" rule 3).
    /// One JSON file: {"shown": [line ids]}. A missing file means every bag is full; a broken one is reported and treated
    /// the same. Saved by writing a temp file and moving it over the old one, so a crash never leaves half a file. Never
    /// throws into the game: I/O errors go to the error callback. Ids no longer in host_lines.json are ignored by
    /// <see cref="HostDialogue"/>.
    /// </summary>
    public sealed class HostBagFile : IHostBagStore
    {
        public const string FileName = "host_bags.json";
        private const string ShownKey = "shown";

        private readonly string _path;
        private readonly Action<string> _onError;

        public HostBagFile(string path, Action<string> onError = null)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _onError = onError ?? (_ => { });
        }

        public string Path => _path;

        public IReadOnlyCollection<string> Load()
        {
            var shown = new List<string>();
            try
            {
                if (!File.Exists(_path)) return shown;
                var root = JObject.Parse(File.ReadAllText(_path, Encoding.UTF8));
                if (root[ShownKey] is JArray array)
                {
                    foreach (var item in array)
                    {
                        if (item.Type == JTokenType.String) shown.Add((string)item);
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is JsonException)
            {
                _onError("cannot read " + _path + " (" + e.Message + "); starting with full host line bags");
            }

            return shown;
        }

        public void Save(IReadOnlyCollection<string> shownIds)
        {
            try
            {
                var root = new JObject { [ShownKey] = new JArray(shownIds.OrderBy(id => id, StringComparer.Ordinal)) };
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
    }
}
