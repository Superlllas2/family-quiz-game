using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/poll.json (docs/15_schemas.md "poll.json", docs/20_playtest.md).</summary>
    public sealed class PollQuestionDef
    {
        public const string KindChoice = "choice";
        public const string KindText = "text";

        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }

        /// <summary>choice (one of Options) or text (free text, at most 200 characters).</summary>
        [JsonProperty("kind", Required = Required.Always)] public string Kind { get; set; }

        [JsonProperty("text", Required = Required.Always)] public LocalizedText Text { get; set; }

        /// <summary>The options of a choice question; null for text.</summary>
        [JsonProperty("options")] public List<LocalizedText> Options { get; set; }

        public bool IsChoice => Kind == KindChoice;
    }
}
