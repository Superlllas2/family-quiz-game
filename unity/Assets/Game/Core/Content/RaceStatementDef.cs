using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/race.json: a true or false statement for the final race (docs/15_schemas.md).</summary>
    public sealed class RaceStatementDef
    {
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("statement", Required = Required.Always)] public LocalizedText Statement { get; set; }
        [JsonProperty("answer", Required = Required.Always)] public bool Answer { get; set; }
        [JsonProperty("source", Required = Required.Always)] public string Source { get; set; }
    }
}
