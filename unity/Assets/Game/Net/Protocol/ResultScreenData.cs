using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// screen.data for id "result" (PROTOCOL.md section 4): the power up pick in Reveal and Scores, sent only to a player
    /// holding a power up (D88). Power ups come on Day 4; until then nobody holds one and the screen is never sent.
    /// </summary>
    public sealed class ResultScreenData
    {
        /// <summary>The held power up.</summary>
        [JsonProperty("powerUp")] public string PowerUp { get; set; }

        [JsonProperty("targets")] public List<PowerUpTargetDto> Targets { get; set; } = new List<PowerUpTargetDto>();
    }
}
