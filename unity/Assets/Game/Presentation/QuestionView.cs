using System;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One question cycle on the TV (docs/02_show_flow.md): QuestionShow (question and read time bar), Answer
    /// (question, 2x2 options, countdown, chips that pop on lock in, never what anyone chose) and Reveal (the correct
    /// tile grows green, the others fade, each chip shows its points). Reads GameState; changes nothing.
    /// </summary>
    public sealed class QuestionView : MonoBehaviour
    {
        private const int OptionCount = 4;
        private const float TileGap = 24f;
        private const float ChipHeight = 86f;

        private readonly AnswerTile[] _tiles = new AnswerTile[OptionCount];
        private TvContext _context;
        private TextMeshProUGUI _number;
        private TextMeshProUGUI _seconds;
        private TextMeshProUGUI _question;
        private TextMeshProUGUI _answered;
        private TimerBar _bar;
        private RectTransform _tilesRoot;
        private ChipRow _chips;
        private GamePhase _mode;
        private long _answerSpanMs;
        private bool _revealApplied;

        public static QuestionView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("QuestionView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<QuestionView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Called on PhaseStarted for QuestionShow, Answer and Reveal.</summary>
        public void Enter(GamePhase phase)
        {
            var previous = _mode;
            _mode = phase;
            var state = _context.Game.State;
            var question = state.Question;
            var lang = _context.Lang;
            _number.text = question != null
                ? _context.Format("tv.question.number", "number", question.Number.ToString(), "total", state.QuestionCount.ToString())
                : string.Empty;
            _question.text = question != null ? question.Def.Text.Get(lang) : string.Empty;

            bool show = phase == GamePhase.QuestionShow;
            bool answer = phase == GamePhase.Answer;
            bool reveal = phase == GamePhase.Reveal;
            LayoutQuestion(show);
            _tilesRoot.gameObject.SetActive(!show);
            _chips.gameObject.SetActive(!show);
            _bar.SetVisible(show || answer);
            _seconds.gameObject.SetActive(answer);
            _answered.gameObject.SetActive(answer);

            if (answer || (reveal && previous != GamePhase.Answer))
            {
                for (int i = 0; i < OptionCount; i++)
                {
                    bool has = question != null && question.Def.Options != null && i < question.Def.Options.Count;
                    _tiles[i].gameObject.SetActive(has);
                    _tiles[i].ResetLook(has ? question.OptionText(i, lang) : string.Empty);
                }
            }

            if (show || answer)
            {
                _revealApplied = false;
                _answerSpanMs = answer ? AnswerClock.Span(state) : 0;
                foreach (var chip in _chips.Chips) chip.SetCaption(null, TvColors.Text);
            }

            Refresh();
            if (reveal && question != null) ApplyReveal(question.CorrectDisplayIndex);
            Tick();
        }

        /// <summary>Roster, connection or player changes: chips follow, captions on Reveal.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _chips.Sync(state, ChipHeight);
            for (int i = 0; i < state.Players.Count && i < _chips.Chips.Count; i++)
            {
                var player = state.Players[i];
                var chip = _chips.Chips[i];
                chip.SetLocked(_mode == GamePhase.Answer && player.Turn != null && player.Turn.Locked);
                if (_mode == GamePhase.Reveal) SetResultCaption(chip, player);
            }
        }

        public void OnAnswerLocked(string playerId)
        {
            if (_mode != GamePhase.Answer) return;
            var chip = _chips.Find(playerId);
            if (chip == null) return;
            chip.SetLocked(true);
            chip.Pop();
        }

        public void OnRevealed(int correctDisplayIndex)
        {
            if (_mode == GamePhase.Reveal) ApplyReveal(correctDisplayIndex);
        }

        /// <summary>Per frame: the timer bars, the seconds left and the answered count, all from the core clock.</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            long now = game.NowMs;
            if (_mode == GamePhase.QuestionShow)
            {
                long span = state.PhaseEndsAtMs.HasValue ? state.PhaseEndsAtMs.Value - state.PhaseStartedAtMs : 0;
                float read = span > 0 ? (float)(now - state.PhaseStartedAtMs) / span : 1f;
                _bar.Set(read, TvColors.Text);
                return;
            }

            if (_mode != GamePhase.Answer) return;
            long left = Math.Max(0, AnswerClock.LatestOpenDeadline(state, now) - now);
            bool lastCall = left <= (long)(game.Config.Timers.LastCall * 1000);
            _bar.Set(_answerSpanMs > 0 ? (float)left / _answerSpanMs : 0f, lastCall ? TvColors.Error : TvColors.Text);
            _seconds.text = ((left + 999) / 1000).ToString();
            _seconds.color = lastCall ? TvColors.Error : TvColors.Text;

            AnswerClock.Count(state, out int locked, out int total);
            _answered.text = _context.Format("tv.question.answered", "count", locked.ToString(), "total", total.ToString());
        }

        private void ApplyReveal(int correct)
        {
            for (int i = 0; i < OptionCount; i++) _tiles[i].Reveal(i == correct);
            if (_revealApplied) return;
            _revealApplied = true;
            var state = _context.Game.State;
            for (int i = 0; i < state.Players.Count && i < _chips.Chips.Count; i++)
            {
                var turn = state.Players[i].Turn;
                _chips.Chips[i].SetLocked(false);
                SetResultCaption(_chips.Chips[i], state.Players[i]);
                if (turn != null && turn.Correct) _chips.Chips[i].Pop();
            }
        }

        private void SetResultCaption(CharacterChip chip, PlayerState player)
        {
            var turn = player.Turn;
            if (turn == null || !turn.Locked)
                chip.SetCaption(_context.T("tv.reveal.noAnswer"), TvColors.DimText);
            else if (turn.Correct)
                chip.SetCaption(_context.Format("tv.reveal.points", "points", turn.Points.ToString()), TvColors.Correct);
            else
                chip.SetCaption(_context.T("tv.reveal.wrong"), TvColors.DimText);
        }

        private void LayoutQuestion(bool centered)
        {
            var rect = _question.rectTransform;
            if (centered)
            {
                UiBuilder.Anchor(rect, 0.06f, 0.38f, 0.94f, 0.8f);
                _question.alignment = TextAlignmentOptions.Center;
                _question.fontSizeMax = 96;
                UiBuilder.Anchor(_bar.Root, 0.2f, 0.3f, 0.8f, 0.318f);
            }
            else
            {
                UiBuilder.Anchor(rect, 0.04f, 0.67f, 0.96f, 0.89f);
                _question.alignment = TextAlignmentOptions.Center;
                _question.fontSizeMax = 80;
                UiBuilder.Anchor(_bar.Root, 0.04f, 0.643f, 0.96f, 0.658f);
            }
        }

        private void Build()
        {
            var root = transform;
            _number = UiBuilder.Text("Number", root, 44, TvColors.DimText, false, TextAlignmentOptions.Left);
            UiBuilder.Anchor(_number.rectTransform, 0.03f, 0.9f, 0.7f, 0.985f);

            _seconds = UiBuilder.Text("Seconds", root, 96, TvColors.Text, true, TextAlignmentOptions.Right);
            _seconds.textWrappingMode = TextWrappingModes.NoWrap;
            _seconds.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_seconds.rectTransform, 0.8f, 0.885f, 0.97f, 0.995f);

            _question = UiBuilder.Text("Question", root, 80, TvColors.Text, true, TextAlignmentOptions.Center);
            // Two lines at most: long questions shrink to 64 before they are cut.
            _question.enableAutoSizing = true;
            _question.fontSizeMin = 64;
            _question.fontSizeMax = 80;
            _question.maxVisibleLines = 2;

            _bar = TimerBar.Create(root);

            _tilesRoot = UiBuilder.Anchor(UiBuilder.Rect("Options", root), 0.04f, 0.27f, 0.96f, 0.625f);
            for (int i = 0; i < OptionCount; i++)
            {
                _tiles[i] = AnswerTile.Create(_tilesRoot, i);
                int column = i % 2;
                int row = i / 2; // 0 is the top row: A B over C D
                float minX = column * 0.5f;
                float minY = row == 0 ? 0.5f : 0f;
                var tile = UiBuilder.Anchor(_tiles[i].Rect, minX, minY, minX + 0.5f, minY + 0.5f);
                tile.offsetMin = new Vector2(column == 1 ? TileGap / 2 : 0, row == 0 ? TileGap / 2 : 0);
                tile.offsetMax = new Vector2(column == 0 ? -TileGap / 2 : 0, row == 1 ? -TileGap / 2 : 0);
            }

            _answered = UiBuilder.Text("Answered", root, 44, TvColors.DimText, false, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_answered.rectTransform, 0.04f, 0.185f, 0.96f, 0.25f);

            _chips = ChipRow.Create(root, _context);
            UiBuilder.Anchor((RectTransform)_chips.transform, 0.02f, 0.025f, 0.98f, 0.105f);

            LayoutQuestion(true);
        }
    }
}
