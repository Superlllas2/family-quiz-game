using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "timers", in seconds (docs/03_rules.md, "Config keys").</summary>
    public sealed class TimersConfig
    {
        [JsonProperty("intro", Required = Required.Always)] public double Intro { get; set; }
        [JsonProperty("topicVote", Required = Required.Always)] public double TopicVote { get; set; }
        [JsonProperty("questionShow", Required = Required.Always)] public double QuestionShow { get; set; }
        [JsonProperty("answer", Required = Required.Always)] public AnswerTimersConfig Answer { get; set; }
        [JsonProperty("lastCall", Required = Required.Always)] public double LastCall { get; set; }

        /// <summary>
        /// Last call on or off (decisions/0020, D119): off, another player locking in never shortens anyone's time; the
        /// phase still ends the moment every connected player has answered.
        /// </summary>
        [JsonProperty("lastCallEnabled", Required = Required.Always)] public bool LastCallEnabled { get; set; }
        [JsonProperty("reveal", Required = Required.Always)] public double Reveal { get; set; }
        [JsonProperty("scores", Required = Required.Always)] public double Scores { get; set; }
        [JsonProperty("roundEnd", Required = Required.Always)] public double RoundEnd { get; set; }
        [JsonProperty("finalIntro", Required = Required.Always)] public double FinalIntro { get; set; }
        [JsonProperty("results", Required = Required.Always)] public double Results { get; set; }
    }
}
