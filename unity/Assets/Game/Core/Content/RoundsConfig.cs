using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "rounds".</summary>
    public sealed class RoundsConfig
    {
        [JsonProperty("count", Required = Required.Always)] public int Count { get; set; }
        [JsonProperty("questionsPerRound", Required = Required.Always)] public int QuestionsPerRound { get; set; }
        [JsonProperty("multipliers", Required = Required.Always)] public double[] Multipliers { get; set; }
        [JsonProperty("difficultyOrder", Required = Required.Always)] public string[] DifficultyOrder { get; set; }
        [JsonProperty("formatOrder", Required = Required.Always)] public string[] FormatOrder { get; set; }
    }
}
