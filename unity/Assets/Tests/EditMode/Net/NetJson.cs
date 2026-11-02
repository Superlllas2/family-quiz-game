using FamilyQuiz.Net;
using FamilyQuiz.Net.Protocol;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>JSON helpers for exact shape assertions.</summary>
    public static class NetJson
    {
        /// <summary>The screen message exactly as it goes on the wire.</summary>
        public static JObject ScreenMessage(ScreenDto screen)
        {
            return JObject.Parse(ProtocolJson.Serialize(new ScreenMessage(screen)));
        }

        /// <summary>Asserts actual equals the expected JSON exactly: same fields, no extra ones, same values.</summary>
        public static void AssertJson(string expectedJson, JToken actual)
        {
            var expected = JToken.Parse(expectedJson);
            Assert.That(JToken.DeepEquals(expected, actual), Is.True,
                "expected\n" + expected.ToString(Newtonsoft.Json.Formatting.None) + "\nbut was\n" + actual?.ToString(Newtonsoft.Json.Formatting.None));
        }
    }
}
