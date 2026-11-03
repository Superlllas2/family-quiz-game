using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FamilyQuiz.DevTools
{
    /// <summary>
    /// F1 debug overlay for developers (not player facing, so plain English): phase, phaseId, budget, time left, the
    /// latest personal deadline in Answer, players connected and locked in, and the live idle share of the current
    /// game with its longest idle stretch outside Intro, FinalIntro and Results. A listener only: reads Game and events, changes nothing.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private const int SortingOrder = 1000;
        private const float FontSize = 26f;

        private readonly IdleMeter _idle = new IdleMeter();
        private readonly StringBuilder _text = new StringBuilder();
        private Game _game;
        private GameObject _panel;
        private TextMeshProUGUI _label;

        /// <summary>Builds the overlay, hidden until F1.</summary>
        public static DebugOverlay Create(Game game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            var go = new GameObject("DebugOverlay");
            var overlay = go.AddComponent<DebugOverlay>();
            overlay._game = game;
            overlay.Build();
            return overlay;
        }

        public bool Visible => _panel.activeSelf;

        /// <summary>Hand every batch of events Core returns, like the other listeners.</summary>
        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (events == null) return;
            foreach (var e in events)
            {
                switch (e)
                {
                    case GameStarted _:
                        // From the scheduled end of the countdown, so the first frame's lag counts as game time.
                        _idle.Start(_game.State.GameStartedAtMs, Idle(), CountsForStretch());
                        break;
                    case GameEnded ended:
                        long end = ended.DurationMs > 0 ? _game.State.GameStartedAtMs + ended.DurationMs : _game.NowMs;
                        _idle.Stop(end);
                        break;
                }
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame) _panel.SetActive(!_panel.activeSelf);
            _idle.Sample(_game.NowMs, Idle(), CountsForStretch());
            if (_panel.activeSelf) _label.SetText(Describe());
        }

        private bool Idle()
        {
            return !_game.AnyoneCanAnswer;
        }

        private bool CountsForStretch()
        {
            // The longest idle stretch leaves out Intro, FinalIntro and Results (docs/19_acceptance.md, Day 3).
            var phase = _game.State.Phase;
            return phase != GamePhase.Results && phase != GamePhase.Intro && phase != GamePhase.FinalIntro;
        }

        private StringBuilder Describe()
        {
            var state = _game.State;
            long now = _game.NowMs;
            _text.Clear();
            _text.Append("<b>DEBUG (F1)</b>\n");
            _text.Append("phase  ").Append(state.Phase).Append("   phaseId ").Append(state.PhaseId).Append('\n');
            _text.Append("budget ").Append(Seconds(state.PhaseBudgetMs));
            if (state.PhaseStagingExtraMs != 0) _text.Append(" (staging +").Append(Seconds(state.PhaseStagingExtraMs)).Append(')');
            _text.Append('\n');
            _text.Append("left   ").Append(state.PhaseEndsAtMs.HasValue ? Seconds(Math.Max(0, state.PhaseEndsAtMs.Value - now)) : "no timer").Append('\n');

            if (state.Phase == GamePhase.Answer || state.Phase == GamePhase.RaceQuestion)
            {
                long latest = long.MinValue;
                foreach (var p in state.Players)
                {
                    if (p.Turn != null) latest = Math.Max(latest, p.Turn.DeadlineMs);
                }

                _text.Append("latest deadline ");
                _text.Append(latest == long.MinValue ? "none" : "in " + Seconds(Math.Max(0, latest - now)));
                _text.Append(" (+grace ").Append(Seconds(_game.Config.Net.InputGraceMs)).Append(")\n");
            }

            int connected = 0;
            int locked = 0;
            int turns = 0;
            foreach (var p in state.Players)
            {
                if (p.Connected) connected++;
                if (p.Turn == null) continue;
                turns++;
                if (p.Turn.Locked) locked++;
            }

            _text.Append("players ").Append(connected).Append('/').Append(state.Players.Count).Append(" connected");
            if (turns > 0) _text.Append(", ").Append(locked).Append('/').Append(turns).Append(" locked");
            _text.Append('\n');

            _text.Append("game #").Append(state.GameNumber).Append("  seed ").Append(state.Seed);
            _text.Append(_idle.Running ? "  running" : _idle.Finished ? "  ended" : "  not started").Append('\n');
            _text.Append("idle   ").Append((_idle.IdleShare * 100).ToString("0.0", CultureInfo.InvariantCulture)).Append("% (")
                .Append(Seconds(_idle.IdleMs)).Append(" of ").Append(Seconds(_idle.GameMs)).Append(")\n");
            _text.Append("longest idle stretch ").Append(Seconds(_idle.LongestStretchMs));
            if (_idle.Running && _idle.CurrentStretchMs > 0) _text.Append("  (now ").Append(Seconds(_idle.CurrentStretchMs)).Append(')');
            return _text;
        }

        private static string Seconds(long ms)
        {
            return (ms / 1000d).ToString("0.0", CultureInfo.InvariantCulture) + " s";
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = UiBuilder.Image("Panel", transform, new Color(0f, 0f, 0f, 0.8f));
            var rect = panel.rectTransform;
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-16, -16);
            rect.sizeDelta = new Vector2(640, 330);
            _panel = panel.gameObject;

            _label = UiBuilder.Text("Text", rect, FontSize, Color.white, false, TMPro.TextAlignmentOptions.TopLeft);
            _label.overflowMode = TextOverflowModes.Overflow;
            _label.richText = true;
            UiBuilder.Anchor(_label.rectTransform, 0, 0, 1, 1);
            _label.rectTransform.offsetMin = new Vector2(18, 14);
            _label.rectTransform.offsetMax = new Vector2(-18, -14);

            _panel.SetActive(false);
        }
    }
}
