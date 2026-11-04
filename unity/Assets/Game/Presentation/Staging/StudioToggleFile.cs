using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// Remembers the TV settings panel's 3D set toggle on this laptop (decisions 0016): a small JSON file next to
    /// seen.json. A TV only choice, never a game setting, so it never goes through Core or the protocol. On by default;
    /// a missing or broken file reads as on.
    /// </summary>
    public sealed class StudioToggleFile
    {
        public const string FileName = "tv_view.json";

        private readonly string _path;

        public StudioToggleFile(string path)
        {
            _path = path;
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(_path)) return true;
                var token = JObject.Parse(File.ReadAllText(_path))["studio3d"];
                return token == null || token.Type != JTokenType.Boolean || token.Value<bool>();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is Newtonsoft.Json.JsonException)
            {
                return true;
            }
        }

        public void Save(bool on)
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_path, new JObject { ["studio3d"] = on }.ToString());
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // The toggle still works for this evening; it is only not remembered.
            }
        }
    }
}
