using System;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The final race on the TV (docs/02_show_flow.md): FinalIntro (title, one line of rules, the track with every chip on
    /// its head start), RaceQuestion (statement number, statement, answer countdown, chips that pop on lock in, never
    /// what anyone chose) and RaceReveal (True or False, and the chips of right answers slide one step forward). One view
    /// for the three phases, so the track and its slides carry over. Reads GameState; changes nothing.
    /// </summary>
    public sealed class RaceView : MonoBehaviour
    {
        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _rules;
        private TextMeshProUGUI _number;
        private TextMeshProUGUI _seconds;
        private TextMeshProUGUI _statement;
        private TextMeshProUGUI _answer;
        private TimerBar _bar;
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
            _mode = phase;
            var state = _context.Game.State;
            bool intro = phase == GamePhase.FinalIntro;
            bool question = phase == GamePhase.RaceQuestion;
            bool reveal = phase == GamePhase.RaceReveal;

            _title.gameObject.SetActive(intro);
            _rules.gameObject.SetActive(intro);
            _number.gameObject.SetActive(!intro);
            _statement.gameObject.SetActive(!intro);
            _seconds.gameObject.SetActive(question);
            _bar.SetVisible(question);
            _answer.gameObject.SetActive(reveal);
            _answerSpanMs = question ? AnswerClock.Span(state) : 0;

            Refresh();
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

        /// <summary>Texts (a language change), the track (roster, connection) and the lock rims.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            var state = game.State;
            var race = state.Race;
            var lang = _context.Lang;
            _title.text = _context.T("tv.finalIntro.title");
            _rules.text = _context.T("tv.finalIntro.rules");
            var statement = race?.Statement;
            _number.text = race != null && race.StatementNumber > 0
                ? _context.Format("tv.race.statementNumber", "number", race.StatementNumber.ToString())
                : string.Empty;
            _statement.text = statement != null ? statement.Statement.Get(lang) : string.Empty;
            if (statement != null)
            {
                _answer.text = _context.T(statement.Answer ? "tv.race.true" : "tv.race.false");
                _answer.color = statement.Answer ? TvColors.Correct : TvColors.Error;
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

        /// <summary>Per frame in RaceQuestion: the answer bar and the seconds left from the personal deadlines (no last call in the race).</summary>
        public void Tick()
        {
            if (_mode != GamePhase.RaceQuestion) return;
            var game = _context.Game;
            long now = game.NowMs;
            long left = Math.Max(0, AnswerClock.LatestOpenDeadline(game.State, now) - now);
            _bar.Set(_answerSpanMs > 0 ? (float)left / _answerSpanMs : 0f, TvColors.Text);
            _seconds.text = ((left + 999) / 1000).ToString();
        }

        private void Build()
        {
            var root = transform;
            _title = UiBuilder.Text("Title", root, 110, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.82f, 0.95f, 0.96f);

            _rules = UiBuilder.Text("Rules", root, 56, TvColors.DimText, true, TextAlignmentOptions.Center);
            _rules.enableAutoSizing = true;
            _rules.fontSizeMin = 40;
            _rules.fontSizeMax = 56;
            UiBuilder.Anchor(_rules.rectTransform, 0.06f, 0.63f, 0.94f, 0.8f);

            _number = UiBuilder.Text("Number", root, 44, TvColors.DimText, false, TextAlignmentOptions.Left);
            UiBuilder.Anchor(_number.rectTransform, 0.03f, 0.9f, 0.7f, 0.985f);

            _seconds = UiBuilder.Text("Seconds", root, 96, TvColors.Text, true, TextAlignmentOptions.Right);
            _seconds.textWrappingMode = TextWrappingModes.NoWrap;
            _seconds.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_seconds.rectTransform, 0.8f, 0.885f, 0.97f, 0.995f);

            _statement = UiBuilder.Text("Statement", root, 72, TvColors.Text, true, TextAlignmentOptions.Center);
            // Three lines at most: long statements shrink to 52 before they are cut.
            _statement.enableAutoSizing = true;
            _statement.fontSizeMin = 52;
            _statement.fontSizeMax = 72;
            _statement.maxVisibleLines = 3;
            UiBuilder.Anchor(_statement.rectTransform, 0.04f, 0.68f, 0.96f, 0.89f);

            _bar = TimerBar.Create(root);
            UiBuilder.Anchor(_bar.Root, 0.04f, 0.643f, 0.96f, 0.658f);

            _answer = UiBuilder.Text("Answer", root, 88, TvColors.Correct, true, TextAlignmentOptions.Center);
            _answer.textWrappingMode = TextWrappingModes.NoWrap;
            // The 88 pt line is taller than its box: with the default ellipsis TMP would hide it entirely.
            _answer.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_answer.rectTransform, 0.2f, 0.585f, 0.8f, 0.68f);

            _track = RaceTrack.Create(root, _context);
            UiBuilder.Anchor(_track.Rect, 0.02f, 0.03f, 0.98f, 0.6f);
        }
    }
}
