using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "pickCharacter" (PROTOCOL.md section 4).</summary>
    public sealed class PickCharacterScreenData
    {
        /// <summary>Character ids taken by the other seated players.</summary>
        [JsonProperty("taken")] public List<int> Taken { get; set; } = new List<int>();

        /// <summary>This player's character id (the one the TV assigned, or the one picked).</summary>
        [JsonProperty("current")] public int Current { get; set; }
    }
}
