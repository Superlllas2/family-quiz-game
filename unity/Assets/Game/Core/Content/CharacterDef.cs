using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>One entry of content/characters.json (the fields the game uses so far).</summary>
    public sealed class CharacterDef
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public LocalizedText Name { get; set; }

        [JsonProperty("personality", Required = Required.Always)]
        public LocalizedText Personality { get; set; }

        /// <summary>Signature color, #RRGGBB.</summary>
        [JsonProperty("color", Required = Required.Always)]
        public string Color { get; set; }
    }
}
