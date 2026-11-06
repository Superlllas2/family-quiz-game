using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>The optional "rate" field of watch, waiting and result in Reveal and Scores (PROTOCOL.md section 4, D109).</summary>
    public sealed class RateDto
    {
        [JsonProperty("questionId")] public string QuestionId { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("close")] public string Close { get; set; }

        /// <summary>Stars per row (feedback.stars): the leftmost is 1, the rightmost is this.</summary>
        [JsonProperty("stars")] public int Stars { get; set; }

        [JsonProperty("items")] public List<RateItemDto> Items { get; set; } = new List<RateItemDto>();
    }
}
