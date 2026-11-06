using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "banks": the question bank test (decisions/0020, D116 to D118).</summary>
    public sealed class BanksConfig
    {
        /// <summary>The bank the TV settings start with: full or curated (<see cref="BankIds"/>).</summary>
        [JsonProperty("default", Required = Required.Always)] public string Default { get; set; }

        /// <summary>The TV asks which bank to play when the app starts. A test tool: off in the finished game.</summary>
        [JsonProperty("chooseOnStart", Required = Required.Always)] public bool ChooseOnStart { get; set; }
    }
}
