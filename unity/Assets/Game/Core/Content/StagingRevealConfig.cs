using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "staging.reveal": the guests' answer cards going up and their reactions.</summary>
    public sealed class StagingRevealConfig
    {
        [JsonProperty("cardsUpSeconds", Required = Required.Always)] public double CardsUpSeconds { get; set; }
        [JsonProperty("reactionSeconds", Required = Required.Always)] public double ReactionSeconds { get; set; }
    }
}
