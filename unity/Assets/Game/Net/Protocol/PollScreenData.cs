using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "poll" (PROTOCOL.md section 4): the questions of content/poll.json in the player's language.</summary>
    public sealed class PollScreenData
    {
        [JsonProperty("questions")] public List<PollQuestionDto> Questions { get; set; } = new List<PollQuestionDto>();
    }
}
