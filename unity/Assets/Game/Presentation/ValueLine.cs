using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The closest value reveal (docs/04_formats.md): a number line from range min to range max, each guess as its
    /// player's character chip on a stem (chips that would cover each other stack in lanes), the true answer as a flag
    /// with "value unit", and a crown on the closest chip or chips. Players without a guess are not on the line.
    /// Things are placed by anchor fractions, so the line is right even before its first layout.
    /// </summary>
    public sealed class ValueLine : MonoBehaviour
    {
        private const float LineThickness = 10f;
        private const float ChipWidth = 230f;
        private const float ChipHeight = 64f;
        private const float ChipNameSize = 36f;
        private const float ChipSpacing = 10f;
        private const float StemWidth = 4f;
        private const float StemGap = 24f;

        /// <summary>A lane holds a chip and its guess caption above it (CharacterChip's caption is 60 tall plus a gap).</summary>
        private const float LaneStep = 140f;

        private const float PoleWidth = 8f;
        private const float TagHeight = 72f;
        private const float TagWidth = 360f;
        private const float CrownWidth = 60f;
        private const float CrownHeight = 40f;
        private const float FallbackWidth = 1600f;
        private const float FallbackHeight = 420f;

        /// <summary>Past this fraction the flag's tag points left so it stays on screen.</summary>
        private const float TagFlipFraction = 0.7f;

        private readonly List<CharacterChip> _chips = new List<CharacterChip>();
        private readonly List<Image> _stems = new List<Image>();
        private readonly List<GameObject> _crowns = new List<GameObject>();
        private readonly List<string> _ids = new List<string>();
        private TvContext _context;
        private RectTransform _plot;
        private RectTransform _stemsRoot;
        private RectTransform _chipsRoot;
        private TextMeshProUGUI _min;
        private TextMeshProUGUI _max;
        private Image _pole;
        private Image _tag;
        private TextMeshProUGUI _tagText;

        public RectTransform Rect => (RectTransform)transform;

        public static ValueLine Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Rect("ValueLine", parent);
            var line = rect.gameObject.AddComponent<ValueLine>();
            line._context = context;
            line.Build();
            return line;
        }

        /// <summary>
        /// Draws the line for the question in play from the players' turns. Chips are rebuilt only when the set of
        /// guessers changed (a player left); otherwise only names, colors and connection update. pop: the closest chips pop.
        /// </summary>
        public void Show(GameState state, bool pop)
        {
            var question = state.Question;
            if (question == null || !question.Def.Answer.HasValue) return;
            var def = question.Def;
            double min = def.Range != null && def.Range.Length > 0 ? def.Range[0] : def.Answer.Value;
            double max = def.Range != null && def.Range.Length > 1 ? def.Range[1] : def.Answer.Value;
            var lang = _context.Lang;

            _min.text = NumberText.Format(min);
            _max.text = NumberText.Format(max);
            PlaceFlag((float)NumberLineLayout.Fraction(def.Answer.Value, min, max),
                NumberText.WithUnit(_context.Format("tv.closestValue.answer", "value", NumberText.Format(def.Answer.Value),
                    "unit", def.Unit != null ? def.Unit.Get(lang) : string.Empty)));

            var guessers = new List<PlayerState>();
            foreach (var p in state.Players)
            {
                if (p.Turn != null && p.Turn.Value.HasValue) guessers.Add(p);
            }

            bool rebuild = guessers.Count != _ids.Count;
            for (int i = 0; !rebuild && i < guessers.Count; i++) rebuild = guessers[i].PlayerId != _ids[i];
            if (rebuild) Rebuild(guessers);

            var fractions = new double[guessers.Count];
            for (int i = 0; i < guessers.Count; i++) fractions[i] = NumberLineLayout.Fraction(guessers[i].Turn.Value.Value, min, max);
            float width = _plot.rect.width > 0f ? _plot.rect.width : FallbackWidth;
            float height = _plot.rect.height > 0f ? _plot.rect.height : FallbackHeight;
            var lanes = NumberLineLayout.Lanes(fractions, (ChipWidth + ChipSpacing) / width);
            int laneCount = NumberLineLayout.LaneCount(lanes);
            // Many stacked lanes squeeze together rather than leave the line's area.
            float step = laneCount > 1 ? Mathf.Min(LaneStep, (height - StemGap - LaneStep) / (laneCount - 1)) : LaneStep;
            step = Mathf.Max(ChipHeight, step);

            for (int i = 0; i < guessers.Count; i++)
            {
                var player = guessers[i];
                var turn = player.Turn;
                float f = (float)fractions[i];
                float y = StemGap + lanes[i] * step;

                var chip = _chips[i];
                Pin(chip.Rect, f, y, ChipWidth, ChipHeight);
                chip.Set(player.Name, _context.ChipColor(player.CharacterId), player.Connected);
                chip.SetCaption(NumberText.Format(turn.Value.Value), TvColors.Text);
                chip.SetLocked(turn.Closest);

                Pin(_stems[i].rectTransform, f, 0f, StemWidth, y);
                _stems[i].color = _context.ChipColor(player.CharacterId);

                _crowns[i].SetActive(turn.Closest);
                if (pop && turn.Closest) chip.Pop();
            }
        }

        private void Rebuild(List<PlayerState> guessers)
        {
            foreach (var chip in _chips) Destroy(chip.gameObject);
            foreach (var stem in _stems) Destroy(stem.gameObject);
            _chips.Clear();
            _stems.Clear();
            _crowns.Clear();
            _ids.Clear();
            foreach (var p in guessers)
            {
                _ids.Add(p.PlayerId);
                _stems.Add(UiBuilder.Image("Stem " + p.PlayerId, _stemsRoot, TvColors.DimText));
                var chip = CharacterChip.Create(_chipsRoot, p.PlayerId, ChipNameSize);
                _chips.Add(chip);
                _crowns.Add(CreateCrown(chip.transform));
            }
        }

        private void PlaceFlag(float f, string text)
        {
            // The pole runs from the line to the top of the plot.
            var pole = _pole.rectTransform;
            pole.anchorMin = new Vector2(f, 0f);
            pole.anchorMax = new Vector2(f, 1f);
            pole.pivot = new Vector2(0.5f, 0f);
            pole.anchoredPosition = Vector2.zero;
            pole.sizeDelta = new Vector2(PoleWidth, 0f);

            bool left = f > TagFlipFraction;
            var tag = _tag.rectTransform;
            tag.anchorMin = new Vector2(f, 1f);
            tag.anchorMax = new Vector2(f, 1f);
            tag.pivot = new Vector2(left ? 1f : 0f, 1f);
            tag.anchoredPosition = new Vector2(left ? -PoleWidth / 2 : PoleWidth / 2, 0f);
            tag.sizeDelta = new Vector2(TagWidth, TagHeight);
            _tagText.text = text;
        }

        /// <summary>A rect whose bottom middle sits at fraction f of the plot's width, y pixels above the line.</summary>
        private static void Pin(RectTransform rect, float f, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(f, 0f);
            rect.anchorMax = new Vector2(f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>A grey box crown: a gold band with three points, on the chip's top left corner.</summary>
        private static GameObject CreateCrown(Transform chip)
        {
            var crown = UiBuilder.Rect("Crown", chip);
            crown.anchorMin = new Vector2(0f, 1f);
            crown.anchorMax = new Vector2(0f, 1f);
            crown.pivot = new Vector2(0.5f, 0.5f);
            crown.anchoredPosition = new Vector2(8f, 4f);
            crown.sizeDelta = new Vector2(CrownWidth, CrownHeight);
            crown.localRotation = Quaternion.Euler(0f, 0f, 20f);

            var band = UiBuilder.Image("Band", crown, TvColors.Crown);
            UiBuilder.Anchor(band.rectTransform, 0f, 0f, 1f, 0.5f);
            for (int i = 0; i < 3; i++)
            {
                var point = UiBuilder.Image("Point " + i, crown, TvColors.Crown);
                float x = i * 0.4f;
                UiBuilder.Anchor(point.rectTransform, x, 0.45f, x + 0.2f, 1f);
            }

            crown.gameObject.SetActive(false);
            return crown.gameObject;
        }

        private void Build()
        {
            // The plot: the area above the line; the line is its bottom edge. Inset so chips at the ends stay on screen.
            _plot = UiBuilder.Anchor(UiBuilder.Rect("Plot", transform), 0f, 0.22f, 1f, 1f);
            _plot.offsetMin = new Vector2(ChipWidth / 2, 0f);
            _plot.offsetMax = new Vector2(-ChipWidth / 2, 0f);

            var line = UiBuilder.Image("Line", _plot, TvColors.DimText, RoundedSprite.Get());
            var lineRect = line.rectTransform;
            lineRect.anchorMin = new Vector2(0f, 0f);
            lineRect.anchorMax = new Vector2(1f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0.5f);
            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = new Vector2(0f, LineThickness);

            _min = EndLabel("Min", 0f);
            _max = EndLabel("Max", 1f);

            _stemsRoot = UiBuilder.Anchor(UiBuilder.Rect("Stems", _plot), 0f, 0f, 1f, 1f);
            _pole = UiBuilder.Image("Pole", _plot, TvColors.Correct);
            _chipsRoot = UiBuilder.Anchor(UiBuilder.Rect("Chips", _plot), 0f, 0f, 1f, 1f);

            _tag = UiBuilder.Image("Flag", _plot, TvColors.Correct, RoundedSprite.Get());
            _tagText = UiBuilder.Text("Text", _tag.transform, 52, TvColors.Background, true, TextAlignmentOptions.Center);
            _tagText.textWrappingMode = TextWrappingModes.NoWrap;
            _tagText.enableAutoSizing = true;
            _tagText.fontSizeMin = 32;
            _tagText.fontSizeMax = 52;
            UiBuilder.Anchor(_tagText.rectTransform, 0.04f, 0f, 0.96f, 1f);
        }

        /// <summary>A range end number under the line at fraction x.</summary>
        private TextMeshProUGUI EndLabel(string name, float x)
        {
            var label = UiBuilder.Text(name, _plot, 48, TvColors.DimText, true, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(x, 0f);
            rect.anchorMax = new Vector2(x, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -LineThickness);
            rect.sizeDelta = new Vector2(ChipWidth, 64f);
            return label;
        }
    }
}
