using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "results" (PROTOCOL.md section 4): sent only to the winner, with their fact (D87).</summary>
    public sealed class ResultsScreenData
    {
        /// <summary>The winner's reward fact in their language.</summary>
        [JsonProperty("fact")] public string Fact { get; set; }
    }
}
