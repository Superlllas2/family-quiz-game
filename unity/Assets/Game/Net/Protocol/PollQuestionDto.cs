using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One entry of poll.data.questions: {id, text, kind, options}; options only for choice questions.</summary>
    public sealed class PollQuestionDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("text")] public string Text { get; set; }
        [JsonProperty("kind")] public string Kind { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Options { get; set; }
    }
}
