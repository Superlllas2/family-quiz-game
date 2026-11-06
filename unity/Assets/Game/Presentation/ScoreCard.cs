using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One player's card in the Scores row (decisions/0017_game_night_ui.md): cream card stock with the rank on a coin,
    /// the player's token, name and score, and the points just earned on a sticker. It glides along the row from its old
    /// place to its new one while the score counts up, on unscaled time.
    /// </summary>
    public sealed class ScoreCard : MonoBehaviour
    {
        private TvPiece _card;
        private TvPiece _coin;
        private TextMeshProUGUI _rank;
        private PlayerToken _token;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _score;
        private TvPiece _sticker;
        private TextMeshProUGUI _stickerText;
        private float _fromX;
        private float _toX;
        private int _fromScore;
        private int _toScore;
        private int _toRank;
        private float _delay;
        private float _age = -1f;

        public string PlayerId { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        public static ScoreCard Create(Transform parent, string playerId)
        {
            var rect = UiBuilder.Rect("Score " + playerId, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            var card = rect.gameObject.AddComponent<ScoreCard>();
            card.PlayerId = playerId;
            card.Build();
            return card;
        }

        public void Set(string label, string name, Color color, bool connected, string points)
        {
            _token.Set(label, color, connected);
            _name.text = name;
            bool show = !string.IsNullOrEmpty(points);
            _sticker.Root.gameObject.SetActive(show);
            _stickerText.text = points ?? string.Empty;
        }

        /// <summary>Starts at fromX with fromScore and fromRank, waits delay, then glides to toX while the score counts up.</summary>
        public void Move(float fromX, float toX, float width, float height, int fromScore, int toScore, int fromRank, int toRank, float delay)
        {
            Rect.sizeDelta = new Vector2(width, height);
            Rect.anchoredPosition = new Vector2(fromX, 0f);
            _fromX = fromX;
            _toX = toX;
            _fromScore = fromScore;
            _toScore = toScore;
            _toRank = toRank;
            _delay = delay;
            _age = 0f;
            _score.text = fromScore.ToString();
            SetRank(fromRank);
            if (_sticker.Root.gameObject.activeSelf) TvTween.On(_sticker.Root).Slam(0.1f, 0.4f);
        }

        /// <summary>At its final place at once.</summary>
        public void Place(float x, float width, float height, int score, int rank)
        {
            _age = -1f;
            Rect.sizeDelta = new Vector2(width, height);
            Rect.anchoredPosition = new Vector2(x, 0f);
            _score.text = score.ToString();
            SetRank(rank);
        }

        private void SetRank(int rank)
        {
            _rank.text = rank.ToString();
            _coin.SetFill(rank == 1 ? TvColors.Mustard : TvColors.Cream2);
        }

        private void Update()
        {
            if (_age < 0f) return;
            _age += Time.unscaledDeltaTime;
            if (_age < _delay) return;
            float t = Mathf.Clamp01((_age - _delay) / TvMotion.RowMoveSeconds);
            float eased = t * t * (3f - 2f * t);
            Rect.anchoredPosition = new Vector2(Mathf.Lerp(_fromX, _toX, eased), 0f);
            _score.text = Mathf.RoundToInt(Mathf.Lerp(_fromScore, _toScore, eased)).ToString();
            SetRank(_toRank);
            if (t >= 1f) _age = -1f;
        }

        private void OnDisable()
        {
            if (_age < 0f) return;
            _age = -1f;
            Rect.anchoredPosition = new Vector2(_toX, 0f);
            _score.text = _toScore.ToString();
            SetRank(_toRank);
        }

        private void Build()
        {
            _card = TvPiece.Card("Card", transform, TvColors.Cream, 18f, 7f);
            UiBuilder.Anchor(_card.Root, 0, 0, 1, 1);
            _card.Root.offsetMax = new Vector2(0f, -22f); // room above for the coin and sticker

            _token = PlayerToken.Create(_card.Content, PlayerId);
            var t = _token.Rect;
            t.anchorMin = t.anchorMax = new Vector2(0f, 0.5f);
            t.pivot = new Vector2(0f, 0.5f);
            t.anchoredPosition = new Vector2(14f, -4f);
            t.sizeDelta = new Vector2(60f, 60f);

            _name = UiBuilder.Text("Name", _card.Content, 28, TvColors.Ink, true, TextAlignmentOptions.Left);
            UiBuilder.Fit(_name, 16, 28, 1);
            UiBuilder.Anchor(_name.rectTransform, 0f, 0.5f, 1f, 0.92f);
            _name.rectTransform.offsetMin = new Vector2(84f, 0f);
            _name.rectTransform.offsetMax = new Vector2(-12f, 0f);

            _score = UiBuilder.Display("Score", _card.Content, 40, TvColors.Ink, TextAlignmentOptions.Left);
            UiBuilder.Fit(_score, 20, 40, 1);
            UiBuilder.Anchor(_score.rectTransform, 0f, 0.08f, 1f, 0.52f);
            _score.rectTransform.offsetMin = new Vector2(84f, 0f);
            _score.rectTransform.offsetMax = new Vector2(-12f, 0f);

            _coin = TvPiece.Disc("Rank", transform, TvColors.Cream2, 4f);
            var c = _coin.Root;
            c.anchorMin = c.anchorMax = new Vector2(0f, 1f);
            c.pivot = new Vector2(0.5f, 0.5f);
            c.anchoredPosition = new Vector2(22f, -26f);
            c.sizeDelta = new Vector2(48f, 48f);
            _rank = UiBuilder.Display("Text", _coin.Content, 22, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_rank, 12, 22, 1);
            UiBuilder.Anchor(_rank.rectTransform, 0.1f, 0.1f, 0.9f, 0.9f);

            _sticker = TvPiece.Card("Points", transform, TvColors.Green, 10f, 4f).Tilt(4f);
            TvTween.On(_sticker.Root).Tilt = 4f;
            var s = _sticker.Root;
            s.anchorMin = s.anchorMax = new Vector2(1f, 1f);
            s.pivot = new Vector2(1f, 0.5f);
            s.anchoredPosition = new Vector2(-8f, -22f);
            s.sizeDelta = new Vector2(104f, 40f);
            _stickerText = UiBuilder.Display("Text", _sticker.Content, 22, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stickerText, 12, 22, 1);
            UiBuilder.Anchor(_stickerText.rectTransform, 0.05f, 0.08f, 0.95f, 0.92f);
        }
    }
}
