using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "feedback": the question rating card on the phones (decisions/0018, D108 to D110).</summary>
    public sealed class FeedbackConfig
    {
        /// <summary>The TV settings panel's default for question feedback.</summary>
        [JsonProperty("enabled", Required = Required.Always)] public bool Enabled { get; set; }

        /// <summary>Stars per row: the leftmost is 1, the rightmost is this.</summary>
        [JsonProperty("stars", Required = Required.Always)] public int Stars { get; set; }

        /// <summary>The rated items in card order; their texts are screen.rate.&lt;id&gt;.text, .low and .high.</summary>
        [JsonProperty("items", Required = Required.Always)] public List<string> Items { get; set; }
    }
}
