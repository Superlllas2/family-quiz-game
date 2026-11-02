using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>TV app to relay: {"to": "&lt;conn&gt;" or "all", "msg": {...}} (PROTOCOL.md section 2).</summary>
    public sealed class OutboundEnvelope
    {
        [JsonProperty("to")] public string To { get; set; }
        [JsonProperty("msg")] public object Msg { get; set; }
    }
}
