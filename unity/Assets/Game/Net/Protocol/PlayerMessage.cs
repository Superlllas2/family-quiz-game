using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// The "player" message (PROTOCOL.md section 4): the player object, under "player" as in snapshot (the phone
    /// page reads it there).
    /// </summary>
    public sealed class PlayerMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "player";
        [JsonProperty("player")] public PlayerDto Player { get; set; }
    }
}
