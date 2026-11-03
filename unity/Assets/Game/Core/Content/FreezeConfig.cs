using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "powerUps.freeze": taps to break the ice.</summary>
    public sealed class FreezeConfig
    {
        [JsonProperty("taps", Required = Required.Always)] public int Taps { get; set; }
    }
}
