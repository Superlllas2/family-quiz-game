using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "race" (docs/02_show_flow.md, "The final race"; docs/03_rules.md, "Final race").</summary>
    public sealed class RaceConfig
    {
        /// <summary>Steps the leader starts on; last place starts on 0.</summary>
        [JsonProperty("maxHeadStart", Required = Required.Always)] public int MaxHeadStart { get; set; }

        /// <summary>The step that ends the race when a player reaches it.</summary>
        [JsonProperty("finishSteps", Required = Required.Always)] public int FinishSteps { get; set; }

        [JsonProperty("stepsPerCorrect", Required = Required.Always)] public int StepsPerCorrect { get; set; }

        /// <summary>Steps back for a wrong answer, never below 0 (D157).</summary>
        [JsonProperty("stepsPerWrong", Required = Required.Always)] public int StepsPerWrong { get; set; }

        /// <summary>The safety stop (D157): this many statements in a row with no answer and no pass from anyone end the race.</summary>
        [JsonProperty("idleStopStatements", Required = Required.Always)] public int IdleStopStatements { get; set; }

        /// <summary>Seconds per statement, times the player's timer multiplier.</summary>
        [JsonProperty("answer", Required = Required.Always)] public double Answer { get; set; }

        /// <summary>Seconds of the reveal after each statement.</summary>
        [JsonProperty("reveal", Required = Required.Always)] public double Reveal { get; set; }
    }
}
