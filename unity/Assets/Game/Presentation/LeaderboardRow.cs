using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One leaderboard line: shared rank, character chip, score and an extra note (points just earned, correct
    /// answers). It can glide from one place to another while its score counts up, on unscaled time.
    /// </summary>
    public sealed class LeaderboardRow : MonoBehaviour
    {
        public const float Width = 1440f;

        private const float RankWidth = 140f;
        private const float ChipWidth = 520f;
        private const float ScoreWidth = 300f;
        private const float Gap = 24f;
        private const float MarkerWidth = 250f;
        private const float MarkerHeight = 52f;
        private const float MarkerFontSize = 30f;

        private TextMeshProUGUI _rank;
        private CharacterChip _chip;
        private TextMeshProUGUI _score;
        private TextMeshProUGUI _extra;
        private PowerUpMarker _marker;
        private float _fromY;
        private float _toY;
        private int _fromScore;
        private int _toScore;
        private int _toRank;
        private float _delay;
        private float _age = -1f;

        public string PlayerId { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        public static LeaderboardRow Create(Transform parent, string playerId)
        {
            var rect = UiBuilder.Rect("Row " + playerId, parent);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            var row = rect.gameObject.AddComponent<LeaderboardRow>();
            row.PlayerId = playerId;
            row.Build();
            return row;
        }

        public void Set(int rank, string name, Color color, bool connected, string extra, Color extraColor)
        {
            _toRank = rank;
            if (_age < 0f) _rank.text = rank.ToString(); // a moving row shows its new rank when it starts to move
            _chip.Set(name, color, connected);
            _extra.text = extra ?? string.Empty;
            _extra.color = OnCream(extraColor);
        }

        /// <summary>The row is cream card stock: colours meant for the dark studio become their inks.</summary>
        private static Color OnCream(Color c)
        {
            if (c == TvColors.Correct) return TvColors.Green;
            if (c == TvColors.Crown) return new Color32(0xA8, 0x6A, 0x0C, 0xFF);
            if (c == TvColors.DimText) return new Color(TvColors.Ink.r, TvColors.Ink.g, TvColors.Ink.b, 0.7f);
            if (c == TvColors.Text) return TvColors.Ink;
            return c;
        }

        /// <summary>
        /// A row with no rank and no score: the chip and a line of text that takes the score's place too (the funny tags
        /// on Results, D86).
        /// </summary>
        public void SetTag(string name, Color color, bool connected, string tag, Color tagColor)
        {
            _rank.text = string.Empty;
            _score.text = string.Empty;
            _chip.Set(name, color, connected);
            _extra.text = tag ?? string.Empty;
            _extra.color = OnCream(tagColor);
            float x = RankWidth + Gap + ChipWidth + Gap;
            Column(_extra.rectTransform, x, Width - x - Gap, 0);
        }

        /// <summary>
        /// A power up marker ("Dad → Freeze") at the right end of the row, the extra note making room for it; null or
        /// empty hides it (docs/03_rules.md, "Using").
        /// </summary>
        public void SetHit(string text, Color color)
        {
            _marker.Set(text, color);
            float x = ExtraX();
            float width = Width - x - Gap;
            if (_marker.Visible) width -= MarkerWidth + Gap;
            Column(_extra.rectTransform, x, width, 0);
        }

        public void SetScore(int score)
        {
            _score.text = score.ToString();
            _fromScore = score;
            _toScore = score;
        }

        /// <summary>Puts the row at a distance from the top of its parent at once.</summary>
        public void Place(float y, float height)
        {
            _age = -1f;
            Rect.sizeDelta = new Vector2(Width, height);
            Rect.anchoredPosition = new Vector2(0, -y);
        }

        /// <summary>Starts at fromY with fromScore and fromRank, waits delay seconds, then glides to toY while the score counts to toScore.</summary>
        public void Move(float fromY, float toY, float height, int fromScore, int toScore, int fromRank, int toRank, float delay)
        {
            Place(fromY, height);
            _fromY = fromY;
            _toY = toY;
            _fromScore = fromScore;
            _toScore = toScore;
            _toRank = toRank;
            _delay = delay;
            _age = 0f;
            _score.text = fromScore.ToString();
            _rank.text = fromRank.ToString();
        }

        private void Update()
        {
            if (_age < 0f) return;
            _age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01((_age - _delay) / TvMotion.RowMoveSeconds);
            if (_age < _delay) return;
            float eased = t * t * (3f - 2f * t);
            Rect.anchoredPosition = new Vector2(0, -Mathf.Lerp(_fromY, _toY, eased));
            _score.text = Mathf.RoundToInt(Mathf.Lerp(_fromScore, _toScore, eased)).ToString();
            _rank.text = _toRank.ToString();
            if (t >= 1f) _age = -1f;
        }

        private void OnDisable()
        {
            if (_age < 0f) return;
            // Hidden mid move: jump to the end so a later show is right.
            _age = -1f;
            Rect.anchoredPosition = new Vector2(0, -_toY);
            _score.text = _toScore.ToString();
            _rank.text = _toRank.ToString();
        }

        private void Build()
        {
            // A cream card on its cardboard edge, a touch crooked; the rank in Unbounded, the score too.
            var card = TvPiece.Card("Card", transform, TvColors.Cream, 18f, 6f);
            UiBuilder.Anchor(card.Root, 0, 0, 1, 1);
            card.Root.SetAsFirstSibling();

            float x = 0f;
            _rank = UiBuilder.Display("Rank", transform, 52, TvColors.Ink, TextAlignmentOptions.Center);
            _rank.textWrappingMode = TextWrappingModes.NoWrap;
            _rank.overflowMode = TextOverflowModes.Overflow;
            _rank.enableAutoSizing = true;
            _rank.fontSizeMin = 24;
            _rank.fontSizeMax = 52;
            Column(_rank.rectTransform, x, RankWidth, 0);
            x += RankWidth + Gap;

            _chip = CharacterChip.Create(transform, PlayerId, 48);
            Column(_chip.Rect, x, ChipWidth, 8);
            x += ChipWidth + Gap;

            _score = UiBuilder.Display("Score", transform, 56, TvColors.Ink, TextAlignmentOptions.Right);
            _score.textWrappingMode = TextWrappingModes.NoWrap;
            _score.overflowMode = TextOverflowModes.Overflow;
            _score.enableAutoSizing = true;
            _score.fontSizeMin = 24;
            _score.fontSizeMax = 56;
            Column(_score.rectTransform, x, ScoreWidth, 0);
            x += ScoreWidth + Gap;

            _extra = UiBuilder.Text("Extra", transform, 44, TvColors.Ink, true, TextAlignmentOptions.Left);
            _extra.textWrappingMode = TextWrappingModes.NoWrap;
            _extra.enableAutoSizing = true;
            _extra.fontSizeMin = 26;
            _extra.fontSizeMax = 44;
            Column(_extra.rectTransform, x, Width - x - Gap, 0);

            _marker = PowerUpMarker.Create(transform, MarkerFontSize);
            var marker = _marker.Root;
            marker.anchorMin = new Vector2(1, 0.5f);
            marker.anchorMax = new Vector2(1, 0.5f);
            marker.pivot = new Vector2(1, 0.5f);
            marker.anchoredPosition = new Vector2(-Gap, 0);
            marker.sizeDelta = new Vector2(MarkerWidth, MarkerHeight);
        }

        /// <summary>The extra note's left edge: after the rank, the chip and the score.</summary>
        private static float ExtraX()
        {
            return RankWidth + Gap + ChipWidth + Gap + ScoreWidth + Gap;
        }

        /// <summary>A column at x from the row's left edge, full height minus an inset at the top and bottom.</summary>
        private static void Column(RectTransform rect, float x, float width, float inset)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0);
            rect.sizeDelta = new Vector2(width, -2 * inset);
        }
    }
}
