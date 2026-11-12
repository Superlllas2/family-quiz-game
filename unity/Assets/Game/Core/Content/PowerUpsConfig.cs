using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "powerUps" (docs/03_rules.md, "Power ups"). enabled, types and targeting are the defaults of the TV settings panel.</summary>
    public sealed class PowerUpsConfig
    {
        [JsonProperty("enabled", Required = Required.Always)] public bool Enabled { get; set; }
        [JsonProperty("types", Required = Required.Always)] public string[] Types { get; set; }
        [JsonProperty("targeting", Required = Required.Always)] public string Targeting { get; set; }
        [JsonProperty("drawChanceLeader", Required = Required.Always)] public double DrawChanceLeader { get; set; }
        [JsonProperty("maxHeld", Required = Required.Always)] public int MaxHeld { get; set; }
        [JsonProperty("maxHitsPerQuestion", Required = Required.Always)] public int MaxHitsPerQuestion { get; set; }
        [JsonProperty("immunityQuestions", Required = Required.Always)] public int ImmunityQuestions { get; set; }
        /// <summary>Most seconds of sabotage per player per question, from the moment the options appear (D148).</summary>
        [JsonProperty("maxSabotageSeconds", Required = Required.Always)] public double MaxSabotageSeconds { get; set; }
        [JsonProperty("onPhone", Required = Required.Always)] public bool OnPhone { get; set; }
        [JsonProperty("freeze", Required = Required.Always)] public FreezeConfig Freeze { get; set; }
        [JsonProperty("slime", Required = Required.Always)] public SlimeConfig Slime { get; set; }
        [JsonProperty("fog", Required = Required.Always)] public FogConfig Fog { get; set; }
    }
}
