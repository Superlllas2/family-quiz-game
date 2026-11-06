using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One row of the rating card (PROTOCOL.md section 4, D110).</summary>
    public sealed class RateItemDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("low")] public string Low { get; set; }
        [JsonProperty("high")] public string High { get; set; }

        /// <summary>The player's latest accepted stars for this item, 0 if none.</summary>
        [JsonProperty("given")] public int Given { get; set; }
    }
}
