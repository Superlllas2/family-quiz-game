using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "paused" (PROTOCOL.md section 4, decisions/0018 D105): the game is paused on the laptop.</summary>
    public sealed class PausedScreenData
    {
        [JsonProperty("text")] public string Text { get; set; }
    }
}
