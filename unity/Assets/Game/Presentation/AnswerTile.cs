using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One multiple choice option on the TV: a rounded tile in the answer color with its letter and text in #14161C.
    /// On Reveal the correct tile turns green and grows; the others fade. The ease runs on unscaled time.
    /// </summary>
    public sealed class AnswerTile : MonoBehaviour
    {
        /// <summary>Answer letters A to D (docs/06_phone_screens.md), the same in both languages.</summary>
        private const string Letters = "ABCD";

        private const float LetterWidth = 110f;

        private Image _card;
        private TextMeshProUGUI _letter;
        private TextMeshProUGUI _text;
        private CanvasGroup _group;
        private Outline _rim;
        private int _index;
        private float _targetScale = 1f;
        private float _targetAlpha = 1f;

        public static AnswerTile Create(Transform parent, int displayIndex)
        {
            var rect = UiBuilder.Rect("Option " + displayIndex, parent);
            var tile = rect.gameObject.AddComponent<AnswerTile>();
            tile._index = displayIndex;
            tile.Build();
            return tile;
        }

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>Back to the plain answer look, at once.</summary>
        public void ResetLook(string text)
        {
            _text.text = text;
            _card.color = TvColors.Answer(_index);
            _rim.enabled = false;
            _targetScale = 1f;
            _targetAlpha = 1f;
            transform.localScale = Vector3.one;
            _group.alpha = 1f;
        }

        public void Reveal(bool correct)
        {
            if (correct)
            {
                _card.color = TvColors.Correct;
                _rim.enabled = true;
                _targetScale = TvMotion.CorrectTileScale;
                _targetAlpha = 1f;
                transform.SetAsLastSibling(); // drawn over its neighbours while it grows
            }
            else
            {
                _targetScale = 1f;
                _targetAlpha = TvMotion.WrongTileAlpha;
            }
        }

        private void Update()
        {
            float k = 1f - Mathf.Exp(-TvMotion.TileEaseSpeed * Time.unscaledDeltaTime);
            float scale = Mathf.Lerp(transform.localScale.x, _targetScale, k);
            transform.localScale = new Vector3(scale, scale, 1f);
            _group.alpha = Mathf.Lerp(_group.alpha, _targetAlpha, k);
        }

        private void Build()
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            _card = UiBuilder.Image("Card", transform, TvColors.Answer(_index), RoundedSprite.Get());
            UiBuilder.Anchor(_card.rectTransform, 0, 0, 1, 1);
            _rim = _card.gameObject.AddComponent<Outline>();
            _rim.effectColor = TvColors.Text;
            _rim.effectDistance = new Vector2(6f, -6f);
            _rim.enabled = false;

            _letter = UiBuilder.Text("Letter", _card.transform, 80, TvColors.Background, true, TextAlignmentOptions.Center);
            _letter.textWrappingMode = TextWrappingModes.NoWrap;
            _letter.text = _index >= 0 && _index < Letters.Length ? Letters[_index].ToString() : string.Empty;
            var letter = _letter.rectTransform;
            letter.anchorMin = new Vector2(0, 0);
            letter.anchorMax = new Vector2(0, 1);
            letter.pivot = new Vector2(0, 0.5f);
            letter.anchoredPosition = new Vector2(16, 0);
            letter.sizeDelta = new Vector2(LetterWidth, 0);

            _text = UiBuilder.Text("Text", _card.transform, 56, TvColors.Background, true, TextAlignmentOptions.Left);
            // Two lines at most; long options shrink before they are cut.
            _text.enableAutoSizing = true;
            _text.fontSizeMin = 40;
            _text.fontSizeMax = 56;
            _text.maxVisibleLines = 2;
            UiBuilder.Anchor(_text.rectTransform, 0, 0, 1, 1);
            _text.rectTransform.offsetMin = new Vector2(16 + LetterWidth + 8, 12);
            _text.rectTransform.offsetMax = new Vector2(-28, -12);
        }
    }
}
