using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>The power up ids (docs/03_rules.md, "Power ups"; PROTOCOL.md input kind powerUp).</summary>
    public static class PowerUpTypes
    {
        public const string Freeze = "freeze";
        public const string Slime = "slime";
        public const string Fog = "fog";

        /// <summary>Every power up in the order of the rules table.</summary>
        public static readonly IReadOnlyList<string> All = new[] { Freeze, Slime, Fog };

        public static bool IsValid(string id)
        {
            return id == Freeze || id == Slime || id == Fog;
        }
    }
}
