using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "topicVote" (PROTOCOL.md section 4): four doors; the last one is the mixed door.</summary>
    public sealed class TopicVoteScreenData
    {
        /// <summary>The doors in door order; the index of each is the "door" the phone sends.</summary>
        [JsonProperty("doors")] public List<DoorDto> Doors { get; set; } = new List<DoorDto>();
    }
}
