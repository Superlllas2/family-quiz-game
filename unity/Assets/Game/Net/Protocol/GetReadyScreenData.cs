using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// screen.data for id "getReady" (PROTOCOL.md section 4): Intro, FinalIntro and QuestionShow. For QuestionShow the text
    /// is the question. In the Intro, prediction lists the players to tap ("Who wins tonight?"); left out elsewhere.
    /// </summary>
    public sealed class GetReadyScreenData
    {
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("prediction", NullValueHandling = NullValueHandling.Ignore)]
        public List<PredictionPlayerDto> Prediction { get; set; }
    }
}
