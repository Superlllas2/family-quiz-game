using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.intro": the establishing shot and the studio lights coming up inside it (D91, D94).</summary>
    public sealed class StagingIntroConfig
    {
        [JsonProperty("establishSeconds", Required = Required.Always)] public double EstablishSeconds { get; set; }
        [JsonProperty("lightsUpSeconds", Required = Required.Always)] public double LightsUpSeconds { get; set; }
    }
}
