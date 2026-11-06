using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "music": the background music on the TV (decisions/0019, D113, D114).</summary>
    public sealed class MusicConfig
    {
        /// <summary>The settings panel slider's default, 0 to 1, until the laptop remembers its own (tv_audio.json).</summary>
        [JsonProperty("volume", Required = Required.Always)] public double Volume { get; set; }

        /// <summary>The crossfade from one track to the next.</summary>
        [JsonProperty("fadeSeconds", Required = Required.Always)] public double FadeSeconds { get; set; }
    }
}
