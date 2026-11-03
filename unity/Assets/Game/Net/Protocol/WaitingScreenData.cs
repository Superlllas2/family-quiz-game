using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "waiting" (PROTOCOL.md section 4): only what was locked (D88).</summary>
    public sealed class WaitingScreenData
    {
        /// <summary>Text of what was locked, for example "B · Danube".</summary>
        [JsonProperty("locked")] public string Locked { get; set; }
    }
}
