using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "poll" (docs/20_playtest.md).</summary>
    public sealed class PollConfig
    {
        [JsonProperty("enabled", Required = Required.Always)] public bool Enabled { get; set; }

        /// <summary>The poll ends after this even if phones have not submitted.</summary>
        [JsonProperty("maxSeconds", Required = Required.Always)] public double MaxSeconds { get; set; }
    }
}
