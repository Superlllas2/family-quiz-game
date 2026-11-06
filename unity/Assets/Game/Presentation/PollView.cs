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
            _bar.Set(span > 0 ? (float)left / span : 0f, TvColors.Mustard);
        }

        /// <summary>A cream card in the middle under the hosts' zone (decisions/0017_game_night_ui.md).</summary>
        private static readonly Rect CardRect = new Rect(307f, 300f, 1306f, 330f);

        private void Build()
        {
            var zone = TvLayout.Put(UiBuilder.Rect("PollZone", transform), CardRect);
            var card = TvPiece.Card("Card", zone, TvColors.Cream, 22f, 10f).Tilt(-1f);
            UiBuilder.Anchor(card.Root, 0, 0, 1, 1);

            _title = UiBuilder.Display("Title", card.Content, 56, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_title, 32, 56, 2);
            UiBuilder.Anchor(_title.rectTransform, 0.06f, 0.42f, 0.94f, 0.92f);

            _count = UiBuilder.Text("Count", card.Content, 44, TvColors.Ink, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(_count, 24, 44, 1);
            UiBuilder.Anchor(_count.rectTransform, 0.06f, 0.2f, 0.94f, 0.4f);

            _bar = TimerBar.Create(card.Content);
            UiBuilder.Anchor(_bar.Root, 0.2f, 0.07f, 0.8f, 0.12f);
        }
    }
}
