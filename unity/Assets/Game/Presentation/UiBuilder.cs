using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>Small helpers to build UGUI objects from code.</summary>
    public static class UiBuilder
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Anchors the rect to a part of its parent given in 0..1 fractions, with no offsets.</summary>
        public static RectTransform Anchor(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, float size, Color color, bool bold,
            TextAlignmentOptions alignment)
        {
            var rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            // Nunito ExtraBold for text, Nunito Black for strong text: real weights, no faux bold.
            var font = bold ? TvFonts.Black : TvFonts.Body;
            if (font != null) text.font = font;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = bold && font == null ? FontStyles.Bold : FontStyles.Normal;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>A heading, number or tile letter in Unbounded ExtraBold.</summary>
        public static TextMeshProUGUI Display(string name, Transform parent, float size, Color color, TextAlignmentOptions alignment)
        {
            var text = Text(name, parent, size, color, false, alignment);
            if (TvFonts.Display != null) text.font = TvFonts.Display;
            return text;
        }

        /// <summary>A hand written word in Caveat Bold.</summary>
        public static TextMeshProUGUI Hand(string name, Transform parent, float size, Color color, TextAlignmentOptions alignment)
        {
            var text = Text(name, parent, size, color, false, alignment);
            if (TvFonts.Hand != null) text.font = TvFonts.Hand;
            return text;
        }

        /// <summary>Text that shrinks from max to min size to fit, on at most the given lines.</summary>
        public static TextMeshProUGUI Fit(TextMeshProUGUI text, float min, float max, int lines)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
            text.maxVisibleLines = lines;
            text.textWrappingMode = lines == 1 ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
            return text;
        }

        /// <summary>Places a rect by its centre in the parent's 0..1 space, with a fixed size in canvas units.</summary>
        public static RectTransform Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(x, y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        public static Image Image(string name, Transform parent, Color color, Sprite sprite = null)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = UnityEngine.UI.Image.Type.Sliced;
            }

            return image;
        }

        public static LayoutElement Size(Component target, float width, float height)
        {
            var layout = target.gameObject.GetComponent<LayoutElement>();
            if (layout == null) layout = target.gameObject.AddComponent<LayoutElement>();
            if (width >= 0) layout.preferredWidth = width;
            if (height >= 0) layout.preferredHeight = height;
            return layout;
        }
    }
}
