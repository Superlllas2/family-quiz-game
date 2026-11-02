using System.Text;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>Seat tokens: random 24 characters (PROTOCOL.md section 4), drawn from the injected random.</summary>
    public static class TokenGenerator
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string Create(IRandom random)
        {
            var sb = new StringBuilder(ProtocolLimits.TokenLength);
            for (int i = 0; i < ProtocolLimits.TokenLength; i++) sb.Append(Alphabet[random.Next(Alphabet.Length)]);
            return sb.ToString();
        }
    }
}
