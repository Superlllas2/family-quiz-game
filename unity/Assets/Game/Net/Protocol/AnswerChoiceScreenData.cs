using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "answerChoice" (PROTOCOL.md section 4). No question text: it is on the TV only (D88).</summary>
    public sealed class AnswerChoiceScreenData
    {
        /// <summary>The 4 option texts in display order; the index of each is the "choice" the phone sends.</summary>
        [JsonProperty("options")] public List<string> Options { get; set; } = new List<string>();

        /// <summary>The player's personal window T in ms (base window times their timer multiplier).</summary>
        [JsonProperty("windowMs")] public long WindowMs { get; set; }
    }
}
