using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// config.json "staging" (docs/21_staging.md): the extra seconds the ShowDirector adds to phase budgets, and the
    /// staging durations the TV's 3D set reads. The core never waits for any of the durations.
    /// </summary>
    public sealed class StagingConfig
    {
        [JsonProperty("extraSeconds", Required = Required.Always)] public StagingExtraConfig ExtraSeconds { get; set; }
        [JsonProperty("lobby", Required = Required.Always)] public StagingLobbyConfig Lobby { get; set; }
        [JsonProperty("intro", Required = Required.Always)] public StagingIntroConfig Intro { get; set; }
        [JsonProperty("questionShow", Required = Required.Always)] public StagingQuestionShowConfig QuestionShow { get; set; }
        [JsonProperty("reveal", Required = Required.Always)] public StagingRevealConfig Reveal { get; set; }
    }
}
