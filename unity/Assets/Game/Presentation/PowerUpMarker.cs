using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The small marker next to a power up's target (docs/03_rules.md, "Using"): a rounded tag in the power up's color
    /// with the attacker's name and the power up ("Dad → Freeze") in #14161C. Hidden without a hit.
    /// </summary>
    public sealed class PowerUpMarker
    {
        private const float MinFontSize = 20f;
        private const float Padding = 12f;

        private readonly Image _box;
        private readonly TextMeshProUGUI _text;

        private PowerUpMarker(RectTransform root, Image box, TextMeshProUGUI text)
        {
            Root = root;
            _box = box;
            _text = text;
        }

        public RectTransform Root { get; }

        public bool Visible => Root.gameObject.activeSelf;

        /// <summary>A hidden marker; place and size it with Root.</summary>
        public static PowerUpMarker Create(Transform parent, float fontSize)
        {
            var box = UiBuilder.Image("PowerUpMarker", parent, TvColors.Text, RoundedSprite.Get());
            var text = UiBuilder.Text("Text", box.transform, fontSize, TvColors.Background, true, TextAlignmentOptions.Center);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(MinFontSize, fontSize);
            text.fontSizeMax = fontSize;
            UiBuilder.Anchor(text.rectTransform, 0, 0, 1, 1);
            text.rectTransform.offsetMin = new Vector2(Padding, 0);
            text.rectTransform.offsetMax = new Vector2(-Padding, 0);
            box.gameObject.SetActive(false);
            return new PowerUpMarker(box.rectTransform, box, text);
        }

        /// <summary>Shows the text on the power up's color; null or empty hides the marker.</summary>
        public void Set(string text, Color color)
        {
            bool show = !string.IsNullOrEmpty(text);
            Root.gameObject.SetActive(show);
            if (!show) return;
            _text.text = text;
            _box.color = color;
        }
    }
}
