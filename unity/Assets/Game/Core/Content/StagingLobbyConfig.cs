using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.lobby".</summary>
    public sealed class StagingLobbyConfig
    {
        [JsonProperty("seatPopSeconds", Required = Required.Always)] public double SeatPopSeconds { get; set; }
    }
}
