using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "answerSlider" (PROTOCOL.md section 4; docs/04_formats.md, closest value).</summary>
    public sealed class AnswerSliderScreenData
    {
        [JsonProperty("question")] public string Question { get; set; }
        [JsonProperty("min")] public double Min { get; set; }
        [JsonProperty("max")] public double Max { get; set; }
        [JsonProperty("step")] public double Step { get; set; }

        /// <summary>The unit in the player's language; may be empty.</summary>
        [JsonProperty("unit")] public string Unit { get; set; }

        /// <summary>The player's personal window T in ms (base window times their timer multiplier).</summary>
        [JsonProperty("windowMs")] public long WindowMs { get; set; }
    }
}
