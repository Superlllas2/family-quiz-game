using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.lobby".</summary>
    public sealed class StagingLobbyConfig
    {
        [JsonProperty("seatPopSeconds", Required = Required.Always)] public double SeatPopSeconds { get; set; }

        /// <summary>Each camera cut of the Lobby tour, fades included (D160).</summary>
        [JsonProperty("cutSeconds", Required = Required.Always)] public double CutSeconds { get; set; }

        /// <summary>The fade out at the end and the fade in at the start of each Lobby cut (D160).</summary>
        [JsonProperty("fadeSeconds", Required = Required.Always)] public double FadeSeconds { get; set; }

        /// <summary>How much darker the studio is in the Lobby, in exposure stops (D160).</summary>
        [JsonProperty("darkStops", Required = Required.Always)] public double DarkStops { get; set; }
    }
}
