using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "powerUps.fog": seconds until the fog clears on its own.</summary>
    public sealed class FogConfig
    {
        [JsonProperty("seconds", Required = Required.Always)] public double Seconds { get; set; }
    }
}
