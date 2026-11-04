using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.questionShow": the curtain opening on the question board (D92).</summary>
    public sealed class StagingQuestionShowConfig
    {
        [JsonProperty("curtainOpenSeconds", Required = Required.Always)] public double CurtainOpenSeconds { get; set; }
        [JsonProperty("questionInSeconds", Required = Required.Always)] public double QuestionInSeconds { get; set; }
    }
}
