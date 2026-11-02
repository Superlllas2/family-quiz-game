using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    public sealed class ScreenMessage
    {
        public ScreenMessage(ScreenDto screen)
        {
            PhaseId = screen.PhaseId;
            Id = screen.Id;
            EndsInMs = screen.EndsInMs;
            Data = screen.Data;
        }

        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "screen";
        [JsonProperty("phaseId")] public long PhaseId { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("endsInMs")] public long? EndsInMs { get; set; }
        [JsonProperty("data")] public object Data { get; set; }
    }
}
