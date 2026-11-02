using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "pollDone" (PROTOCOL.md section 4).</summary>
    public sealed class PollDoneScreenData
    {
        [JsonProperty("canPlayAgain")] public bool CanPlayAgain { get; set; }
    }
}
