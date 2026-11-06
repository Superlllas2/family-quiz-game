using Newtonsoft.Json;

namespace FamilyQuiz.Net.Protocol
{
    /// <summary>
    /// screen.data for id "watch" (PROTOCOL.md section 4): empty, the phone says "Eyes on the TV" (D88); in Reveal and
    /// Scores it may carry the rating card (D109).
    /// </summary>
    public sealed class WatchScreenData
    {
        [JsonProperty("rate", NullValueHandling = NullValueHandling.Ignore)] public RateDto Rate { get; set; }
    }
}
