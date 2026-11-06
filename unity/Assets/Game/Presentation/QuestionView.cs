using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One question cycle on the TV (docs/02_show_flow.md, decisions/0017_game_night_ui.md), all on the TvLayout zones:
    /// <list type="bullet">
    /// <item>QuestionShow: the question card, large, on the board of the Stage shot, with the read time burning down its
    /// fuse; a closest value question shows its ruler under it.</item>
    /// <item>Answer: the card, small, under the guests of the Wide shot, and the four tiles under it (or the ruler); the
    /// clock token and the players' tokens in the status corner, a token gets its check when its player locks in.
    /// Never what anyone chose.</item>
    /// <item>Reveal: the stamp slams onto the right tile, the others sink; each player's token sits on the tile they
    /// picked and their points show as a sticker under their status token; then the card flips over to "Why?" and the
    /// fact. A closest value Reveal shows the ruler with every guess on a pin, the flag and the crown instead.</item>
    /// </list>
    /// Power up tags show under the status corner. Reads GameState; changes nothing.
    /// </summary>
    public sealed class QuestionView : MonoBehaviour
    {
        private const int OptionCount = 4;
        private const float TileGapX = 26f;
        private const float TileGapY = 30f;

        /// <summary>When the card flips to the fact on Reveal, after the stamp landed.</summary>
        private const float FlipDelaySeconds = 0.9f;

        private readonly AnswerTile[] _tiles = new AnswerTile[OptionCount];
        private TvContext _context;
        private QuestionCard _card;
        private RectTransform _tilesRoot;
        private ValueLine _valueLine;
        private StatusStrip _status;
        private HitList _hits;
        private RectTransform _stamp;
        private TextMeshProUGUI _stampText;
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
            bool show = phase == GamePhase.QuestionShow;
            bool answer = phase == GamePhase.Answer;
            bool reveal = phase == GamePhase.Reveal;
            bool closest = question != null && question.IsClosestValue;
            bool fresh = !(reveal && previous == GamePhase.Answer);

            // The card: large on the board, small under the guests; its front each time the question is set up.
            TvLayout.Put(_card.Rect, show ? TvLayout.ShowCard : TvLayout.AnswerCard);
            _card.SetLarge(show);
            _card.gameObject.SetActive(!(reveal && closest));
            if (fresh) ShowFront(question);
            if (show || answer) _card.PopIn(show ? 0.2f : 0f);

            _tilesRoot.gameObject.SetActive(!show && !closest);
            _stamp.gameObject.SetActive(false);
            if (answer || (reveal && fresh))
            {
                for (int i = 0; i < OptionCount; i++)
                {
                    bool has = question != null && question.Def.Options != null && i < question.Def.Options.Count;
                    _tiles[i].gameObject.SetActive(has);
                    _tiles[i].ResetLook(has ? question.OptionText(i, _context.Lang) : string.Empty, answer, 0.15f + 0.08f * i);
                }
            }

            _valueLine.gameObject.SetActive(closest);
            if (closest)
            {
                TvLayout.Put(_valueLine.Rect, show ? TvLayout.ShowRuler : reveal ? TvLayout.RulerReveal : TvLayout.Ruler);
                _valueLine.ShowRange(question);
                if (show || answer) TvTween.On(_valueLine.Rect).Rise(show ? 0.4f : 0.1f);
            }

            _status.gameObject.SetActive(!show);
            _status.SetClockVisible(answer);
            if (show || answer)
            {
                _revealApplied = false;
                _answerSpanMs = answer ? AnswerClock.Span(state) : 0;
                _status.Sync(state);
                _status.ClearMarks();
            }

            Refresh();
            if (reveal && question != null) ApplyReveal(question.CorrectDisplayIndex);
            Tick();
        }

        /// <summary>Roster, connection or player changes: tokens follow; their marks come from the state.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _status.Sync(state);
            if (_mode == GamePhase.Answer)
            {
                for (int i = 0; i < state.Players.Count && i < _status.Tokens.Count; i++)
                {
                    var turn = state.Players[i].Turn;
                    _status.Tokens[i].SetLocked(turn != null && turn.Locked);
                }
            }

            if (_mode == GamePhase.Reveal && _revealApplied)
            {
                SetStickers(state, false);
                if (IsClosestValue()) _valueLine.Show(state, false);
            }

            RefreshHits();
        }

        /// <summary>
        /// Power up tags (docs/03_rules.md, "Using"): in Answer the effect that landed on each target (PowerUpApplied),
        /// in Reveal the hits used in this pick window (PowerUpUsed) that land at the next Answer.
        /// </summary>
        public void RefreshHits()
        {
            if (_mode == GamePhase.QuestionShow) _hits.Clear();
            else _hits.Refresh(_context.Game.State, _mode);
        }

        public void OnAnswerLocked(string playerId)
        {
            if (_mode != GamePhase.Answer) return;
            var token = _status.Find(playerId);
            if (token != null) token.SetLocked(true, true);
        }

        public void OnRevealed(int correctDisplayIndex)
        {
            if (_mode == GamePhase.Reveal) ApplyReveal(correctDisplayIndex);
        }

        /// <summary>Per frame: the fuse and the clock, from the core clock.</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            long now = game.NowMs;
            if (_mode == GamePhase.QuestionShow)
            {
                long span = state.PhaseEndsAtMs.HasValue ? state.PhaseEndsAtMs.Value - state.PhaseStartedAtMs : 0;
                float read = span > 0 ? (float)(now - state.PhaseStartedAtMs) / span : 1f;
                _card.SetFuse(1f - Mathf.Clamp01(read));
                return;
            }

            if (_mode != GamePhase.Answer) return;
            long left = Math.Max(0, AnswerClock.LatestOpenDeadline(state, now) - now);
            bool lastCall = left <= (long)(game.Config.Timers.LastCall * 1000);
            _status.SetClock(_answerSpanMs > 0 ? (float)left / _answerSpanMs : 0f, (left + 999) / 1000, lastCall);
        }

        /// <summary>Kept for TvPresenter: the layout is the same with the 3D studio on or off.</summary>
        public void SetStudioMode(bool on)
        {
        }

        private void ShowFront(QuestionInPlay question)
        {
            if (question == null)
            {
                _card.ShowFront(string.Empty, null, null, null, null);
                return;
            }

            var state = _context.Game.State;
            string multiplier = question.Multiplier > 1.0001
                ? _context.Format("tv.question.multiplier", "multiplier", NumberText.Format(question.Multiplier))
                : null;
            _card.ShowFront(question.Def.Text.Get(_context.Lang), TopicTitle(question.Def.Topic),
                _context.Format("tv.question.coin", "number", (question.Slot + 1).ToString(), "total", state.QuestionCount.ToString()),
                _context.T("tv.question.coinLabel"), multiplier);
        }

        private string TopicTitle(string topicId)
        {
            foreach (var t in _context.Game.Content.Topics)
            {
                if (t.Id == topicId) return t.Title.Get(_context.Lang);
            }

            return null;
        }

        private void ApplyReveal(int correct)
        {
            bool closest = IsClosestValue();
            var state = _context.Game.State;
            if (!closest)
            {
                for (int i = 0; i < OptionCount; i++) _tiles[i].Reveal(i == correct);
                if (correct >= 0 && correct < OptionCount) PlaceStamp(_tiles[correct], !_revealApplied);
            }

            if (_revealApplied) return;
            _revealApplied = true;
            for (int i = 0; i < state.Players.Count && i < _status.Tokens.Count; i++) _status.Tokens[i].SetLocked(false);
            SetStickers(state, true);
            if (closest)
            {
                _valueLine.Show(state, true);
                return;
            }

            var fact = state.Question?.Def.Fact;
            string factText = fact != null ? fact.Get(_context.Lang) : null;
            if (!string.IsNullOrEmpty(factText)) _card.FlipToBack(_context.T("tv.reveal.why"), factText, FlipDelaySeconds);
        }

        /// <summary>
        /// Each player's pick and points as a sticker under their status token: "A +970" on the answer's ink for a
        /// multiple choice pick ("B Wrong" on the faded ink), the points alone for a closest guess, "No answer" on
        /// cream. The tiles stay clear for their text.
        /// </summary>
        private void SetStickers(GameState state, bool animate)
        {
            const string letters = "ABCD";
            for (int i = 0; i < state.Players.Count && i < _status.Tokens.Count; i++)
            {
                var turn = state.Players[i].Turn;
                string text;
                Color fill;
                int choice = turn != null && turn.Locked && turn.Choice.HasValue ? turn.Choice.Value : -1;
                string letter = choice >= 0 && choice < letters.Length ? letters[choice] + " " : string.Empty;
                if (turn == null || !turn.Locked)
                {
                    text = _context.T("tv.reveal.noAnswer");
                    fill = TvColors.Cream2;
                }
                else if (turn.Correct || turn.Points > 0) // a closest guess scores its bonus even when it is not "correct"
                {
                    text = letter + _context.Format("tv.reveal.points", "points", turn.Points.ToString());
                    fill = choice >= 0 ? TvColors.Answer(choice) : TvColors.Mustard;
                }
                else
                {
                    text = letter + _context.T("tv.reveal.wrong");
                    fill = choice >= 0 ? TvColors.Faded(TvColors.Answer(choice)) : TvColors.Cream2;
                }

                _status.Tokens[i].SetSticker(text, fill, animate, 0.5f + 0.06f * i);
            }
        }

        /// <summary>The rubber stamp on the right tile's right end, slammed down once.</summary>
        private void PlaceStamp(AnswerTile tile, bool slam)
        {
            _stamp.SetParent(tile.Body, false);
            _stamp.anchorMin = _stamp.anchorMax = new Vector2(1f, 0.5f);
            _stamp.pivot = new Vector2(1f, 0.5f);
            _stamp.anchoredPosition = new Vector2(-14f, 0f);
            _stamp.sizeDelta = new Vector2(190f, 56f);
            _stamp.SetAsLastSibling();
            _stampText.text = _context.T("tv.reveal.stamp");
            _stamp.gameObject.SetActive(true);
            if (slam) TvTween.On(_stamp).Slam(0.15f);
        }

        private bool IsClosestValue()
        {
            var question = _context.Game.State.Question;
            return question != null && question.IsClosestValue;
        }

        private void Build()
        {
            var root = transform;
            _valueLine = ValueLine.Create(root, _context);
            _valueLine.gameObject.SetActive(false);

            _tilesRoot = TvLayout.Put(UiBuilder.Rect("Options", root), TvLayout.Tiles);
            for (int i = 0; i < OptionCount; i++)
            {
                _tiles[i] = AnswerTile.Create(_tilesRoot, i);
                int column = i % 2;
                int row = i / 2; // 0 is the top row: A B over C D
                float minX = column * 0.5f;
                float minY = row == 0 ? 0.5f : 0f;
                var tile = UiBuilder.Anchor(_tiles[i].Rect, minX, minY, minX + 0.5f, minY + 0.5f);
                tile.offsetMin = new Vector2(column == 1 ? TileGapX / 2 : 0, row == 0 ? TileGapY / 2 : 0);
                tile.offsetMax = new Vector2(column == 0 ? -TileGapX / 2 : 0, row == 1 ? -TileGapY / 2 : 0);
            }

            _card = QuestionCard.Create(root);
            TvLayout.Put(_card.Rect, TvLayout.ShowCard);

            // The stamp: red ink frame and word on a cream wash, a little turned; moved onto the right tile on Reveal.
            _stamp = UiBuilder.Rect("Stamp", root);
            _stamp.localRotation = Quaternion.Euler(0, 0, 7f);
            TvTween.On(_stamp).Tilt = 7f;
            var wash = UiBuilder.Image("Wash", _stamp, new Color(TvColors.Cream.r, TvColors.Cream.g, TvColors.Cream.b, 0.9f), TvSprites.Solid);
            wash.pixelsPerUnitMultiplier = 2.5f;
            UiBuilder.Anchor(wash.rectTransform, 0, 0, 1, 1);
            var frame = UiBuilder.Image("Frame", _stamp, TvColors.Red, TvSprites.Frame);
            frame.pixelsPerUnitMultiplier = 1.6f;
            UiBuilder.Anchor(frame.rectTransform, 0, 0, 1, 1);
            _stampText = UiBuilder.Display("Word", _stamp, 34, TvColors.Red, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stampText, 18, 34, 1);
            _stampText.characterSpacing = 6f;
            UiBuilder.Anchor(_stampText.rectTransform, 0.06f, 0.1f, 0.94f, 0.9f);
            _stamp.gameObject.SetActive(false);

            _status = StatusStrip.Create(root, _context);
            _hits = HitList.Create(root, _context);
        }
    }
}
