using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One entry of lobby.data.players (PROTOCOL.md section 4).</summary>
    public sealed class LobbyPlayerDto
    {
        [JsonProperty("playerId")] public string PlayerId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("characterId")] public int CharacterId { get; set; }
        [JsonProperty("ready")] public bool Ready { get; set; }
    }
}
