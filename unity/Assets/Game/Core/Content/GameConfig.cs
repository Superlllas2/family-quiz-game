using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// The parts of content/config.json the game uses so far. Every number lives in that file (CLAUDE.md).
    /// Other keys are ignored here and added when a feature needs them.
    /// </summary>
    public sealed class GameConfig
    {
        [JsonProperty("configVersion", Required = Required.Always)]
        public int ConfigVersion { get; set; }

        [JsonProperty("players", Required = Required.Always)]
        public PlayersConfig Players { get; set; }

        [JsonProperty("net", Required = Required.Always)]
        public NetConfig Net { get; set; }
    }
}
