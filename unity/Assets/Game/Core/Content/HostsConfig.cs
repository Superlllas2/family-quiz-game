using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "hosts" (docs/15_schemas.md, "Selection").</summary>
    public sealed class HostsConfig
    {
        /// <summary>Silence per host after a bubble ends before that host speaks again.</summary>
        [JsonProperty("minGapSeconds", Required = Required.Always)] public double MinGapSeconds { get; set; }

        /// <summary>How long a text bubble stays.</summary>
        [JsonProperty("bubbleSeconds", Required = Required.Always)] public double BubbleSeconds { get; set; }

        /// <summary>The streak length that fires streak3.</summary>
        [JsonProperty("streakTrigger", Required = Required.Always)] public int StreakTrigger { get; set; }
    }
}
