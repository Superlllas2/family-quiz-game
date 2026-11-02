using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/characters.json (docs/15_schemas.md).</summary>
    public static class CharacterLoader
    {
        public const string FileName = "characters.json";

        private static readonly Regex ColorPattern = new Regex("^#[0-9A-Fa-f]{6}$");

        public static IReadOnlyList<CharacterDef> Parse(string json, GameConfig config)
        {
            List<CharacterDef> list;
            try
            {
                list = JsonConvert.DeserializeObject<List<CharacterDef>>(json);
            }
            catch (JsonException e)
            {
                throw new ContentException(FileName, e.Message, e);
            }

            if (list == null) throw new ContentException(FileName, "file is empty");
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c.Id != i) throw new ContentException(FileName, c.Id + ": ids must be 0, 1, 2, ... in order");
                if (c.Color == null || !ColorPattern.IsMatch(c.Color))
                    throw new ContentException(FileName, c.Id + ": color must be #RRGGBB");
            }

            if (list.Count < config.Players.Max)
                throw new ContentException(FileName, "needs at least players.max characters");
            return list;
        }
    }
}
