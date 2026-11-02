using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>The "player" object (PROTOCOL.md section 4).</summary>
    public sealed class PlayerDto
    {
        [JsonProperty("playerId")] public string PlayerId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("characterId")] public int CharacterId { get; set; }
        [JsonProperty("lang")] public string Lang { get; set; }
        [JsonProperty("score")] public int Score { get; set; }
        [JsonProperty("rank")] public int Rank { get; set; }
        [JsonProperty("streak")] public int Streak { get; set; }
        [JsonProperty("heldPowerUp")] public string HeldPowerUp { get; set; }
        [JsonProperty("timerMultiplier")] public double TimerMultiplier { get; set; }
    }
}
