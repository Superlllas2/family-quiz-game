using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/questions.json (docs/15_schemas.md). Closest value fields arrive on Day 3.</summary>
    public sealed class QuestionDef
    {
        public const string MultipleChoice = "multipleChoice";
        public const string ClosestValue = "closestValue";

        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("topic", Required = Required.Always)] public string Topic { get; set; }
        [JsonProperty("format", Required = Required.Always)] public string Format { get; set; }
        [JsonProperty("difficulty", Required = Required.Always)] public string Difficulty { get; set; }
        [JsonProperty("text", Required = Required.Always)] public LocalizedText Text { get; set; }
        [JsonProperty("fact")] public LocalizedText Fact { get; set; }
        [JsonProperty("source", Required = Required.Always)] public string Source { get; set; }
        [JsonProperty("kids")] public bool? Kids { get; set; }

        /// <summary>Multiple choice: 4 options in file order.</summary>
        [JsonProperty("options")] public List<LocalizedText> Options { get; set; }

        /// <summary>Multiple choice: index of the right option in Options (0 to 3).</summary>
        [JsonProperty("correct")] public int Correct { get; set; } = -1;
    }
}
