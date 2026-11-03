using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// The "effect" message (PROTOCOL.md section 4): a sabotage starts on this phone at the start of an Answer phase.
    /// params: freeze {taps}, slime {blobs}, fog {seconds}, from content/config.json powerUps.
    /// </summary>
    public sealed class EffectMessage
    {
        [JsonProperty("v")] public int V { get; set; } = ProtocolConstants.Version;
        [JsonProperty("type")] public string Type { get; set; } = "effect";
        [JsonProperty("effect")] public string Effect { get; set; }

        /// <summary>The attacker's name.</summary>
        [JsonProperty("from")] public string From { get; set; }

        [JsonProperty("params")] public object Params { get; set; }
    }
}
