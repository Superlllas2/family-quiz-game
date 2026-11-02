using System.Text;
using FamilyQuiz.Core;

namespace FamilyQuiz.Net
{
    /// <summary>The room code and the TV secret (PROTOCOL.md section 2). Immutable.</summary>
    public sealed class RoomCredentials
    {
        public RoomCredentials(string code, string secret)
        {
            Code = code;
            Secret = secret;
        }

        public string Code { get; }
        public string Secret { get; }

        public static RoomCredentials Create(IRandom random)
        {
            return new RoomCredentials(
                Pick(random, ProtocolConstants.RoomCodeAlphabet, ProtocolConstants.RoomCodeLength),
                Pick(random, ProtocolConstants.TvSecretAlphabet, ProtocolConstants.TvSecretLength));
        }

        private static string Pick(IRandom random, string alphabet, int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++) sb.Append(alphabet[random.Next(alphabet.Length)]);
            return sb.ToString();
        }
    }
}
