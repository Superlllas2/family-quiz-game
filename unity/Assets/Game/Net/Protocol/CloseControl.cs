using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>TV app to relay control message: {"close": "&lt;conn&gt;", "code": 4002} (PROTOCOL.md section 2).</summary>
    public sealed class CloseControl
    {
        [JsonProperty("close")] public string Close { get; set; }
        [JsonProperty("code")] public int Code { get; set; }
    }
}
