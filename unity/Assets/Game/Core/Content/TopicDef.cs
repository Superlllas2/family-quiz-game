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

        /// <summary>The door it stands on (D154): screen (door 1), knowledge (door 2) or life (door 3). Null for the mixed topic.</summary>
        [JsonProperty("shelf")] public string Shelf { get; set; }

        /// <summary>Topics that feel alike share a category; once one is chosen, no topic of it is offered again in the game.</summary>
        [JsonProperty("category")] public string Category { get; set; }

        /// <summary>young, old or all: who it suits more. A vote's three doors are never all young or all old.</summary>
        [JsonProperty("lean")] public string Lean { get; set; }

        /// <summary>About Russia or the USSR: at most one per vote.</summary>
        [JsonProperty("russian")] public bool Russian { get; set; }
    }
}
