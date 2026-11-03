using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>Placeholder colors from docs/06_phone_screens.md until the visual style guide exists.</summary>
    public static class TvColors
    {
        /// <summary>Dark background #14161C; also the text color on character chips and answer tiles.</summary>
        public static readonly Color Background = new Color32(0x14, 0x16, 0x1C, 0xFF);

        public static readonly Color Text = Color.white;

        public static readonly Color DimText = new Color32(0xB8, 0xBC, 0xC6, 0xFF);

        public static readonly Color Error = new Color32(0xE7, 0x4C, 0x3C, 0xFF);

        /// <summary>Empty part of a timer bar.</summary>
        public static readonly Color BarTrack = new Color32(0x2C, 0x30, 0x3A, 0xFF);

        /// <summary>Row and panel cards on the dark background.</summary>
        public static readonly Color Card = new Color32(0x22, 0x25, 0x2E, 0xFF);

        /// <summary>The correct tile on Reveal and the "+points" captions. Brighter than answer D's green so it reads as "right".</summary>
        public static readonly Color Correct = new Color32(0x3D, 0xF5, 0x86, 0xFF);

        /// <summary>The crown on the closest chip of a closest value reveal, and the round winners' names.</summary>
        public static readonly Color Crown = new Color32(0xF5, 0xC5, 0x18, 0xFF);

        /// <summary>The finish cell of the race track.</summary>
        public static readonly Color Finish = new Color32(0x2E, 0x5E, 0x45, 0xFF);

        /// <summary>Answer tile colors A to D (docs/06_phone_screens.md).</summary>
        private static readonly Color[] AnswerColors =
        {
            new Color32(0xE7, 0x4C, 0x3C, 0xFF),
            new Color32(0x34, 0x98, 0xDB, 0xFF),
            new Color32(0xF1, 0xC4, 0x0F, 0xFF),
            new Color32(0x2E, 0xCC, 0x71, 0xFF)
        };

        public static Color Answer(int displayIndex)
        {
            return displayIndex >= 0 && displayIndex < AnswerColors.Length ? AnswerColors[displayIndex] : Text;
        }

        public static Color FromHex(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : fallback;
        }
    }
}
