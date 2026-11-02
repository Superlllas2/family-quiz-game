using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.extraSeconds": seconds added to a phase budget (0 for game night 1).</summary>
    public sealed class StagingExtraConfig
    {
        [JsonProperty("questionShow", Required = Required.Always)] public double QuestionShow { get; set; }
        [JsonProperty("reveal", Required = Required.Always)] public double Reveal { get; set; }
        [JsonProperty("scores", Required = Required.Always)] public double Scores { get; set; }
        [JsonProperty("roundEnd", Required = Required.Always)] public double RoundEnd { get; set; }
    }
}
