using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "answerTrueFalse" (PROTOCOL.md section 4): True and False for a race statement, which is on the TV only (D88).</summary>
    public sealed class AnswerTrueFalseScreenData
    {
        /// <summary>The player's personal window T in ms (race.answer times their timer multiplier).</summary>
        [JsonProperty("windowMs")] public long WindowMs { get; set; }
    }
}
