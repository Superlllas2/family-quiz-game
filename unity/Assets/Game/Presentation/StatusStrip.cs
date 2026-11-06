using System.Collections.Generic;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The top right corner of the TV during a vote and a question (decisions/0017_game_night_ui.md): a clock token
    /// whose sweep runs down with the timer (red and wiggling in the last call), and one token per seated player that
    /// gets its check badge when they voted or locked in, and a points sticker on Reveal. It owns the TvLayout status
    /// zone; nothing else is drawn there.
    /// </summary>
    public sealed class StatusStrip : MonoBehaviour
    {
        public const float TokenSize = 76f;
        public const float TokenGap = 18f;
        public const float ClockSize = 116f;

        private readonly List<PlayerToken> _tokens = new List<PlayerToken>();
        private TvContext _context;
        private RectTransform _row;
        private TvPiece _clock;
        private Image _sweep;
        private TextMeshProUGUI _seconds;
        private TvTween _clockTween;

        public IReadOnlyList<PlayerToken> Tokens => _tokens;

        public static StatusStrip Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Rect("Status", parent);
            TvLayout.PlaceStatus(rect);
            var strip = rect.gameObject.AddComponent<StatusStrip>();
            strip._context = context;
            strip.Build();
            return strip;
        }

        public PlayerToken Find(string playerId)
        {
            foreach (var t in _tokens) if (t.PlayerId == playerId) return t;
            return null;
        }

        /// <summary>One token per seated player, right aligned before the clock, with letters, colours and connection.</summary>
        public void Sync(GameState state)
        {
            bool same = state.Players.Count == _tokens.Count;
            for (int i = 0; same && i < _tokens.Count; i++) same = state.Players[i].PlayerId == _tokens[i].PlayerId;
            if (!same)
            {
                foreach (var t in _tokens) Destroy(t.gameObject);
                _tokens.Clear();
                foreach (var p in state.Players) _tokens.Add(PlayerToken.Create(_row, p.PlayerId));
            }

            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);
            int n = _tokens.Count;
            for (int i = 0; i < n; i++)
            {
                var p = state.Players[i];
                var rect = _tokens[i].Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                // Join order from left to right, ending just before the clock.
                rect.anchoredPosition = new Vector2(-(n - 1 - i) * (TokenSize + TokenGap), -6f);
                rect.sizeDelta = new Vector2(TokenSize, TokenSize);
                _tokens[i].Set(labels[i], _context.ChipColor(p.CharacterId), p.Connected);
            }
        }

        public void ClearMarks()
        {
            foreach (var t in _tokens)
            {
                t.SetLocked(false);
                t.SetSticker(null, TvColors.Cream);
            }
        }

        public void SetClockVisible(bool visible)
        {
            _clock.Root.gameObject.SetActive(visible);
            if (!visible) _clockTween.SetWiggle(false);
        }

        /// <summary>The sweep from 1 (full) to 0, the whole seconds left and the last call look.</summary>
        public void SetClock(float fraction, long seconds, bool lastCall)
        {
            _sweep.fillAmount = Mathf.Clamp01(fraction);
            _sweep.color = lastCall ? TvColors.Red : TvColors.Mustard;
            _seconds.text = seconds.ToString();
            _clockTween.SetWiggle(lastCall && seconds > 0);
        }

        private void Build()
        {
            _clock = TvPiece.Disc("Clock", transform, TvColors.Cream, 6f);
            var clock = _clock.Root;
            clock.anchorMin = clock.anchorMax = new Vector2(1f, 1f);
            clock.pivot = new Vector2(1f, 1f);
            clock.anchoredPosition = Vector2.zero;
            clock.sizeDelta = new Vector2(ClockSize, ClockSize);
            _clockTween = TvTween.On(clock);

            _sweep = UiBuilder.Image("Sweep", _clock.Content, TvColors.Mustard);
            _sweep.sprite = TvSprites.DiscSolid;
            _sweep.type = Image.Type.Filled;
            _sweep.fillMethod = Image.FillMethod.Radial360;
            _sweep.fillOrigin = (int)Image.Origin360.Top;
            _sweep.fillClockwise = false;
            UiBuilder.Anchor(_sweep.rectTransform, 0.13f, 0.13f, 0.87f, 0.87f);

            var face = UiBuilder.Image("Face", _clock.Content, TvColors.Cream);
            face.sprite = TvSprites.DiscSolid;
            UiBuilder.Anchor(face.rectTransform, 0.28f, 0.28f, 0.72f, 0.72f);

            _seconds = UiBuilder.Display("Seconds", _clock.Content, 34, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_seconds, 20, 34, 1);
            UiBuilder.Anchor(_seconds.rectTransform, 0.2f, 0.2f, 0.8f, 0.8f);

            _row = UiBuilder.Rect("Tokens", transform);
            _row.anchorMin = new Vector2(0f, 0f);
            _row.anchorMax = new Vector2(1f, 1f);
            _row.offsetMin = Vector2.zero;
            _row.offsetMax = new Vector2(-(ClockSize + TokenGap), 0f);
        }
    }
}
