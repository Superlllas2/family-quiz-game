using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One multiple choice option on the TV (decisions/0017_game_night_ui.md): a chunky tile in the answer ink, a little
    /// crooked, with its letter on a cream disc and the option text. It bounces in with the Answer. On Reveal the right
    /// tile grows a touch and gets the stamp (QuestionView), the others sink onto the table and fade, and the tokens of
    /// the players who picked a tile sit on its top edge.
    /// </summary>
    public sealed class AnswerTile : MonoBehaviour
    {
        /// <summary>Answer letters A to D (docs/06_phone_screens.md), the same in both languages.</summary>
        private const string Letters = "ABCD";

        private const float Edge = 8f;
        private const float SunkEdge = 2f;
        private const float PickSize = 46f;
        private const float PickGap = 6f;

        /// <summary>The right end of the right tile kept for the stamp (QuestionView puts it there).</summary>
        public const float StampRoom = 270f;
        private static readonly float[] Tilts = { -0.6f, 0.5f, 0.4f, -0.7f };

        private readonly List<PlayerToken> _picks = new List<PlayerToken>();
        private TvPiece _piece;
        private TextMeshProUGUI _letter;
        private TextMeshProUGUI _text;
        private TvTween _tween;
        private int _index;
        private float _targetScale = 1f;
        private float _targetDrop;

        public static AnswerTile Create(Transform parent, int displayIndex)
        {
            var rect = UiBuilder.Rect("Option " + displayIndex, parent);
            var tile = rect.gameObject.AddComponent<AnswerTile>();
            tile._index = displayIndex;
            tile.Build();
            return tile;
        }

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>The tile body's rect: what the stamp lands on.</summary>
        public RectTransform Body => _piece.Root;

        /// <summary>Back to the plain answer look; animate bounces it in after delay.</summary>
        public void ResetLook(string text, bool animate, float delay)
        {
            _text.text = text;
            _text.color = TvColors.AnswerText(_index);
            _piece.SetFill(TvColors.Answer(_index));
            _piece.SetEdge(Edge);
            _targetScale = 1f;
            _targetDrop = 0f;
            _piece.Root.localScale = Vector3.one;
            _piece.Root.anchoredPosition = Vector2.zero;
            _text.rectTransform.offsetMax = new Vector2(-20f, -8f);
            ClearPicks();
            if (animate) _tween.Pop(delay, 0.5f);
            else _tween.Finish();
        }

        public void Reveal(bool correct)
        {
            if (correct)
            {
                _targetScale = 1.04f;
                _targetDrop = 0f;
                // The stamp lands on the right end: the text makes room for it (and shrinks if it must).
                _text.rectTransform.offsetMax = new Vector2(-StampRoom, -8f);
                transform.SetAsLastSibling(); // drawn over its neighbours while it grows
            }
            else
            {
                _targetScale = 1f;
                _targetDrop = Edge - SunkEdge;
                _piece.SetFill(TvColors.Faded(TvColors.Answer(_index)));
                _text.color = TvColors.Faded(TvColors.AnswerText(_index));
                _piece.SetEdge(SunkEdge);
            }
        }

        /// <summary>
        /// The tokens of the players who picked this tile, from its right corner, on its top edge (onTop) or its bottom
        /// edge, sticking out 14 units: the gaps around the tiles leave room for that and no more.
        /// </summary>
        public void SetPicks(IReadOnlyList<(string Id, string Label, Color Color)> picks, bool animate, bool onTop, float rightInset)
        {
            ClearPicks();
            // Many picks on one tile overlap like a pile of coins rather than run off its left end.
            float width = Rect.rect.width > 0f ? Rect.rect.width : 640f;
            float step = picks.Count > 0 ? Mathf.Min(PickSize + PickGap, (width - rightInset - 28f - PickSize) / Mathf.Max(1, picks.Count - 1)) : 0f;
            step = Mathf.Max(PickSize * 0.45f, step);
            for (int i = 0; i < picks.Count; i++)
            {
                var token = PlayerToken.Create(transform, picks[i].Id);
                var rect = token.Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(1f, onTop ? 1f : 0f);
                rect.pivot = new Vector2(1f, onTop ? 1f - 14f / PickSize : 14f / PickSize);
                rect.anchoredPosition = new Vector2(-14f - rightInset - i * step, 0f);
                rect.sizeDelta = new Vector2(PickSize, PickSize);
                token.Set(picks[i].Label, picks[i].Color, true);
                if (animate) TvTween.On(rect).Pop(0.25f + 0.06f * i, 0.35f);
                _picks.Add(token);
            }
        }

        private void ClearPicks()
        {
            foreach (var t in _picks) Destroy(t.gameObject);
            _picks.Clear();
        }

        private void Update()
        {
            float k = 1f - Mathf.Exp(-TvMotion.TileEaseSpeed * Time.unscaledDeltaTime);
            var root = _piece.Root;
            float scale = Mathf.Lerp(root.localScale.x, _targetScale, k);
            root.localScale = new Vector3(scale, scale, 1f);
            var pos = root.anchoredPosition;
            root.anchoredPosition = new Vector2(pos.x, Mathf.Lerp(pos.y, -_targetDrop, k));
        }

        private void Build()
        {
            _piece = TvPiece.Card("Tile", transform, TvColors.Answer(_index), 20f, Edge);
            UiBuilder.Anchor(_piece.Root, 0, 0, 1, 1);
            float tilt = Tilts[Mathf.Clamp(_index, 0, Tilts.Length - 1)];
            transform.localRotation = Quaternion.Euler(0, 0, tilt);
            _tween = TvTween.On(this);
            _tween.Tilt = tilt;

            var disc = TvPiece.Disc("LetterDisc", _piece.Content, TvColors.Cream, 0f);
            var discRect = disc.Root;
            discRect.anchorMin = new Vector2(0f, 0.5f);
            discRect.anchorMax = new Vector2(0f, 0.5f);
            discRect.pivot = new Vector2(0f, 0.5f);
            discRect.anchoredPosition = new Vector2(16f, 0f);
            discRect.sizeDelta = new Vector2(58f, 58f);
            _letter = UiBuilder.Display("Letter", disc.Content, 30, TvColors.Ink, TextAlignmentOptions.Center);
            _letter.textWrappingMode = TextWrappingModes.NoWrap;
            _letter.text = _index >= 0 && _index < Letters.Length ? Letters[_index].ToString() : string.Empty;
            UiBuilder.Anchor(_letter.rectTransform, 0, 0, 1, 1);

            _text = UiBuilder.Text("Text", _piece.Content, 38, TvColors.AnswerText(_index), true, TextAlignmentOptions.Left);
            UiBuilder.Fit(_text, 26, 38, 2);
            UiBuilder.Anchor(_text.rectTransform, 0, 0, 1, 1);
            _text.rectTransform.offsetMin = new Vector2(16f + 58f + 16f, 8f);
            _text.rectTransform.offsetMax = new Vector2(-20f, -8f);
        }
    }
}
