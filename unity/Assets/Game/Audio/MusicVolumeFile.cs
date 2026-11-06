using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// Remembers the music volume of the TV settings panel on this laptop (decisions/0019, D114): a small JSON file
    /// next to seen.json. A TV only choice, never a game setting, so it never goes through Core or the protocol. A
    /// missing or broken file, or a value outside 0 to 1, reads as the default (music.volume in config.json).
    /// </summary>
    public sealed class MusicVolumeFile
    {
        public const string FileName = "tv_audio.json";
        private const string Key = "musicVolume";

        private readonly string _path;
        private readonly float _default;

        public MusicVolumeFile(string path, float defaultVolume)
        {
            _path = path;
            _default = defaultVolume;
        }

        public float Load()
        {
            try
            {
                if (!File.Exists(_path)) return _default;
                var token = JObject.Parse(File.ReadAllText(_path))[Key];
                if (token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer)) return _default;
                var value = token.Value<float>();
                return value >= 0f && value <= 1f ? value : _default;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is Newtonsoft.Json.JsonException)
            {
                return _default;
            }
        }

        public void Save(float volume)
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_path, new JObject { [Key] = Math.Round(Clamp(volume), 3) }.ToString());
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // The slider still works for this evening; it is only not remembered.
            }
        }

        public static float Clamp(float volume)
        {
            return float.IsNaN(volume) ? 0f : Math.Max(0f, Math.Min(1f, volume));
        }
    }
}
