using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "roundEnd" (PROTOCOL.md section 4).</summary>
    public sealed class RoundEndScreenData
    {
        /// <summary>Shared rank after the round.</summary>
        [JsonProperty("rank")] public int Rank { get; set; }

        [JsonProperty("score")] public int Score { get; set; }

        /// <summary>The 1 based number of the next round, or null after the last round (the final race comes next).</summary>
        [JsonProperty("nextRound")] public int? NextRound { get; set; }
    }
}
