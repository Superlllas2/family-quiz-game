using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "timers.answer": the answer window per format, in seconds, before timer multipliers.</summary>
    public sealed class AnswerTimersConfig
    {
        [JsonProperty("multipleChoice", Required = Required.Always)] public double MultipleChoice { get; set; }
        [JsonProperty("closestValue", Required = Required.Always)] public double ClosestValue { get; set; }
    }
}
