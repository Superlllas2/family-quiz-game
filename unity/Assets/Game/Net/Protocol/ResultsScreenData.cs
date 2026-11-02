using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "results" (PROTOCOL.md section 4).</summary>
    public sealed class ResultsScreenData
    {
        /// <summary>Shared rank at the end of the game.</summary>
        [JsonProperty("place")] public int Place { get; set; }

        [JsonProperty("correctCount")] public int CorrectCount { get; set; }

        /// <summary>The awards this player won, as texts in their language.</summary>
        [JsonProperty("awards")] public List<string> Awards { get; set; } = new List<string>();
    }
}
