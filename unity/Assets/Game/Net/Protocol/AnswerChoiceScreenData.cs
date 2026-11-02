using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "answerChoice" (PROTOCOL.md section 4).</summary>
    public sealed class AnswerChoiceScreenData
    {
        [JsonProperty("question")] public string Question { get; set; }

        /// <summary>The 4 option texts in display order; the index of each is the "choice" the phone sends.</summary>
        [JsonProperty("options")] public List<string> Options { get; set; } = new List<string>();

        /// <summary>The player's personal window T in ms (base window times their timer multiplier).</summary>
        [JsonProperty("windowMs")] public long WindowMs { get; set; }
    }
}
