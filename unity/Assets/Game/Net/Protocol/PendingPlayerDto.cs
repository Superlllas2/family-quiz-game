using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One entry of waiting.data.pending (PROTOCOL.md section 4).</summary>
    public sealed class PendingPlayerDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("characterId")] public int CharacterId { get; set; }
    }
}
