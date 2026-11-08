using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One multiple choice option on the TV (decisions/0017_game_night_ui.md): a chunky tile in the answer ink, a little
    /// crooked, with its letter on a cream disc. In Answer it is empty, A to D only: the option texts are on the phones
    /// (D141). On Reveal the text appears, the right tile grows, gets a cream halo and the stamp (QuestionView), the
    /// others sink onto the table and fade, and each player who picked a tile sits on its edge as a pill with their
    /// token, name and points (D142).
    /// </summary>
    public sealed class AnswerTile : MonoBehaviour
    {
        /// <summary>Answer letters A to D (docs/06_phone_screens.md), the same in both languages.</summary>
        private const string Letters = "ABCD";

        private const float Edge = 8f;
        private const float SunkEdge = 2f;
        private const float PillHeight = 40f;
        private const float PillWidth = 176f;
        private const float PillGap = 6f;
        private const float HaloSize = 10f;

        /// <summary>The right end of the right tile kept for the stamp (QuestionView puts it there).</summary>
        public const float StampRoom = 215f;
        private static readonly float[] Tilts = { -0.6f, 0.5f, 0.4f, -0.7f };

        private readonly List<GameObject> _picks = new List<GameObject>();
        private TvPiece _halo;
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
            _text.rectTransform.offsetMax = new Vector2(-20f, -4f);
            _halo.Root.gameObject.SetActive(false);
            ClearPicks();
            if (animate) _tween.Pop(delay, 0.5f);
            else _tween.Finish();
        }

        public void Reveal(bool correct)
        {
            if (correct)
            {
                _targetScale = 1.06f;
                _targetDrop = 0f;
                _halo.Root.gameObject.SetActive(true); // D142: the right tile stands out clearly
                // The stamp lands on the right end: the text makes room for it (and shrinks if it must).
                _text.rectTransform.offsetMax = new Vector2(-StampRoom, -4f);
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
        /// The players who picked this tile (D142), as pills on its outer edge (onTop: the top edge, else the bottom
        /// edge), half on the tile and half in the gap around the tiles, from the left: each with the player's token,
        /// name and points. Many picks overlap like a pile of cards rather than run off the tile.
        /// </summary>
        public void SetPicks(IReadOnlyList<(string Id, string Label, Color Color, string Name, string Points)> picks, bool animate, bool onTop)
        {
            ClearPicks();
            float width = Rect.rect.width > 0f ? Rect.rect.width : 640f;
            float room = width - 2 * 20f - PillWidth;
            float step = picks.Count > 1 ? Mathf.Min(PillWidth + PillGap, room / (picks.Count - 1)) : 0f;
            for (int i = 0; i < picks.Count; i++)
            {
                var pill = TvPiece.Card("Pick " + picks[i].Id, transform, TvColors.Cream, PillHeight / 2f, 3f);
                var rect = pill.Root;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, onTop ? 1f : 0f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(20f + i * step, 0f);
                rect.sizeDelta = new Vector2(PillWidth, PillHeight);

                var token = PlayerToken.Create(pill.Content, picks[i].Id);
                var t = token.Rect;
                t.anchorMin = t.anchorMax = new Vector2(0f, 0.5f);
                t.pivot = new Vector2(0f, 0.5f);
                t.anchoredPosition = new Vector2(3f, 0f);
                t.sizeDelta = new Vector2(PillHeight - 6f, PillHeight - 6f);
                token.Set(picks[i].Label, picks[i].Color, true);

                var points = UiBuilder.Display("Points", pill.Content, 20, TvColors.Ink, TextAlignmentOptions.Right);
                points.textWrappingMode = TextWrappingModes.NoWrap;
                UiBuilder.Anchor(points.rectTransform, 0.62f, 0, 1, 1);
                points.rectTransform.offsetMax = new Vector2(-12f, 0f);
                points.text = picks[i].Points;

                var name = UiBuilder.Text("Name", pill.Content, 20, TvColors.Ink, true, TextAlignmentOptions.Left);
                UiBuilder.Fit(name, 12, 20, 1);
                UiBuilder.Anchor(name.rectTransform, 0, 0, 0.62f, 1);
                name.rectTransform.offsetMin = new Vector2(PillHeight + 2f, 0f);
                name.text = picks[i].Name;

                if (animate) TvTween.On(rect).Pop(0.35f + 0.07f * i, 0.35f);
                _picks.Add(rect.gameObject);
            }
        }

        private void ClearPicks()
        {
            foreach (var t in _picks) Destroy(t);
            _picks.Clear();
        }

        private void Update()
        {
            float k = 1f - Mathf.Exp(-TvMotion.TileEaseSpeed * Time.unscaledDeltaTime);
            var root = _piece.Root;
            float scale = Mathf.Lerp(root.localScale.x, _targetScale, k);
            root.localScale = new Vector3(scale, scale, 1f);
            _halo.Root.localScale = root.localScale; // the halo grows with the right tile
            var pos = root.anchoredPosition;
            root.anchoredPosition = new Vector2(pos.x, Mathf.Lerp(pos.y, -_targetDrop, k));
        }

        private void Build()
        {
            _halo = TvPiece.Card("Halo", transform, TvColors.Cream, 26f, 0f);
            UiBuilder.Anchor(_halo.Root, 0, 0, 1, 1);
            _halo.Root.offsetMin = new Vector2(-HaloSize, -HaloSize);
            _halo.Root.offsetMax = new Vector2(HaloSize, HaloSize);
            _halo.Root.gameObject.SetActive(false);
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
            UiBuilder.Fit(_text, 22, 38, 2);
            UiBuilder.Anchor(_text.rectTransform, 0, 0, 1, 1);
            _text.rectTransform.offsetMin = new Vector2(16f + 58f + 16f, 4f);
            _text.rectTransform.offsetMax = new Vector2(-20f, -4f);
        }
    }
}
