using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The game night palette (decisions/0017_game_night_ui.md): a family board game on a walnut table. Every piece is
    /// cream card stock or printed ink with a thick ink outline and a solid ink bottom edge. The answer inks A to D are
    /// the same on the phones (docs/06_phone_screens.md).
    /// </summary>
    public static class TvColors
    {
        /// <summary>The die cut outlines, the solid bottom edges and the type on cream.</summary>
        public static readonly Color Ink = new Color32(0x2B, 0x1D, 0x16, 0xFF);

        /// <summary>Card stock.</summary>
        public static readonly Color Cream = new Color32(0xF6, 0xEB, 0xD5, 0xFF);

        /// <summary>Card stock in shade; the light wood of the ruler.</summary>
        public static readonly Color Cream2 = new Color32(0xEA, 0xDB, 0xBE, 0xFF);

        /// <summary>The walnut games table: the background with the 3D studio off.</summary>
        public static readonly Color Table = new Color32(0x3A, 0x24, 0x18, 0xFF);

        public static readonly Color Red = new Color32(0xE4, 0x57, 0x2E, 0xFF);
        public static readonly Color Cobalt = new Color32(0x3A, 0x6E, 0xA5, 0xFF);
        public static readonly Color Mustard = new Color32(0xF2, 0xB3, 0x3D, 0xFF);
        public static readonly Color Green = new Color32(0x4F, 0x9D, 0x45, 0xFF);
        public static readonly Color Teal = new Color32(0x2A, 0x9D, 0x8F, 0xFF);
        public static readonly Color Plum = new Color32(0x7B, 0x4B, 0x94, 0xFF);
        public static readonly Color Wood = new Color32(0xC9, 0x8A, 0x4B, 0xFF);

        /// <summary>The flat background behind the views when the 3D studio is off.</summary>
        public static readonly Color Background = Table;

        /// <summary>Text straight on the studio or the table.</summary>
        public static readonly Color Text = Cream;

        public static readonly Color DimText = new Color32(0xCF, 0xBD, 0x9E, 0xFF);

        public static readonly Color Error = Red;

        /// <summary>Empty part of a timer bar.</summary>
        public static readonly Color BarTrack = Ink;

        /// <summary>Row and panel cards.</summary>
        public static readonly Color Card = Cream;

        /// <summary>Points and "right" marks on text over the studio: a green that reads on the dark set.</summary>
        public static readonly Color Correct = new Color32(0x86, 0xD8, 0x6E, 0xFF);

        /// <summary>The crown on the closest guess and the round winners.</summary>
        public static readonly Color Crown = Mustard;

        /// <summary>The finish cell of the race track.</summary>
        public static readonly Color Finish = Red;

        /// <summary>The freeze power up's marker: icy blue.</summary>
        public static readonly Color Freeze = new Color32(0x8F, 0xD8, 0xF8, 0xFF);

        /// <summary>The slime power up's marker: green.</summary>
        public static readonly Color Slime = new Color32(0x9B, 0xD8, 0x57, 0xFF);

        /// <summary>The fog power up's marker: grey.</summary>
        public static readonly Color Fog = new Color32(0xB9, 0xB4, 0xAA, 0xFF);

        /// <summary>The hosts' cube badges: Konstantin red, Ilja navy.</summary>
        public static readonly Color HostKonstantin = new Color32(0xB8, 0x3A, 0x2E, 0xFF);

        public static readonly Color HostIlja = new Color32(0x26, 0x3A, 0x6B, 0xFF);

        public static readonly Color HostCard = Ink;

        public static readonly Color Bubble = Cream;

        /// <summary>
        /// The TV's menus (settings panel, pause menu, bank chooser) are board game pieces too (D100): a card of card
        /// stock, choices of shaded card stock, the selected one in teal ink.
        /// </summary>
        public static readonly Color Panel = Cream;

        public static readonly Color ButtonOff = Cream2;

        public static readonly Color ButtonOn = Teal;

        /// <summary>Small print on card stock: hints, counts, credits.</summary>
        public static readonly Color InkSoft = new Color32(0x6E, 0x58, 0x46, 0xFF);

        /// <summary>A power up's marker color; Cream for an unknown one.</summary>
        public static Color PowerUp(string powerUp)
        {
            switch (powerUp)
            {
                case FamilyQuiz.Core.Content.PowerUpTypes.Freeze: return Freeze;
                case FamilyQuiz.Core.Content.PowerUpTypes.Slime: return Slime;
                case FamilyQuiz.Core.Content.PowerUpTypes.Fog: return Fog;
                default: return Cream;
            }
        }

        /// <summary>Answer inks A to D (docs/06_phone_screens.md).</summary>
        private static readonly Color[] AnswerColors = { Red, Cobalt, Mustard, Green };

        public static Color Answer(int displayIndex)
        {
            return displayIndex >= 0 && displayIndex < AnswerColors.Length ? AnswerColors[displayIndex] : Cream;
        }

        /// <summary>The text on an answer tile: cream on red, cobalt and green, ink on mustard.</summary>
        public static Color AnswerText(int displayIndex)
        {
            return displayIndex == 2 ? Ink : Cream;
        }

        /// <summary>Topic card headers in the vote, in door order; the last (mixed) door is plum.</summary>
        private static readonly Color[] DoorColors = { Teal, Mustard, Red, Cobalt, Green, Plum };

        public static Color Door(int index, bool mixed)
        {
            if (mixed) return Plum;
            return DoorColors[((index % DoorColors.Length) + DoorColors.Length) % DoorColors.Length];
        }

        /// <summary>Ink for text on a coloured piece: ink on light colours, cream on dark ones.</summary>
        public static Color TextOn(Color fill)
        {
            float luma = 0.2126f * fill.r + 0.7152f * fill.g + 0.0722f * fill.b;
            return luma > 0.5f ? Ink : Cream;
        }

        /// <summary>The colour of a piece that dropped out: greyed toward the table and darker.</summary>
        public static Color Faded(Color c)
        {
            float grey = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
            var g = new Color(grey, grey, grey, c.a);
            var mixed = Color.Lerp(c, g, 0.7f) * 0.72f;
            mixed.a = c.a;
            return mixed;
        }

        public static Color FromHex(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : fallback;
        }
    }
}
