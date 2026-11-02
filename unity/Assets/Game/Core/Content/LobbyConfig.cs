using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "lobby".</summary>
    public sealed class LobbyConfig
    {
        [JsonProperty("startCountdownSeconds", Required = Required.Always)] public double StartCountdownSeconds { get; set; }
    }
}
