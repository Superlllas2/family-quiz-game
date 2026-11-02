using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "players".</summary>
    public sealed class PlayersConfig
    {
        [JsonProperty("min", Required = Required.Always)]
        public int Min { get; set; }

        [JsonProperty("max", Required = Required.Always)]
        public int Max { get; set; }

        [JsonProperty("timerMultiplierOptions", Required = Required.Always)]
        public double[] TimerMultiplierOptions { get; set; }

        [JsonProperty("seatHoldSeconds", Required = Required.Always)]
        public double SeatHoldSeconds { get; set; }
    }
}
