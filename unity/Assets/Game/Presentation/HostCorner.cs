using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One host's corner of the TV until the 3D hosts exist: a placeholder portrait box with the host's name in the
    /// bottom corner and a text bubble beside it, toward the middle of the screen. Hidden until shown.
    /// </summary>
    public sealed class HostCorner
    {
        private const float Margin = 24f;
        private const float PortraitSize = 200f;
        private const float BubbleGap = 16f;
        private const float BubbleWidth = 680f;
        private const float BubblePadding = 22f;
        private const float NamePadding = 12f;
        private const float NameSize = 36f;
        private const float NameMinSize = 24f;
        private const float LineSize = 42f;
        private const float LineMinSize = 30f;

        private readonly GameObject _root;
        private readonly TextMeshProUGUI _name;
        private readonly TextMeshProUGUI _line;

        private HostCorner(GameObject root, TextMeshProUGUI name, TextMeshProUGUI line)
        {
            _root = root;
            _name = name;
            _line = line;
        }

        public bool Visible => _root.activeSelf;

        /// <summary>The language the texts were last drawn in; null to redraw.</summary>
        public string ShownLang { get; set; }

        /// <summary>A corner on the bottom left, or with right on the bottom right.</summary>
        public static HostCorner Create(Transform parent, string host, bool right)
        {
            float anchorX = right ? 1f : 0f;
            float sign = right ? -1f : 1f;
            var root = UiBuilder.Anchor(UiBuilder.Rect("Host " + host, parent), 0, 0, 1, 1);

            var portrait = UiBuilder.Image("Portrait", root, TvColors.HostCard, RoundedSprite.Get());
            Place(portrait.rectTransform, anchorX, sign * Margin, PortraitSize);
            var name = UiBuilder.Text("Name", portrait.transform, NameSize, TvColors.Text, true, TextAlignmentOptions.Center);
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.enableAutoSizing = true;
            name.fontSizeMin = NameMinSize;
            name.fontSizeMax = NameSize;
            UiBuilder.Anchor(name.rectTransform, 0, 0, 1, 1);
            name.rectTransform.offsetMin = new Vector2(NamePadding, 0);
            name.rectTransform.offsetMax = new Vector2(-NamePadding, 0);

            var bubble = UiBuilder.Image("Bubble", root, TvColors.Bubble, RoundedSprite.Get());
            Place(bubble.rectTransform, anchorX, sign * (Margin + PortraitSize + BubbleGap), BubbleWidth);
            var line = UiBuilder.Text("Line", bubble.transform, LineSize, TvColors.Background, true,
                right ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
            line.enableAutoSizing = true;
            line.fontSizeMin = LineMinSize;
            line.fontSizeMax = LineSize;
            UiBuilder.Anchor(line.rectTransform, 0, 0, 1, 1);
            line.rectTransform.offsetMin = new Vector2(BubblePadding, BubblePadding * 0.6f);
            line.rectTransform.offsetMax = new Vector2(-BubblePadding, -BubblePadding * 0.6f);

            root.gameObject.SetActive(false);
            return new HostCorner(root.gameObject, name, line);
        }

        public void SetVisible(bool visible)
        {
            if (_root.activeSelf != visible) _root.SetActive(visible);
        }

        public void SetTexts(string hostName, string line)
        {
            _name.text = hostName ?? string.Empty;
            _line.text = line ?? string.Empty;
        }

        /// <summary>A box of the given width and the portrait's height on the bottom edge, x from the left or right edge.</summary>
        private static void Place(RectTransform rect, float anchorX, float x, float width)
        {
            rect.anchorMin = new Vector2(anchorX, 0);
            rect.anchorMax = new Vector2(anchorX, 0);
            rect.pivot = new Vector2(anchorX, 0);
            rect.anchoredPosition = new Vector2(x, Margin);
            rect.sizeDelta = new Vector2(width, PortraitSize);
        }
    }
}
