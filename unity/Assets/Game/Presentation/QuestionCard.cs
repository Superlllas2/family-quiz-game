using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The question card (decisions/0017_game_night_ui.md): cream card stock, a little crooked, with the topic on a
    /// ribbon over its top left corner, the question number on a coin at its top right, the round's multiplier on a
    /// sticker on its left edge when it is more than 1, and the read time as a thin fuse along its bottom. After the
    /// reveal it flips over to its back: "Why?" in hand writing and the question's fact. Scores shows the back alone.
    /// </summary>
    public sealed class QuestionCard : MonoBehaviour
    {
        private const float Tilt = -1.2f;

        private TvPiece _card;
        private TvPiece _ribbon;
        private TextMeshProUGUI _topic;
        private TvPiece _coin;
        private TextMeshProUGUI _coinNumber;
        private TextMeshProUGUI _coinLabel;
        private TvPiece _sticker;
        private TextMeshProUGUI _stickerText;
        private TextMeshProUGUI _question;
        private TextMeshProUGUI _why;
        private TextMeshProUGUI _fact;
        private Image _fuseTrack;
        private Image _fuse;
        private TvTween _tween;
        private bool _large;

        public RectTransform Rect => (RectTransform)transform;

        public static QuestionCard Create(Transform parent)
        {
            var rect = UiBuilder.Rect("QuestionCard", parent);
            var card = rect.gameObject.AddComponent<QuestionCard>();
            card.Build();
            return card;
        }

        /// <summary>Large on the board in QuestionShow (up to 4 lines of bigger type), small under the guests after.</summary>
        public void SetLarge(bool large)
        {
            _large = large;
            if (large) UiBuilder.Fit(_question, 40, 64, 4);
            else UiBuilder.Fit(_question, 22, 42, 3);
            UiBuilder.Fit(_fact, 20, large ? 40 : 32, large ? 4 : 3);
        }

        /// <summary>The front: question, topic ribbon, coin and the sticker (null multiplier hides it).</summary>
        public void ShowFront(string question, string topic, string coinNumber, string coinLabel, string multiplier)
        {
            _tween.Finish();
            _question.text = question;
            _question.gameObject.SetActive(true);
            _why.gameObject.SetActive(false);
            _fact.gameObject.SetActive(false);
            _topic.text = topic ?? string.Empty;
            _ribbon.Root.gameObject.SetActive(!string.IsNullOrEmpty(topic));
            _coinNumber.text = coinNumber ?? string.Empty;
            _coinLabel.text = coinLabel ?? string.Empty;
            _coin.Root.gameObject.SetActive(!string.IsNullOrEmpty(coinNumber));
            _sticker.Root.gameObject.SetActive(!string.IsNullOrEmpty(multiplier));
            _stickerText.text = multiplier ?? string.Empty;
            SetFuse(-1f);
        }

        /// <summary>The back: "Why?" and the fact; the coin and sticker go, the topic ribbon stays.</summary>
        public void ShowBack(string why, string fact)
        {
            _question.gameObject.SetActive(false);
            _why.text = why;
            _fact.text = fact;
            _why.gameObject.SetActive(true);
            _fact.gameObject.SetActive(true);
            _coin.Root.gameObject.SetActive(false);
            _sticker.Root.gameObject.SetActive(false);
            SetFuse(-1f);
        }

        /// <summary>Flips over to the back after delay.</summary>
        public void FlipToBack(string why, string fact, float delay)
        {
            _tween.Flip(() => ShowBack(why, fact), delay, 0.45f);
        }

        /// <summary>Pops the card in (it slides onto the table with a bounce) and the coin and sticker after it.</summary>
        public void PopIn(float delay)
        {
            _tween.Pop(delay, 0.5f);
            if (_coin.Root.gameObject.activeSelf) TvTween.On(_coin.Root).Pop(delay + 0.3f, 0.35f);
            if (_sticker.Root.gameObject.activeSelf) TvTween.On(_sticker.Root).Slam(delay + 0.45f, 0.4f);
        }

        /// <summary>Keeps the right end of the card clear of the question (the coin's room by default, more for a stamp).</summary>
        public void ReserveRight(float units)
        {
            _question.rectTransform.offsetMax = new Vector2(-units, _question.rectTransform.offsetMax.y);
        }

        /// <summary>The read time fuse along the bottom, from 1 (full) to 0; negative hides it.</summary>
        public void SetFuse(float fraction)
        {
            bool show = fraction >= 0f;
            _fuseTrack.gameObject.SetActive(show);
            if (!show) return;
            _fuse.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        }

        private void Build()
        {
            _card = TvPiece.Card("Card", transform, TvColors.Cream, 22f, 9f);
            UiBuilder.Anchor(_card.Root, 0, 0, 1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, Tilt);
            _tween = TvTween.On(this);
            _tween.Tilt = Tilt;

            _question = UiBuilder.Display("Question", _card.Content, 42, TvColors.Ink, TextAlignmentOptions.Left);
            UiBuilder.Anchor(_question.rectTransform, 0, 0, 1, 1);
            _question.rectTransform.offsetMin = new Vector2(60f, 18f);
            // Under the tilted topic ribbon's lowest point, so the topic never covers the question (QuestionCardLayout).
            _question.rectTransform.offsetMax = new Vector2(-96f, -QuestionCardLayout.QuestionTopInset());
            _question.lineSpacing = -6f;

            _why = UiBuilder.Hand("Why", _card.Content, 48, TvColors.Teal, TextAlignmentOptions.TopLeft);
            _why.textWrappingMode = TextWrappingModes.NoWrap;
            // The hand writing is taller than its box: with the default ellipsis TMP would hide it entirely.
            _why.overflowMode = TextOverflowModes.Overflow;
            _why.rectTransform.anchorMin = new Vector2(0f, 1f);
            _why.rectTransform.anchorMax = new Vector2(1f, 1f);
            _why.rectTransform.pivot = new Vector2(0f, 1f);
            // On the ribbon's line, right of it.
            _why.rectTransform.anchoredPosition = new Vector2(430f, 0f);
            _why.rectTransform.sizeDelta = new Vector2(-470f, 54f);
            _fact = UiBuilder.Text("Fact", _card.Content, 32, TvColors.Ink, false, TextAlignmentOptions.TopLeft);
            UiBuilder.Anchor(_fact.rectTransform, 0, 0, 1, 1);
            _fact.rectTransform.offsetMin = new Vector2(60f, 14f);
            _fact.rectTransform.offsetMax = new Vector2(-40f, -58f);

            _fuseTrack = UiBuilder.Image("Fuse", _card.Content, TvColors.Cream2, TvSprites.Solid);
            _fuseTrack.pixelsPerUnitMultiplier = 5f;
            var track = _fuseTrack.rectTransform;
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 0f);
            track.pivot = new Vector2(0.5f, 0f);
            track.anchoredPosition = new Vector2(0f, 10f);
            track.sizeDelta = new Vector2(-80f, 10f);
            _fuse = UiBuilder.Image("Burn", _fuseTrack.transform, TvColors.Red, TvSprites.Solid);
            _fuse.pixelsPerUnitMultiplier = 5f;
            UiBuilder.Anchor(_fuse.rectTransform, 0, 0, 1, 1);

            _ribbon = TvPiece.Card("Ribbon", transform, TvColors.Teal, 10f, QuestionCardLayout.RibbonEdge)
                .Tilt(QuestionCardLayout.RibbonTilt);
            var ribbon = _ribbon.Root;
            ribbon.anchorMin = ribbon.anchorMax = new Vector2(0f, 1f);
            ribbon.pivot = new Vector2(0f, 0.5f);
            ribbon.anchoredPosition = new Vector2(QuestionCardLayout.RibbonX, QuestionCardLayout.RibbonY);
            ribbon.sizeDelta = new Vector2(QuestionCardLayout.RibbonWidth, QuestionCardLayout.RibbonHeight);
            _topic = UiBuilder.Text("Topic", _ribbon.Content, 24, TvColors.Cream, true, TextAlignmentOptions.Left);
            UiBuilder.Fit(_topic, 16, 24, 1);
            UiBuilder.Anchor(_topic.rectTransform, 0, 0, 1, 1);
            _topic.rectTransform.offsetMin = new Vector2(18f, 2f);
            _topic.rectTransform.offsetMax = new Vector2(-14f, -2f);

            _coin = TvPiece.Disc("Coin", transform, TvColors.Mustard, 5f);
            var coin = _coin.Root;
            coin.anchorMin = coin.anchorMax = new Vector2(1f, 1f);
            coin.pivot = new Vector2(0.5f, 0.5f);
            coin.anchoredPosition = new Vector2(-10f, -26f);
            coin.sizeDelta = new Vector2(96f, 96f);
            _coinNumber = UiBuilder.Display("Number", _coin.Content, 28, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_coinNumber, 16, 28, 1);
            UiBuilder.Anchor(_coinNumber.rectTransform, 0.12f, 0.4f, 0.88f, 0.8f);
            _coinLabel = UiBuilder.Text("Label", _coin.Content, 15, TvColors.Ink, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(_coinLabel, 10, 15, 1);
            UiBuilder.Anchor(_coinLabel.rectTransform, 0.14f, 0.2f, 0.86f, 0.42f);

            _sticker = TvPiece.Star("Multiplier", transform, TvColors.Red);
            var sticker = _sticker.Root;
            // Half off the card's left edge at mid height: clear of the tiles under the card.
            sticker.anchorMin = sticker.anchorMax = new Vector2(0f, 0.5f);
            sticker.pivot = new Vector2(0.5f, 0.5f);
            sticker.anchoredPosition = new Vector2(-30f, -6f);
            sticker.sizeDelta = new Vector2(104f, 104f);
            sticker.localRotation = Quaternion.Euler(0, 0, 10f);
            TvTween.On(sticker).Tilt = 10f;
            _stickerText = UiBuilder.Display("Text", _sticker.Content, 28, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stickerText, 16, 28, 1);
            UiBuilder.Anchor(_stickerText.rectTransform, 0.16f, 0.2f, 0.84f, 0.8f);

            SetLarge(false);
        }
    }
}
