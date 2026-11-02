using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "lobby" (PROTOCOL.md section 4).</summary>
    public sealed class LobbyScreenData
    {
        [JsonProperty("players")] public List<LobbyPlayerDto> Players { get; set; }
        [JsonProperty("canStart")] public bool CanStart { get; set; }
    }
}
