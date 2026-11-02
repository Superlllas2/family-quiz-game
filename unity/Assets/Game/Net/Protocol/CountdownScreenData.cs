using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>screen.data for id "countdown" (PROTOCOL.md section 4); endsInMs carries the countdown.</summary>
    public sealed class CountdownScreenData
    {
        [JsonProperty("text")] public string Text { get; set; }
    }
}
