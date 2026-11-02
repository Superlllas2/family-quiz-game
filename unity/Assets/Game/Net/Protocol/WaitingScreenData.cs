using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "waiting" (PROTOCOL.md section 4).</summary>
    public sealed class WaitingScreenData
    {
        /// <summary>Text of what was locked, for example "B · Danube".</summary>
        [JsonProperty("locked")] public string Locked { get; set; }

        /// <summary>The connected players still answering.</summary>
        [JsonProperty("pending")] public List<PendingPlayerDto> Pending { get; set; } = new List<PendingPlayerDto>();
    }
}
