using Newtonsoft.Json;

namespace FamilyQuiz.Net
{
    /// <summary>JSON for protocol messages: field names come from the [JsonProperty] attributes on each DTO.</summary>
    public static class ProtocolJson
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Include
        };

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }
    }
}
