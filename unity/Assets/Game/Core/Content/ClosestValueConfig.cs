using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "closestValue" (docs/03_rules.md, "Closest value").</summary>
    public sealed class ClosestValueConfig
    {
        /// <summary>Points for the closest guess or guesses, before the round multiplier.</summary>
        [JsonProperty("closestBonus", Required = Required.Always)] public int ClosestBonus { get; set; }
    }
}
