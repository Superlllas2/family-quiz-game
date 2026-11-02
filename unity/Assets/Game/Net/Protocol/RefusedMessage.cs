using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class RefusedMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "refused";
        [JsonProperty("reason")] public string Reason { get; set; }
    }
}
