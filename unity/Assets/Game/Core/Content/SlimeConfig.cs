using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "powerUps.slime": blobs to wipe away.</summary>
    public sealed class SlimeConfig
    {
        [JsonProperty("blobs", Required = Required.Always)] public int Blobs { get; set; }
    }
}
