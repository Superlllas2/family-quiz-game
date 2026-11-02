using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>A player facing string in both languages (docs/15_schemas.md).</summary>
    public sealed class LocalizedText
    {
        [JsonProperty("ru", Required = Required.Always)]
        public string Ru { get; set; }

        [JsonProperty("en", Required = Required.Always)]
        public string En { get; set; }

        public string Get(string lang)
        {
            return lang == Languages.English ? En : Ru;
        }
    }
}
