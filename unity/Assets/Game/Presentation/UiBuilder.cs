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
            text.fontSize = size;
            text.color = color;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            return text;
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
