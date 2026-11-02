namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Player names (PROTOCOL.md section 3, "Names"): trimmed, then 1 to 16 characters, each a Latin letter
    /// (A to Z, a to z), a Cyrillic letter (А to Я, а to я, Ё, ё), a digit or a space.
    /// </summary>
    public static class NameRules
    {
        /// <summary>Returns the trimmed name if it is allowed, else null.</summary>
        public static string Normalize(string raw)
        {
            if (raw == null) return null;
            var name = raw.Trim();
            if (name.Length < 1 || name.Length > ProtocolLimits.MaxNameLength) return null;
            foreach (var c in name)
            {
                if (!IsAllowed(c)) return null;
            }

            return name;
        }

        /// <summary>Names compare ignoring case, Cyrillic included ("nameTaken").</summary>
        public static bool SameName(string a, string b)
        {
            return string.Equals(a.ToLowerInvariant(), b.ToLowerInvariant(), System.StringComparison.Ordinal);
        }

        private static bool IsAllowed(char c)
        {
            if (c >= 'A' && c <= 'Z') return true;
            if (c >= 'a' && c <= 'z') return true;
            if (c >= '0' && c <= '9') return true;
            if (c == ' ') return true;
            if (c >= 'А' && c <= 'я') return true; // А to я
            return c == 'Ё' || c == 'ё'; // Ё, ё
        }
    }
}
