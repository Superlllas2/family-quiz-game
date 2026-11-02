using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging": only the extra seconds the ShowDirector adds to phase budgets (docs/21_staging.md).</summary>
    public sealed class StagingConfig
    {
        [JsonProperty("extraSeconds", Required = Required.Always)] public StagingExtraConfig ExtraSeconds { get; set; }
    }
}
