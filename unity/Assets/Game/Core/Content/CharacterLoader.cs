using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/characters.json (docs/15_schemas.md, tools/validate_content.mjs).</summary>
    public static class CharacterLoader
    {
        public const string FileName = "characters.json";

        private static readonly Regex ColorPattern = new Regex("^#[0-9A-Fa-f]{6}$");

        /// <summary>The English fields for artists and animators; each must be a non empty string.</summary>
        private static readonly string[] DescriptionFields =
            { "look", "entrance", "idlePose", "happyReaction", "sadReaction", "cardHold", "victory" };

        public static IReadOnlyList<CharacterDef> Parse(string json, GameConfig config)
        {
            var errors = new ContentErrors(FileName);
            var array = JsonItems.ParseArray(FileName, json);
            var items = JsonItems.Convert<CharacterDef>(array, errors);
            var list = new List<CharacterDef>();
            var names = new Dictionary<string, HashSet<string>>
            {
                { Languages.Russian, new HashSet<string>() },
                { Languages.English, new HashSet<string>() }
            };
            var colors = new HashSet<string>();
            foreach (var (c, obj, id) in items)
            {
                errors.Check(c.Id == array.IndexOf(obj), id, "id must be 0, 1, 2, ... in file order");
                errors.CheckText(id, "name", c.Name, ContentLimits.CharacterNameMax);
                if (c.Name?.Ru != null && c.Name.En != null)
                {
                    errors.Check(names[Languages.Russian].Add(c.Name.Ru.ToLowerInvariant()), id, "name.ru is used twice");
                    errors.Check(names[Languages.English].Add(c.Name.En.ToLowerInvariant()), id, "name.en is used twice");
                }

                errors.CheckText(id, "personality", c.Personality, ContentLimits.CharacterPersonalityMax);
                if (c.Color == null || !ColorPattern.IsMatch(c.Color))
                {
                    errors.Add(id, "color must be #RRGGBB");
                }
                else
                {
                    errors.Check(colors.Add(c.Color.ToUpperInvariant()), id, "color is used twice");
                    errors.Check(Contrast(c.Color, ContentLimits.ChipTextColor) >= ContentLimits.MinChipContrast, id,
                        "color needs a contrast of " + ContentLimits.MinChipContrast.ToString(CultureInfo.InvariantCulture)
                                                     + " with " + ContentLimits.ChipTextColor);
                }

                foreach (var field in DescriptionFields)
                {
                    var token = obj[field];
                    errors.Check(token != null && token.Type == JTokenType.String && token.ToString().Trim().Length > 0, id,
                        field + " is missing");
                }

                list.Add(c);
            }

            errors.Check(array.Count >= config.Players.Max, "-", "needs at least players.max characters");
            errors.ThrowIfAny();
            return list;
        }

        /// <summary>WCAG contrast ratio between two #RRGGBB colors.</summary>
        private static double Contrast(string a, string b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        private static double Luminance(string hex)
        {
            int n = int.Parse(hex.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return 0.2126 * Linear((n >> 16) & 255) + 0.7152 * Linear((n >> 8) & 255) + 0.0722 * Linear(n & 255);
        }

        private static double Linear(int channel)
        {
            double c = channel / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
    }
}
