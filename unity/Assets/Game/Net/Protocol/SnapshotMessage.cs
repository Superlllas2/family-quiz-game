using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class SnapshotMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "snapshot";
        [JsonProperty("player")] public PlayerDto Player { get; set; }
        [JsonProperty("screen")] public ScreenDto Screen { get; set; }
    }
}
