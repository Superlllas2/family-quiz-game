using Newtonsoft.Json;

namespace FamilyQuiz.Bootstrap
{
    /// <summary>StreamingAssets/local.json: machine settings, not in git (docs/15_schemas.md).</summary>
    public sealed class LocalSettings
    {
        public const string FileName = "local.json";

        [JsonProperty("relayHost")]
        public string RelayHost { get; set; }
    }
}
