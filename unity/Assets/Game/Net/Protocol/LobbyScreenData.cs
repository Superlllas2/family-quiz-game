using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "lobby" (PROTOCOL.md section 4).</summary>
    public sealed class LobbyScreenData
    {
        [JsonProperty("players")] public List<LobbyPlayerDto> Players { get; set; }
        /// <summary>This player is ready (D76).</summary>
        [JsonProperty("ready")] public bool Ready { get; set; }

        /// <summary>players.min or more players are connected.</summary>
        [JsonProperty("enoughPlayers")] public bool EnoughPlayers { get; set; }
    }
}
