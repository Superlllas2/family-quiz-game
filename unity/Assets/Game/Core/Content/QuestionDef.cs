using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/questions.json (docs/15_schemas.md).</summary>
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

        /// <summary>Curated bank only (decisions/0020): the full bank question this one comes from, or null for a new one.</summary>
        [JsonProperty("basedOn")] public string BasedOn { get; set; }

        /// <summary>Curated bank only: same (an exact copy of BasedOn), rewrite (the same fact, a better question) or new.</summary>
        [JsonProperty("refine")] public string Refine { get; set; }

        /// <summary>Multiple choice: 4 options in file order.</summary>
        [JsonProperty("options")] public List<LocalizedText> Options { get; set; }

        /// <summary>Multiple choice: index of the right option in Options (0 to 3).</summary>
        [JsonProperty("correct")] public int Correct { get; set; } = -1;

        /// <summary>Closest value: the true value.</summary>
        [JsonProperty("answer")] public double? Answer { get; set; }

        /// <summary>Closest value: slider limits [min, max]; the answer lies strictly inside.</summary>
        [JsonProperty("range")] public double[] Range { get; set; }

        /// <summary>Closest value: slider step.</summary>
        [JsonProperty("step")] public double? Step { get; set; }

        /// <summary>Closest value: distance at which points reach zero.</summary>
        [JsonProperty("tolerance")] public double? Tolerance { get; set; }

        /// <summary>Closest value: the unit, for example {"ru": "год", "en": "year"}; may be empty strings.</summary>
        [JsonProperty("unit")] public LocalizedText Unit { get; set; }
    }
}
