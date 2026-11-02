using System;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>StartCountdown: a big 3, 2, 1 from the core clock (ceil of the seconds left) under "Starting".</summary>
    public sealed class CountdownView : MonoBehaviour
    {
        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _number;
        private long _shown = -1;
        private float _popAge = -1f;

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
            _title.text = _context.T("tv.countdown.title");
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
            _popAge = 0f;
        }

        private void Update()
        {
            if (_popAge < 0f) return;
            _popAge += Time.unscaledDeltaTime;
            float t = _popAge / TvMotion.PopSeconds;
            float scale = t >= 1f ? 1f : 1f + TvMotion.PopScale * Mathf.Sin(Mathf.PI * t);
            _number.rectTransform.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f) _popAge = -1f;
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 88, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.72f, 0.95f, 0.86f);
            _number = UiBuilder.Text("Number", transform, 420, TvColors.Text, true, TextAlignmentOptions.Center);
            _number.textWrappingMode = TextWrappingModes.NoWrap;
            _number.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Anchor(_number.rectTransform, 0.2f, 0.15f, 0.8f, 0.7f);
        }
    }
}
