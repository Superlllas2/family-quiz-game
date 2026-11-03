using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "closestValue" (docs/03_rules.md, "Closest value").</summary>
    public sealed class ClosestValueConfig
    {
        /// <summary>Points for the closest guess or guesses, before the round multiplier.</summary>
        [JsonProperty("closestBonus", Required = Required.Always)] public int ClosestBonus { get; set; }

        /// <summary>Distance points (before the multiplier) for a guess exactly on the tolerance edge.</summary>
        [JsonProperty("minPoints", Required = Required.Always)] public int MinPoints { get; set; }

        /// <summary>The exact band as a share of the tolerance: a guess this close or closer earns scoring.maxPoints.</summary>
        [JsonProperty("exactShare", Required = Required.Always)] public double ExactShare { get; set; }
    }
}
