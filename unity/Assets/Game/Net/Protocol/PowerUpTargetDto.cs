using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One entry of result.data.targets (PROTOCOL.md section 4). Power ups arrive on Day 4.</summary>
    public sealed class PowerUpTargetDto
    {
        [JsonProperty("playerId")] public string PlayerId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("available")] public bool Available { get; set; }
    }
}
