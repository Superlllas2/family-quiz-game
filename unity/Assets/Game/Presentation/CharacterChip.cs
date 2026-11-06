using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A player's name piece (decisions/0017_game_night_ui.md): a rounded piece in the character's signature colour with
    /// the player's name, on its cardboard edge. Half see through while disconnected, a check badge once locked in, a
    /// quick pop on lock in, an optional sticker caption above it and a power up marker above that (docs/03_rules.md,
    /// "Using"). Used by the race, the round end and the results lists.
    /// </summary>
    public sealed class CharacterChip : MonoBehaviour
    {
        private const float CaptionHeight = 48f;
        private const float CaptionGap = 8f;
        private const float MarkerHeight = 44f;
        private const float MarkerFontSize = 26f;

        private TvPiece _card;
        private TextMeshProUGUI _name;
        private TvPiece _caption;
        private TextMeshProUGUI _captionText;
        private CanvasGroup _group;
        private TvPiece _check;
        private PowerUpMarker _marker;

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
            _name.color = TvColors.TextOn(color);
            _card.SetFill(color);
            _group.alpha = connected ? 1f : TvMotion.DisconnectedAlpha;
        }

        public void SetLocked(bool locked)
        {
            _check.Root.gameObject.SetActive(locked);
        }

        /// <summary>Shows a sticker caption above the chip; null or empty hides it.</summary>
        public void SetCaption(string text, Color color)
        {
            bool show = !string.IsNullOrEmpty(text);
            _caption.Root.gameObject.SetActive(show);
            PlaceMarker();
            if (!show) return;
            _captionText.text = text;
            var fill = color == TvColors.Correct ? TvColors.Mustard : TvColors.Cream;
            _caption.SetFill(fill);
            _captionText.color = TvColors.Ink;
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
            float y = CaptionGap + (_caption.Root.gameObject.activeSelf ? CaptionHeight + CaptionGap : 0f);
            _marker.Root.anchoredPosition = new Vector2(0, y);
        }

        /// <summary>A quick bounce on unscaled time; it never waits for anything.</summary>
        public void Pop()
        {
            TvTween.On(_card.Root).Pop(0f, 0.3f);
        }

        private void Build(float nameSize)
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            _card = TvPiece.Card("Card", transform, TvColors.Cream, 16f, 5f);
            UiBuilder.Anchor(_card.Root, 0, 0, 1, 1);

            _name = UiBuilder.Text("Name", _card.Content, nameSize, TvColors.Ink, true, TextAlignmentOptions.Center);
            _name.textWrappingMode = TextWrappingModes.NoWrap;
            // Long names (up to 16 wide letters) shrink to fit the chip instead of being cut.
            _name.enableAutoSizing = true;
            _name.fontSizeMin = Mathf.Min(20f, nameSize);
            _name.fontSizeMax = nameSize;
            UiBuilder.Anchor(_name.rectTransform, 0, 0, 1, 1);
            _name.rectTransform.offsetMin = new Vector2(16, 0);
            _name.rectTransform.offsetMax = new Vector2(-16, 0);

            _check = TvPiece.Disc("Check", transform, TvColors.Cream, 3f);
            var check = _check.Root;
            check.anchorMin = check.anchorMax = new Vector2(1f, 1f);
            check.pivot = new Vector2(0.5f, 0.5f);
            check.anchoredPosition = new Vector2(-6f, -6f);
            check.sizeDelta = new Vector2(34f, 34f);
            var mark = UiBuilder.Image("Mark", _check.Content, TvColors.Green);
            mark.sprite = TvSprites.Check;
            UiBuilder.Anchor(mark.rectTransform, 0.16f, 0.16f, 0.84f, 0.84f);
            _check.Root.gameObject.SetActive(false);

            _caption = TvPiece.Card("Caption", transform, TvColors.Mustard, 10f, 4f).Tilt(-3f);
            var caption = _caption.Root;
            caption.anchorMin = new Vector2(0.1f, 1f);
            caption.anchorMax = new Vector2(0.9f, 1f);
            caption.pivot = new Vector2(0.5f, 0f);
            caption.anchoredPosition = new Vector2(0, CaptionGap);
            caption.sizeDelta = new Vector2(0, CaptionHeight);
            _captionText = UiBuilder.Display("Text", _caption.Content, 26, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_captionText, 14, 26, 1);
            UiBuilder.Anchor(_captionText.rectTransform, 0.04f, 0.06f, 0.96f, 0.94f);
            _caption.Root.gameObject.SetActive(false);

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
