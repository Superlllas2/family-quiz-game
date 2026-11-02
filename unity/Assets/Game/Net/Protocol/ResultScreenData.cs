using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "result", shown in Reveal and Scores (PROTOCOL.md section 4).</summary>
    public sealed class ResultScreenData
    {
        [JsonProperty("correct")] public bool Correct { get; set; }
        [JsonProperty("points")] public int Points { get; set; }

        /// <summary>The right answer, for example "Right answer: A · Volga".</summary>
        [JsonProperty("correctText")] public string CorrectText { get; set; }

        [JsonProperty("streak")] public int Streak { get; set; }

        /// <summary>Shared rank after this question.</summary>
        [JsonProperty("rank")] public int Rank { get; set; }

        /// <summary>The held power up, or null (always written; power ups arrive on Day 4).</summary>
        [JsonProperty("powerUp")] public string PowerUp { get; set; }

        [JsonProperty("targets")] public List<PowerUpTargetDto> Targets { get; set; } = new List<PowerUpTargetDto>();
    }
}
