using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "scoring" (docs/03_rules.md, "Scoring").</summary>
    public sealed class ScoringConfig
    {
        [JsonProperty("maxPoints", Required = Required.Always)] public int MaxPoints { get; set; }
        [JsonProperty("speedPenalty", Required = Required.Always)] public double SpeedPenalty { get; set; }
        [JsonProperty("roundTo", Required = Required.Always)] public int RoundTo { get; set; }
        [JsonProperty("fastAnswerMs", Required = Required.Always)] public int FastAnswerMs { get; set; }
    }
}
