using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class AckMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "ack";
        [JsonProperty("inputId")] public string InputId { get; set; }
        [JsonProperty("ok")] public bool Ok { get; set; }

        /// <summary>Left out when there is no reason (PROTOCOL.md: "reason (if not ok)"; "duplicate" is ok with a reason).</summary>
        [JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
        public string Reason { get; set; }
    }
}
