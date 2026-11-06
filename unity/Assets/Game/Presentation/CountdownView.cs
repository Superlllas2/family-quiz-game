using System;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// StartCountdown (decisions/0017_game_night_ui.md): "Starting" on the title ribbon and a big 3, 2, 1 on a mustard
    /// coin on the floor under the seats, from the core clock (ceil of the seconds left); the coin bounces on each number.
    /// </summary>
    public sealed class CountdownView : MonoBehaviour
    {
        private TvContext _context;
        private TitleRibbon _title;
        private TvPiece _coin;
        private TextMeshProUGUI _number;
        private long _shown = -1;

        public static CountdownView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("CountdownView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<CountdownView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            _title.Set(_context.T("tv.countdown.title"), null);
            _shown = -1;
            Tick();
        }

        public void Tick()
        {
            var game = _context.Game;
            var end = game.State.PhaseEndsAtMs;
            long left = end.HasValue ? Math.Max(0, end.Value - game.NowMs) : 0;
            // Ceil, never below 1 while the countdown runs: 3, 2, 1, then the next phase.
            long seconds = Math.Max(1, (left + 999) / 1000);
            if (seconds == _shown) return;
            _shown = seconds;
            _number.text = seconds.ToString();
            TvTween.On(_coin.Root).Pop(0f, 0.35f);
        }

        private void Build()
        {
            _title = TitleRibbon.Create(transform, TvColors.Teal);
            _coin = TvPiece.Disc("Coin", transform, TvColors.Mustard, 10f);
            UiBuilder.Place(_coin.Root, 0.5f, 0.2f, 300f, 300f);
            _number = UiBuilder.Display("Number", _coin.Content, 170, TvColors.Ink, TextAlignmentOptions.Center);
            _number.textWrappingMode = TextWrappingModes.NoWrap;
            _number.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_number.rectTransform, 0, 0, 1, 1);
        }
    }
}
