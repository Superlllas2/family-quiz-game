using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// screen.data for id "getReady" (PROTOCOL.md section 4). For QuestionShow the text is the question. The optional
    /// prediction list belongs to the Intro (Day 3) and is left out until then.
    /// </summary>
    public sealed class GetReadyScreenData
    {
        [JsonProperty("text")] public string Text { get; set; }
    }
}
