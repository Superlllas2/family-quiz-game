using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>A screen: the body of the "screen" message and snapshot.screen (PROTOCOL.md section 4).</summary>
    public sealed class ScreenDto
    {
        [JsonProperty("phaseId")] public long PhaseId { get; set; }
        [JsonProperty("id")] public string Id { get; set; }

        /// <summary>Null for screens without a countdown; always written, also when null.</summary>
        [JsonProperty("endsInMs")] public long? EndsInMs { get; set; }

        [JsonProperty("data")] public object Data { get; set; }
    }
}
