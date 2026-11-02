using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class WelcomeMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "welcome";
        [JsonProperty("playerId")] public string PlayerId { get; set; }
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("room")] public string Room { get; set; }
    }
}
