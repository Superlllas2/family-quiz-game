using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>config.json "net".</summary>
    public sealed class NetConfig
    {
        [JsonProperty("inputGraceMs", Required = Required.Always)]
        public int InputGraceMs { get; set; }

        [JsonProperty("tvReconnectSeconds", Required = Required.Always)]
        public double TvReconnectSeconds { get; set; }
    }
}
