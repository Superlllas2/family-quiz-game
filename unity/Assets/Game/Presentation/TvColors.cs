using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>Placeholder colors from docs/06_phone_screens.md until the visual style guide exists.</summary>
    public static class TvColors
    {
        /// <summary>Dark background #14161C; also the text color on character chips.</summary>
        public static readonly Color Background = new Color32(0x14, 0x16, 0x1C, 0xFF);

        public static readonly Color Text = Color.white;

        public static readonly Color DimText = new Color32(0xB8, 0xBC, 0xC6, 0xFF);

        public static readonly Color Error = new Color32(0xE7, 0x4C, 0x3C, 0xFF);

        public static Color FromHex(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : fallback;
        }
    }
}
