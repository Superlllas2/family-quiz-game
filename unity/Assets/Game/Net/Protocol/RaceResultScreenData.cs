using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "raceResult" (PROTOCOL.md section 4), shown in the race reveal.</summary>
    public sealed class RaceResultScreenData
    {
        /// <summary>True when the player's answer was right (they moved).</summary>
        [JsonProperty("correct")] public bool Correct { get; set; }

        /// <summary>The player's step after the move.</summary>
        [JsonProperty("position")] public int Position { get; set; }

        /// <summary>The finish step (race.finishSteps), to draw the small track.</summary>
        [JsonProperty("finish")] public int Finish { get; set; }
    }
}
