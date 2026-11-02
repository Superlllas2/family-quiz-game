using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/topics.json (docs/15_schemas.md).</summary>
    public sealed class TopicDef
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("title", Required = Required.Always)] public LocalizedText Title { get; set; }

        /// <summary>True for the mixed door (topics.mixedTopicId), which draws from every topic.</summary>
        [JsonProperty("mixed")] public bool Mixed { get; set; }
    }
}
