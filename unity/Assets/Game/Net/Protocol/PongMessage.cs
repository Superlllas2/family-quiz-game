using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class PongMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "pong";

        /// <summary>The phone's t, echoed exactly as it came.</summary>
        [JsonProperty("t")] public JToken T { get; set; }

        /// <summary>TV clock ms (the core clock).</summary>
        [JsonProperty("tv")] public long Tv { get; set; }
    }
}
