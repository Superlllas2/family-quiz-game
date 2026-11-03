using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>The two hosts of host_lines.json (docs/15_schemas.md).</summary>
    public static class HostIds
    {
        public const string Konstantin = "konstantin";
        public const string Ilja = "ilja";

        public static readonly IReadOnlyList<string> All = new[] { Konstantin, Ilja };

        public static bool IsValid(string id)
        {
            return id == Konstantin || id == Ilja;
        }

        public static string Other(string id)
        {
            return id == Konstantin ? Ilja : Konstantin;
        }
    }
}
