using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/host_lines.json (docs/15_schemas.md, "host_lines.json").</summary>
    public sealed class HostLineDef
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("host", Required = Required.Always)] public string Host { get; set; }
        [JsonProperty("trigger", Required = Required.Always)] public string Trigger { get; set; }

        /// <summary>At most 90 characters per language; may contain {name} and {target}.</summary>
        [JsonProperty("text", Required = Required.Always)] public LocalizedText Text { get; set; }

        /// <summary>Relative chance among the lines still in the bag; default 1.</summary>
        [JsonProperty("weight")] public double Weight { get; set; } = 1;
    }
}
