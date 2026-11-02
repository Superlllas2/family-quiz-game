using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "topics" (docs/02_show_flow.md, "TopicVote").</summary>
    public sealed class TopicsConfig
    {
        [JsonProperty("doors", Required = Required.Always)] public int Doors { get; set; }
        [JsonProperty("minUnseenToOffer", Required = Required.Always)] public int MinUnseenToOffer { get; set; }
        [JsonProperty("mixedTopicId", Required = Required.Always)] public string MixedTopicId { get; set; }
    }
}
