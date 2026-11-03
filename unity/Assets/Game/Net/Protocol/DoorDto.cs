using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>One door of screen "topicVote" (PROTOCOL.md section 4): the topic title in the player's language.</summary>
    public sealed class DoorDto
    {
        [JsonProperty("title")] public string Title { get; set; }
    }
}
