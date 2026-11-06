using System;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The final race on the TV (docs/02_show_flow.md, decisions/0017_game_night_ui.md): FinalIntro (the title ribbon,
    /// the rules on a card, the track with every chip on its head start), RaceQuestion (the statement on a card with its
    /// number on the ribbon, the clock token, chips that get their check on lock in, never what anyone chose) and
    /// RaceReveal ("True" or "False" stamped on the card, and the chips of right answers slide one step forward). One
    /// view for the three phases, so the track and its slides carry over. Reads GameState; changes nothing.
    /// </summary>
    public sealed class RaceView : MonoBehaviour
    {
        private static readonly Rect CardRect = new Rect(307f, 556f, 1306f, 190f);

        private TvContext _context;
        private TitleRibbon _title;
        private QuestionCard _card;
        private StatusStrip _clock;
        private RectTransform _stamp;
        private TextMeshProUGUI _stampText;
        private UnityEngine.UI.Image _stampFrame;
        private RaceTrack _track;
        private GamePhase _mode;
        private long _answerSpanMs;

        public static RaceView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("RaceView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<RaceView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Called on PhaseStarted for FinalIntro, RaceQuestion and RaceReveal.</summary>
        public void Enter(GamePhase phase)
        {
            var previous = _mode;
            _mode = phase;
            var state = _context.Game.State;
            bool intro = phase == GamePhase.FinalIntro;
            bool question = phase == GamePhase.RaceQuestion;
            bool reveal = phase == GamePhase.RaceReveal;

            _title.SetVisible(intro);
            if (intro) _title.Rise();
            _clock.gameObject.SetActive(question);
            _clock.SetClockVisible(question);
            _stamp.gameObject.SetActive(false);
            _answerSpanMs = question ? AnswerClock.Span(state) : 0;

            Refresh();
            if (intro || question) _card.PopIn(0f);
            if (reveal) Stamp(previous == GamePhase.RaceQuestion);
            var race = state.Race;
            int finish = _context.Game.Config.Race.FinishSteps;
            if (race != null)
            {
                // Each phase starts from the state; only the reveal moves runners, from where they are drawn.
                if (reveal) _track.SlideAll(race, finish, TvMotion.RaceMoveDelaySeconds);
                else _track.PlaceAll(race, finish);
            }

            Tick();
        }

        /// <summary>Texts (a language change), the track (roster, connection) and the lock checks.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            var state = game.State;
            var race = state.Race;
            var lang = _context.Lang;
            _title.Set(_context.T("tv.finalIntro.title"), null);
            if (_mode == GamePhase.FinalIntro)
            {
                _card.ShowFront(_context.T("tv.finalIntro.rules"), null, null, null, null);
            }
            else
            {
                var statement = race?.Statement;
                string number = race != null && race.StatementNumber > 0
                    ? _context.Format("tv.race.statementNumber", "number", race.StatementNumber.ToString())
                    : null;
                _card.ShowFront(statement != null ? statement.Statement.Get(lang) : string.Empty, number, null, null, null);
                if (statement != null) _stampText.text = _context.T(statement.Answer ? "tv.race.true" : "tv.race.false");
            }

            _track.gameObject.SetActive(race != null);
            if (race == null) return;
            _track.Sync(state, game.Config.Race.FinishSteps);
            foreach (var runner in _track.Runners)
            {
                var player = state.FindById(runner.PlayerId);
                runner.Chip.SetLocked(_mode == GamePhase.RaceQuestion && player?.Turn != null && player.Turn.Locked);
            }
        }

        public void OnAnswerLocked(string playerId)
        {
            if (_mode != GamePhase.RaceQuestion) return;
            var runner = _track.Find(playerId);
            if (runner == null) return;
            runner.Chip.SetLocked(true);
            runner.Chip.Pop();
        }

        /// <summary>RaceRevealed follows PhaseStarted(RaceReveal): the slides already started; this only makes sure.</summary>
        public void OnRaceRevealed()
        {
            var race = _context.Game.State.Race;
            if (_mode == GamePhase.RaceReveal && race != null)
                _track.SlideAll(race, _context.Game.Config.Race.FinishSteps, TvMotion.RaceMoveDelaySeconds);
        }

        /// <summary>Per frame in RaceQuestion: the clock from the personal deadlines (no last call in the race).</summary>
        public void Tick()
        {
            if (_mode != GamePhase.RaceQuestion) return;
            var game = _context.Game;
            long now = game.NowMs;
            long left = Math.Max(0, AnswerClock.LatestOpenDeadline(game.State, now) - now);
            _clock.SetClock(_answerSpanMs > 0 ? (float)left / _answerSpanMs : 0f, (left + 999) / 1000, false);
        }

        /// <summary>"True" in green or "False" in red, stamped on the card's right end.</summary>
        private void Stamp(bool slam)
        {
            var statement = _context.Game.State.Race?.Statement;
            if (statement == null) return;
            var color = statement.Answer ? TvColors.Green : TvColors.Red;
            _stampText.color = color;
            _stampFrame.color = color;
            _stamp.gameObject.SetActive(true);
            if (slam) TvTween.On(_stamp).Slam(0.05f);
        }

        private void Build()
        {
            var root = transform;
            _track = RaceTrack.Create(root, _context);
            UiBuilder.Anchor(_track.Rect, 0.02f, 0.03f, 0.98f, 0.49f);

            _card = QuestionCard.Create(root);
            TvLayout.Put(_card.Rect, CardRect);
            _card.SetLarge(false);

            _stamp = UiBuilder.Rect("Stamp", _card.transform);
            _stamp.anchorMin = _stamp.anchorMax = new Vector2(1f, 0.5f);
            _stamp.pivot = new Vector2(0.5f, 0.5f);
            _stamp.anchoredPosition = new Vector2(-60f, 0f);
            _stamp.sizeDelta = new Vector2(250f, 70f);
            _stamp.localRotation = Quaternion.Euler(0, 0, 8f);
            TvTween.On(_stamp).Tilt = 8f;
            var wash = UiBuilder.Image("Wash", _stamp, new Color(TvColors.Cream.r, TvColors.Cream.g, TvColors.Cream.b, 0.92f), TvSprites.Solid);
            wash.pixelsPerUnitMultiplier = 2.5f;
            UiBuilder.Anchor(wash.rectTransform, 0, 0, 1, 1);
            _stampFrame = UiBuilder.Image("Frame", _stamp, TvColors.Red, TvSprites.Frame);
            _stampFrame.pixelsPerUnitMultiplier = 1.6f;
            UiBuilder.Anchor(_stampFrame.rectTransform, 0, 0, 1, 1);
            _stampText = UiBuilder.Display("Word", _stamp, 36, TvColors.Red, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stampText, 18, 36, 1);
            _stampText.characterSpacing = 4f;
            UiBuilder.Anchor(_stampText.rectTransform, 0.06f, 0.1f, 0.94f, 0.9f);
            _stamp.gameObject.SetActive(false);

            _title = TitleRibbon.Create(root, TvColors.Red);
            _clock = StatusStrip.Create(root, _context);
        }
    }
}
