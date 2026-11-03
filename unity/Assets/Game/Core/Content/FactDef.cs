using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/facts.json: an interesting fact, the winner's reward on Results (D87, docs/15_schemas.md).</summary>
    public sealed class FactDef
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("text", Required = Required.Always)] public LocalizedText Text { get; set; }
        [JsonProperty("source", Required = Required.Always)] public string Source { get; set; }
    }
}
