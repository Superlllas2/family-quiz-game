using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A closest value question on a wooden ruler (docs/04_formats.md, decisions/0017_game_night_ui.md): the ruler from
    /// range min to range max with ticks and the unit. In QuestionShow and Answer that is all (ShowRange). On Reveal
    /// (Show) each guess stands on the ruler as its player's token on a pin with the guessed number above it (pins that
    /// would cover each other stack in lanes), the true answer is a red flag with "value unit", and the closest token or
    /// tokens wear a crown. Players without a guess are not on the ruler. Placed by anchor fractions, so it is right
    /// even before its first layout.
    /// </summary>
    public sealed class ValueLine : MonoBehaviour
    {
        private const float RulerHeight = 64f;
        private const float TokenSize = 54f;
        private const float GuessHeight = 30f;
        private const float PinWidth = 4f;
        private const float PinGap = 18f;

        /// <summary>A lane holds a token with its guess beside it.</summary>
        private const float LaneStep = TokenSize + 10f;

        /// <summary>The guess's width to the right of its token.</summary>
        private const float GuessWidth = 104f;

        private const float FlagHeight = 56f;
        private const float FlagWidth = 300f;
        private const float FallbackWidth = 1500f;
        private const float FallbackHeight = 400f;
        private const int Ticks = 40;

        /// <summary>Past this fraction the flag points left so it stays on screen.</summary>
        private const float FlagFlipFraction = 0.7f;

        private readonly List<PlayerToken> _tokens = new List<PlayerToken>();
        private readonly List<Image> _pins = new List<Image>();
        private readonly List<TextMeshProUGUI> _guesses = new List<TextMeshProUGUI>();
        private readonly List<GameObject> _crowns = new List<GameObject>();
        private readonly List<string> _ids = new List<string>();
        private TvContext _context;
        private RectTransform _plot;
        private RectTransform _pinsRoot;
        private TextMeshProUGUI _min;
        private TextMeshProUGUI _max;
        private TextMeshProUGUI _unit;
        private Image _pole;
        private TvPiece _flag;
        private TextMeshProUGUI _flagText;

        public RectTransform Rect => (RectTransform)transform;

        public static ValueLine Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Rect("ValueLine", parent);
            var line = rect.gameObject.AddComponent<ValueLine>();
            line._context = context;
            line.Build();
            return line;
        }

        /// <summary>Only the ruler with the question's range and unit: no pins, no flag.</summary>
        public void ShowRange(QuestionInPlay question)
        {
            if (question == null) return;
            var def = question.Def;
            SetEnds(def);
            _pole.gameObject.SetActive(false);
            _flag.Root.gameObject.SetActive(false);
            _pinsRoot.gameObject.SetActive(false);
        }

        /// <summary>
        /// The ruler with every guess and the flag. Tokens are rebuilt only when the set of guessers changed (a player
        /// left); otherwise only letters, colors and connection update. pop: the pieces pop in.
        /// </summary>
        public void Show(GameState state, bool pop)
        {
            var question = state.Question;
            if (question == null || !question.Def.Answer.HasValue) return;
            var def = question.Def;
            SetEnds(def);
            double min = def.Range != null && def.Range.Length > 0 ? def.Range[0] : def.Answer.Value;
            double max = def.Range != null && def.Range.Length > 1 ? def.Range[1] : def.Answer.Value;
            var lang = _context.Lang;
            _pole.gameObject.SetActive(true);
            _flag.Root.gameObject.SetActive(true);
            _pinsRoot.gameObject.SetActive(true);
            PlaceFlag((float)NumberLineLayout.Fraction(def.Answer.Value, min, max),
                NumberText.WithUnit(_context.Format("tv.closestValue.answer", "value", NumberText.Format(def.Answer.Value),
                    "unit", def.Unit != null ? def.Unit.Get(lang) : string.Empty)));
            if (pop) TvTween.On(_flag.Root).Slam(0.1f);

            var guessers = new List<PlayerState>();
            foreach (var p in state.Players)
            {
                if (p.Turn != null && p.Turn.Value.HasValue) guessers.Add(p);
            }

            bool rebuild = guessers.Count != _ids.Count;
            for (int i = 0; !rebuild && i < guessers.Count; i++) rebuild = guessers[i].PlayerId != _ids[i];
            if (rebuild) Rebuild(guessers);

            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);

            var fractions = new double[guessers.Count];
            for (int i = 0; i < guessers.Count; i++) fractions[i] = NumberLineLayout.Fraction(guessers[i].Turn.Value.Value, min, max);
            float width = _plot.rect.width > 0f ? _plot.rect.width : FallbackWidth;
            float height = _plot.rect.height > 0f ? _plot.rect.height : FallbackHeight;
            // A pin takes its token and the guess to its right: lanes keep pins that close apart.
            var lanes = NumberLineLayout.Lanes(fractions, (TokenSize + 6f + GuessWidth) / width);
            int laneCount = NumberLineLayout.LaneCount(lanes);
            // Many stacked lanes squeeze together rather than leave the ruler's area (and stay under the flag).
            float room = height - PinGap - FlagHeight - LaneStep;
            float step = laneCount > 1 ? Mathf.Min(LaneStep, room / (laneCount - 1)) : LaneStep;
            step = Mathf.Max(TokenSize * 0.75f, step);

            for (int i = 0; i < guessers.Count; i++)
            {
                var player = guessers[i];
                var turn = player.Turn;
                float f = (float)fractions[i];
                float y = PinGap + lanes[i] * step;
                var color = _context.ChipColor(player.CharacterId);
                int seat = IndexOf(state, player.PlayerId);

                var token = _tokens[i];
                Pin(token.Rect, f, y, TokenSize, TokenSize);
                token.Set(seat >= 0 ? labels[seat] : "?", color, player.Connected);
                var g = _guesses[i].rectTransform;
                g.anchorMin = g.anchorMax = new Vector2(f, 0f);
                g.pivot = new Vector2(0f, 0f);
                g.anchoredPosition = new Vector2(TokenSize / 2f + 6f, y + (TokenSize - GuessHeight) / 2f);
                g.sizeDelta = new Vector2(GuessWidth, GuessHeight);
                _guesses[i].text = NumberText.Format(turn.Value.Value);

                Pin(_pins[i].rectTransform, f, 0f, PinWidth, y + TokenSize * 0.5f);
                _pins[i].color = TvColors.Ink;

                _crowns[i].SetActive(turn.Closest);
                if (pop)
                {
                    TvTween.On(token.Rect).Pop(0.35f + 0.05f * i, 0.35f);
                    if (turn.Closest) TvTween.On(_crowns[i].transform).Slam(0.7f, 0.35f);
                }
            }
        }

        private static int IndexOf(GameState state, string playerId)
        {
            for (int i = 0; i < state.Players.Count; i++) if (state.Players[i].PlayerId == playerId) return i;
            return -1;
        }

        private void SetEnds(QuestionDef def)
        {
            double min = def.Range != null && def.Range.Length > 0 ? def.Range[0] : 0;
            double max = def.Range != null && def.Range.Length > 1 ? def.Range[1] : 0;
            _min.text = NumberText.Format(min);
            _max.text = NumberText.Format(max);
            _unit.text = def.Unit != null ? def.Unit.Get(_context.Lang) : string.Empty;
        }

        private void Rebuild(List<PlayerState> guessers)
        {
            foreach (var t in _tokens) Destroy(t.gameObject);
            foreach (var p in _pins) Destroy(p.gameObject);
            foreach (var g in _guesses) Destroy(g.gameObject);
            _tokens.Clear();
            _pins.Clear();
            _guesses.Clear();
            _crowns.Clear();
            _ids.Clear();
            foreach (var p in guessers)
            {
                _ids.Add(p.PlayerId);
                _pins.Add(UiBuilder.Image("Pin " + p.PlayerId, _pinsRoot, TvColors.Ink));
            }

            foreach (var p in guessers)
            {
                var guess = UiBuilder.Display("Guess " + p.PlayerId, _pinsRoot, 24, TvColors.Cream, TextAlignmentOptions.Left);
                UiBuilder.Fit(guess, 14, 24, 1);
                guess.outlineWidth = 0.25f;
                guess.outlineColor = TvColors.Ink;
                _guesses.Add(guess);
                var token = PlayerToken.Create(_pinsRoot, p.PlayerId);
                _tokens.Add(token);
                _crowns.Add(CreateCrown(token.transform));
            }
        }

        private void PlaceFlag(float f, string text)
        {
            // The pole runs from the ruler to the top of the plot, the flag hangs from its top.
            var pole = _pole.rectTransform;
            pole.anchorMin = new Vector2(f, 0f);
            pole.anchorMax = new Vector2(f, 1f);
            pole.pivot = new Vector2(0.5f, 0f);
            pole.anchoredPosition = Vector2.zero;
            pole.sizeDelta = new Vector2(6f, 0f);

            bool left = f > FlagFlipFraction;
            var flag = _flag.Root;
            flag.anchorMin = new Vector2(f, 1f);
            flag.anchorMax = new Vector2(f, 1f);
            flag.pivot = new Vector2(left ? 1f : 0f, 1f);
            flag.anchoredPosition = new Vector2(left ? -3f : 3f, 0f);
            flag.sizeDelta = new Vector2(FlagWidth, FlagHeight);
            _flagText.text = text;
        }

        /// <summary>A rect whose bottom middle sits at fraction f of the plot's width, y units above the ruler.</summary>
        private static void Pin(RectTransform rect, float f, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(f, 0f);
            rect.anchorMax = new Vector2(f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        /// <summary>A mustard crown sticker on the token's top left: a star burst with "1".</summary>
        private static GameObject CreateCrown(Transform token)
        {
            var crown = TvPiece.Star("Crown", token, TvColors.Mustard);
            var rect = crown.Root;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(4f, -4f);
            rect.sizeDelta = new Vector2(34f, 34f);
            var mark = UiBuilder.Display("Mark", crown.Content, 16, TvColors.Ink, TextAlignmentOptions.Center);
            mark.text = "1";
            UiBuilder.Anchor(mark.rectTransform, 0, 0, 1, 1);
            crown.Root.gameObject.SetActive(false);
            return crown.Root.gameObject;
        }

        private void Build()
        {
            // The ruler: a wood piece along the bottom with ticks, the ends' numbers on it and the unit in the middle.
            var ruler = TvPiece.Card("Ruler", transform, TvColors.Wood, 10f, 7f);
            var r = ruler.Root;
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(1f, 0f);
            r.pivot = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(0f, 7f);
            r.sizeDelta = new Vector2(0f, RulerHeight);

            // The plot: the area above the ruler, inset so tokens at the ends stay on the ruler.
            _plot = UiBuilder.Anchor(UiBuilder.Rect("Plot", transform), 0f, 0f, 1f, 1f);
            _plot.offsetMin = new Vector2(70f, 7f + RulerHeight);
            _plot.offsetMax = new Vector2(-70f, 0f);

            var ticks = UiBuilder.Rect("Ticks", ruler.Content);
            UiBuilder.Anchor(ticks, 0f, 0f, 1f, 1f);
            ticks.offsetMin = new Vector2(70f, 0f);
            ticks.offsetMax = new Vector2(-70f, 0f);
            for (int i = 0; i <= Ticks; i++)
            {
                bool major = i % 5 == 0;
                var tick = UiBuilder.Image("Tick " + i, ticks, TvColors.Ink);
                var t = tick.rectTransform;
                float x = (float)i / Ticks;
                t.anchorMin = new Vector2(x, 1f);
                t.anchorMax = new Vector2(x, 1f);
                t.pivot = new Vector2(0.5f, 1f);
                t.anchoredPosition = new Vector2(0f, -4f);
                t.sizeDelta = new Vector2(major ? 4f : 2.5f, major ? 22f : 12f);
            }

            _min = UiBuilder.Display("Min", ruler.Content, 26, TvColors.Ink, TextAlignmentOptions.Left);
            UiBuilder.Fit(_min, 16, 26, 1);
            UiBuilder.Anchor(_min.rectTransform, 0f, 0f, 0.3f, 0.5f);
            _min.rectTransform.offsetMin = new Vector2(16f, 2f);
            _max = UiBuilder.Display("Max", ruler.Content, 26, TvColors.Ink, TextAlignmentOptions.Right);
            UiBuilder.Fit(_max, 16, 26, 1);
            UiBuilder.Anchor(_max.rectTransform, 0.7f, 0f, 1f, 0.5f);
            _max.rectTransform.offsetMax = new Vector2(-16f, 0f);
            _unit = UiBuilder.Text("Unit", ruler.Content, 26, TvColors.Ink, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(_unit, 16, 26, 1);
            UiBuilder.Anchor(_unit.rectTransform, 0.3f, 0f, 0.7f, 0.5f);

            _pole = UiBuilder.Image("Pole", _plot, TvColors.Ink);
            _pinsRoot = UiBuilder.Anchor(UiBuilder.Rect("Pins", _plot), 0f, 0f, 1f, 1f);

            _flag = TvPiece.Card("Flag", _plot, TvColors.Red, 8f, 5f);
            _flagText = UiBuilder.Display("Text", _flag.Content, 30, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_flagText, 18, 30, 1);
            UiBuilder.Anchor(_flagText.rectTransform, 0.05f, 0.08f, 0.95f, 0.92f);
        }
    }
}
