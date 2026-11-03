using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One player button of the Intro prediction on screen "getReady" (PROTOCOL.md section 4).</summary>
    public sealed class PredictionPlayerDto
    {
        [JsonProperty("playerId")] public string PlayerId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("characterId")] public int CharacterId { get; set; }
    }
}
