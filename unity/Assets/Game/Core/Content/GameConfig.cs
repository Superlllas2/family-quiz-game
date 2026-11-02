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

        [JsonProperty("timers", Required = Required.Always)]
        public TimersConfig Timers { get; set; }

        [JsonProperty("rounds", Required = Required.Always)]
        public RoundsConfig Rounds { get; set; }

        [JsonProperty("topics", Required = Required.Always)]
        public TopicsConfig Topics { get; set; }

        [JsonProperty("scoring", Required = Required.Always)]
        public ScoringConfig Scoring { get; set; }

        [JsonProperty("players", Required = Required.Always)]
        public PlayersConfig Players { get; set; }

        [JsonProperty("lobby", Required = Required.Always)]
        public LobbyConfig Lobby { get; set; }

        [JsonProperty("net", Required = Required.Always)]
        public NetConfig Net { get; set; }

        [JsonProperty("staging", Required = Required.Always)]
        public StagingConfig Staging { get; set; }
    }
}
