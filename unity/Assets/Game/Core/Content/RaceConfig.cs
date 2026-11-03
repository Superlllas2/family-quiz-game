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

        /// <summary>The race ends after this many statements at the latest.</summary>
        [JsonProperty("maxQuestions", Required = Required.Always)] public int MaxQuestions { get; set; }

        [JsonProperty("stepsPerCorrect", Required = Required.Always)] public int StepsPerCorrect { get; set; }

        /// <summary>Seconds per statement, times the player's timer multiplier.</summary>
        [JsonProperty("answer", Required = Required.Always)] public double Answer { get; set; }

        /// <summary>Seconds of the reveal after each statement.</summary>
        [JsonProperty("reveal", Required = Required.Always)] public double Reveal { get; set; }
    }
}
