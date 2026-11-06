using System.Collections.Generic;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The letters on the players' round tokens (decisions/0017_game_night_ui.md): the first letter of the name, two
    /// letters where two names share it, and a number after the letter as the last resort, so no two tokens on the TV
    /// read the same. Plain C#, tested without the engine.
    /// </summary>
    public static class TokenLabels
    {
        public static string[] For(IReadOnlyList<string> names)
        {
            int n = names.Count;
            var one = new string[n];
            var two = new string[n];
            for (int i = 0; i < n; i++)
            {
                var letters = Letters(names[i]);
                one[i] = letters.Length > 0 ? letters.Substring(0, 1).ToUpperInvariant() : "?";
                two[i] = letters.Length > 1 ? one[i] + letters.Substring(1, 1).ToLowerInvariant() : one[i];
            }

            var labels = new string[n];
            for (int i = 0; i < n; i++) labels[i] = Count(one, one[i]) > 1 ? two[i] : one[i];
            // Still the same (Ma and Ma): the letter and the player's number among those who share it.
            var before = (string[])labels.Clone();
            var seen = new Dictionary<string, int>();
            for (int i = 0; i < n; i++)
            {
                if (Count(before, before[i]) <= 1) continue;
                seen.TryGetValue(one[i], out int k);
                seen[one[i]] = ++k;
                labels[i] = one[i] + k;
            }

            return labels;
        }

        private static int Count(string[] values, string value)
        {
            int c = 0;
            foreach (var v in values) if (v == value) c++;
            return c;
        }

        /// <summary>The name's letters only (digits and marks dropped), so "Kid992" gives "Kid".</summary>
        private static string Letters(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            var chars = new List<char>();
            foreach (var c in name) if (char.IsLetter(c)) chars.Add(c);
            return new string(chars.ToArray());
        }
    }
}
