using System;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The Poll (docs/02_show_flow.md): "Thanks! Fill in the poll on your phone", the count of submitted polls of the
    /// connected players and a bar that drains over poll.maxSeconds. Reads GameState; changes nothing.
    /// </summary>
    public sealed class PollView : MonoBehaviour
    {
        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _count;
        private TimerBar _bar;

        public static PollView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("PollView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<PollView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            Refresh();
            Tick();
        }

        /// <summary>A poll submitted, a connection changed or the language changed.</summary>
        public void Refresh()
        {
            _title.text = _context.T("tv.poll.title");
            PollCount.Count(_context.Game.State, out int done, out int total);
            _count.text = _context.Format("tv.poll.count", "done", done.ToString(), "total", total.ToString());
        }

        /// <summary>Every frame: the bar drains with the core clock to the end of the poll.</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            if (!state.PhaseEndsAtMs.HasValue)
            {
                _bar.SetVisible(false);
                return;
            }

            _bar.SetVisible(true);
            long end = state.PhaseEndsAtMs.Value;
            long left = Math.Max(0, end - game.NowMs);
            long span = end - state.PhaseStartedAtMs;
            if (span <= 0) span = (long)(game.Config.Poll.MaxSeconds * 1000);
            _bar.Set(span > 0 ? (float)left / span : 0f, TvColors.DimText);
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 88, TvColors.Text, true, TextAlignmentOptions.Center);
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 64;
            _title.fontSizeMax = 88;
            UiBuilder.Anchor(_title.rectTransform, 0.06f, 0.5f, 0.94f, 0.78f);

            _count = UiBuilder.Text("Count", transform, 64, TvColors.DimText, true, TextAlignmentOptions.Center);
            _count.textWrappingMode = TextWrappingModes.NoWrap;
            _count.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_count.rectTransform, 0.06f, 0.36f, 0.94f, 0.46f);

            _bar = TimerBar.Create(transform);
            UiBuilder.Anchor(_bar.Root, 0.2f, 0.27f, 0.8f, 0.285f);
        }
    }
}
