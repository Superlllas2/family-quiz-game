using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The geometry of the question card's topic ribbon and question text (decisions/0017_game_night_ui.md), in canvas
    /// units with the card's top left corner at (0, 0) and y up. The ribbon is tilted around its left end, so its right
    /// end dips into the card; the question text starts below the ribbon's lowest point, so a topic never covers a
    /// letter of the question (Nov 6, 2026 playtest, notes.md 2).
    /// </summary>
    public static class QuestionCardLayout
    {
        /// <summary>The ribbon's size; its left end sticks out of the card by -RibbonX.</summary>
        public const float RibbonWidth = 420f;
        public const float RibbonHeight = 44f;

        /// <summary>The ink edge drawn under the ribbon's body.</summary>
        public const float RibbonEdge = 5f;

        /// <summary>Degrees, positive counter clockwise, around the middle of the ribbon's left end.</summary>
        public const float RibbonTilt = -2f;

        /// <summary>The middle of the ribbon's left end: 16 units over the card's top, clear of the guests above.</summary>
        public const float RibbonX = -18f;
        public const float RibbonY = -6f;

        /// <summary>Room left between the ribbon's lowest point and the top of the question text.</summary>
        public const float Gap = 4f;

        /// <summary>The lowest y of the ribbon, its bottom edge included, once tilted (negative: inside the card).</summary>
        public static float RibbonLowestY()
        {
            float rad = RibbonTilt * Mathf.Deg2Rad;
            float sin = Mathf.Sin(rad), cos = Mathf.Cos(rad);
            float bottom = -RibbonHeight / 2f - RibbonEdge;
            float top = RibbonHeight / 2f;
            float lowest = float.MaxValue;
            foreach (var x in new[] { 0f, RibbonWidth })
            foreach (var y in new[] { bottom, top })
                lowest = Mathf.Min(lowest, RibbonY + x * sin + y * cos);
            return lowest;
        }

        /// <summary>How far under the card's top the question text starts (a positive inset).</summary>
        public static float QuestionTopInset()
        {
            return Mathf.Ceil(-RibbonLowestY() + Gap);
        }
    }
}
