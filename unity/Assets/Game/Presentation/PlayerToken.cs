using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A player's round token (decisions/0017_game_night_ui.md): a disc in the character's colour with the player's
    /// letters (TokenLabels), a check badge that pops on when they locked in or voted, an optional sticker under it
    /// (points on Reveal) and half see through while disconnected. Fills its rect; the sticker hangs below it.
    /// </summary>
    public sealed class PlayerToken : MonoBehaviour
    {
        private TvPiece _disc;
        private TextMeshProUGUI _label;
        private TvPiece _check;
        private TvPiece _sticker;
        private TextMeshProUGUI _stickerText;
        private CanvasGroup _group;
        private EffectBadges _effects;
        private bool _locked;

        public string PlayerId { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>The sticker's height under the token, in canvas units: room to leave below a row of tokens.</summary>
        public const float StickerHeight = 40f;

        /// <summary>The size of the effect icons under the token in Answer (D149).</summary>
        public const float EffectSize = 46f;

        public static PlayerToken Create(Transform parent, string playerId)
        {
            var rect = UiBuilder.Rect("Token " + playerId, parent);
            var token = rect.gameObject.AddComponent<PlayerToken>();
            token.PlayerId = playerId;
            token.Build();
            return token;
        }

        public void Set(string label, Color color, bool connected)
        {
            _label.text = label;
            _label.color = TvColors.TextOn(color);
            _disc.SetFill(color);
            _group.alpha = connected ? 1f : TvMotion.DisconnectedAlpha;
        }

        /// <summary>The check badge; pops on when it turns on.</summary>
        public void SetLocked(bool locked, bool animate = false)
        {
            if (_locked == locked && _check.Root.gameObject.activeSelf == locked) return;
            _locked = locked;
            _check.Root.gameObject.SetActive(locked);
            if (locked && animate)
            {
                TvTween.On(_check.Root).Pop(0f, 0.35f);
                TvTween.On(_disc.Root).Pop(0f, 0.3f);
            }
        }

        /// <summary>A sticker under the token; null or empty hides it.</summary>
        public void SetSticker(string text, Color fill, bool animate = false, float delay = 0f)
        {
            bool show = !string.IsNullOrEmpty(text);
            _sticker.Root.gameObject.SetActive(show);
            if (!show) return;
            _stickerText.text = text;
            _stickerText.color = TvColors.TextOn(fill);
            _sticker.SetFill(fill);
            if (animate) TvTween.On(_sticker.Root).Pop(delay);
        }

        /// <summary>The power ups that hit this player (D149), as icons under the token; null or empty hides them.</summary>
        public void SetEffects(System.Collections.Generic.IReadOnlyList<string> powerUps)
        {
            if (_effects == null)
            {
                if (powerUps == null || powerUps.Count == 0) return;
                _effects = EffectBadges.Create(transform, EffectSize, 0.5f, 0f);
                _effects.Root.anchoredPosition = new Vector2(0f, -EffectSize / 2f + 4f);
            }

            _effects.Set(powerUps);
        }

        /// <summary>Where an attack arrow starts or lands under the token and its sticker, in the token's local space.</summary>
        public Vector2 ArrowPointBelow
        {
            get
            {
                var r = Rect.rect;
                return new Vector2(r.center.x, r.yMin - StickerHeight + 4f);
            }
        }

        public void Bounce()
        {
            TvTween.On(_disc.Root).Pop(0f, 0.3f);
        }

        private void Build()
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            _disc = TvPiece.Disc("Disc", transform, TvColors.Cream);
            UiBuilder.Anchor(_disc.Root, 0, 0, 1, 1);
            _label = UiBuilder.Display("Label", _disc.Content, 30, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_label, 16, 30, 1);
            UiBuilder.Anchor(_label.rectTransform, 0.12f, 0.12f, 0.88f, 0.88f);

            _check = TvPiece.Disc("Check", transform, TvColors.Cream, 3f);
            var check = _check.Root;
            check.anchorMin = check.anchorMax = new Vector2(1f, 1f);
            check.pivot = new Vector2(0.5f, 0.5f);
            check.anchoredPosition = new Vector2(-8f, -8f);
            check.sizeDelta = new Vector2(34f, 34f);
            var mark = UiBuilder.Image("Mark", _check.Content, TvColors.Green);
            mark.sprite = TvSprites.Check;
            UiBuilder.Anchor(mark.rectTransform, 0.16f, 0.16f, 0.84f, 0.84f);
            _check.Root.gameObject.SetActive(false);

            _sticker = TvPiece.Card("Sticker", transform, TvColors.Mustard, 10f, 4f);
            var sticker = _sticker.Root;
            sticker.anchorMin = new Vector2(0.5f, 0f);
            sticker.anchorMax = new Vector2(0.5f, 0f);
            sticker.pivot = new Vector2(0.5f, 1f);
            sticker.anchoredPosition = new Vector2(0f, -6f);
            sticker.sizeDelta = new Vector2(90f, StickerHeight - 8f);
            _sticker.Tilt(-4f);
            TvTween.On(sticker).Tilt = -4f;
            _stickerText = UiBuilder.Display("Text", _sticker.Content, 20, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stickerText, 12, 20, 1);
            UiBuilder.Anchor(_stickerText.rectTransform, 0.06f, 0.06f, 0.94f, 0.94f);
            _sticker.Root.gameObject.SetActive(false);
        }
    }
}
