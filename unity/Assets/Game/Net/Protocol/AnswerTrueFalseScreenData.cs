using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "answerTrueFalse" (PROTOCOL.md section 4): a race statement.</summary>
    public sealed class AnswerTrueFalseScreenData
    {
        [JsonProperty("statement")] public string Statement { get; set; }

        /// <summary>The player's personal window T in ms (race.answer times their timer multiplier).</summary>
        [JsonProperty("windowMs")] public long WindowMs { get; set; }
    }
}
