using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A player's character until the 3D characters exist: a rounded card in the character's signature color with
    /// the player's name in #14161C (docs/15_schemas.md, characters.json). Dimmed while disconnected, a white rim
    /// once locked in, a quick pop on lock in, an optional caption above it (Reveal results) and a power up marker above
    /// that (docs/03_rules.md, "Using").
    /// </summary>
    public sealed class CharacterChip : MonoBehaviour
    {
        private const float CaptionHeight = 60f;
        private const float CaptionGap = 6f;
        private const float MarkerHeight = 44f;
        private const float MarkerFontSize = 30f;

        private Image _card;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _caption;
        private CanvasGroup _group;
        private Outline _rim;
        private PowerUpMarker _marker;
        private float _popAge = -1f;

        public string PlayerId { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>A chip that fills its own rect; size it with anchors or a LayoutElement.</summary>
        public static CharacterChip Create(Transform parent, string playerId, float nameSize)
        {
            var rect = UiBuilder.Rect("Chip " + playerId, parent);
            var chip = rect.gameObject.AddComponent<CharacterChip>();
            chip.PlayerId = playerId;
            chip.Build(nameSize);
            return chip;
        }

        public void Set(string name, Color color, bool connected)
        {
            _name.text = name;
            _card.color = color;
            _group.alpha = connected ? 1f : TvMotion.DisconnectedAlpha;
        }

        public void SetLocked(bool locked)
        {
            _rim.enabled = locked;
        }

        /// <summary>Shows a caption above the chip; null or empty hides it.</summary>
        public void SetCaption(string text, Color color)
        {
            bool show = !string.IsNullOrEmpty(text);
            _caption.gameObject.SetActive(show);
            PlaceMarker();
            if (!show) return;
            _caption.text = text;
            _caption.color = color;
        }

        /// <summary>
        /// Shows a power up marker ("Dad → Freeze") above the chip, above the caption while one shows; null or empty
        /// hides it.
        /// </summary>
        public void SetHit(string text, Color color)
        {
            _marker.Set(text, color);
            PlaceMarker();
        }

        private void PlaceMarker()
        {
            if (_marker == null) return;
            float y = CaptionGap + (_caption.gameObject.activeSelf ? CaptionHeight + CaptionGap : 0f);
            _marker.Root.anchoredPosition = new Vector2(0, y);
        }

        /// <summary>A quick scale up and back on unscaled time; it never waits for anything.</summary>
        public void Pop()
        {
            _popAge = 0f;
        }

        private void Update()
        {
            if (_popAge < 0f) return;
            _popAge += Time.unscaledDeltaTime;
            float t = _popAge / TvMotion.PopSeconds;
            if (t >= 1f)
            {
                _card.rectTransform.localScale = Vector3.one;
                _popAge = -1f;
                return;
            }

            float scale = 1f + TvMotion.PopScale * Mathf.Sin(Mathf.PI * t);
            _card.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnDisable()
        {
            _popAge = -1f;
            if (_card != null) _card.rectTransform.localScale = Vector3.one;
        }

        private void Build(float nameSize)
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            _card = UiBuilder.Image("Card", transform, TvColors.Text, RoundedSprite.Get());
            UiBuilder.Anchor(_card.rectTransform, 0, 0, 1, 1);
            _rim = _card.gameObject.AddComponent<Outline>();
            _rim.effectColor = TvColors.Text;
            _rim.effectDistance = new Vector2(5f, -5f);
            _rim.enabled = false;

            _name = UiBuilder.Text("Name", _card.transform, nameSize, TvColors.Background, true, TextAlignmentOptions.Center);
            _name.textWrappingMode = TextWrappingModes.NoWrap;
            // Long names (up to 16 wide letters) shrink to fit the chip instead of being cut.
            _name.enableAutoSizing = true;
            _name.fontSizeMin = Mathf.Min(28f, nameSize);
            _name.fontSizeMax = nameSize;
            UiBuilder.Anchor(_name.rectTransform, 0, 0, 1, 1);
            _name.rectTransform.offsetMin = new Vector2(20, 0);
            _name.rectTransform.offsetMax = new Vector2(-20, 0);

            _caption = UiBuilder.Text("Caption", transform, 48, TvColors.Text, true, TextAlignmentOptions.Center);
            _caption.textWrappingMode = TextWrappingModes.NoWrap;
            _caption.overflowMode = TextOverflowModes.Overflow;
            _caption.enableAutoSizing = true;
            _caption.fontSizeMin = 30;
            _caption.fontSizeMax = 48;
            var caption = _caption.rectTransform;
            caption.anchorMin = new Vector2(0, 1);
            caption.anchorMax = new Vector2(1, 1);
            caption.pivot = new Vector2(0.5f, 0);
            caption.anchoredPosition = new Vector2(0, CaptionGap);
            caption.sizeDelta = new Vector2(0, CaptionHeight);
            _caption.gameObject.SetActive(false);

            _marker = PowerUpMarker.Create(transform, MarkerFontSize);
            var marker = _marker.Root;
            marker.anchorMin = new Vector2(0, 1);
            marker.anchorMax = new Vector2(1, 1);
            marker.pivot = new Vector2(0.5f, 0);
            marker.sizeDelta = new Vector2(0, MarkerHeight);
            PlaceMarker();
        }
    }
}
