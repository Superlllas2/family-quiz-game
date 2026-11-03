using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Results (docs/02_show_flow.md): the title, the winner, the top 3 from the final order (GameState.Placement: the
    /// race order with its tie breaks, so places are never shared) with their score and correct answers, the two awards
    /// (Most correct and Best comeback, every tied name), and a draining bar with the seconds until everyone is back in
    /// the Lobby for the next game (D78), so the jump never surprises anyone.
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const int TopPlaces = 3;
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _winner;
        private LeaderboardList _list;
        private TextMeshProUGUI _award;
        private TextMeshProUGUI _comeback;
        private TextMeshProUGUI _lobbyIn;
        private TimerBar _bar;
        private long _shownSeconds = -1;

        public static ResultsView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("ResultsView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<ResultsView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            _shownSeconds = -1;
            Refresh();
            Tick();
        }

        /// <summary>Every frame: the bar drains with the core clock; the text changes once a second (ceil, never 0).</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            if (!state.PhaseEndsAtMs.HasValue)
            {
                _bar.SetVisible(false);
                _lobbyIn.gameObject.SetActive(false);
                return;
            }

            _bar.SetVisible(true);
            _lobbyIn.gameObject.SetActive(true);
            long end = state.PhaseEndsAtMs.Value;
            long left = Math.Max(0, end - game.NowMs);
            long span = end - state.PhaseStartedAtMs;
            _bar.Set(span > 0 ? (float)left / span : 0f, TvColors.DimText);

            long seconds = Math.Max(1, (left + 999) / 1000);
            if (seconds == _shownSeconds) return;
            _shownSeconds = seconds;
            _lobbyIn.text = _context.Format("tv.results.lobbyIn", "seconds", seconds.ToString());
        }

        public void Refresh()
        {
            var state = _context.Game.State;
            _title.text = _context.T("tv.results.title");

            var winner = state.WinnerId != null ? state.FindById(state.WinnerId) : null;
            _winner.gameObject.SetActive(winner != null);
            if (winner != null)
            {
                _winner.text = _context.Format("tv.results.winner", "name", winner.Name);
                _winner.color = _context.ChipColor(winner.CharacterId);
            }

            var top = Standings.Top(state, TopPlaces);
            _list.Rebuild(top);
            float height = _list.RowHeight();
            for (int i = 0; i < top.Count; i++)
            {
                var p = top[i];
                var row = _list.Find(p.PlayerId);
                row.Set(i + 1, p.Name, _context.ChipColor(p.CharacterId), p.Connected,
                    _context.Format("tv.results.correctCount", "count", p.CorrectCount.ToString()), TvColors.DimText);
                row.SetScore(p.Score);
                row.Place(_list.Y(i), height);
            }

            ShowAward(_award, "tv.awards.mostCorrect", state.MostCorrectIds);
            ShowAward(_comeback, "tv.awards.bestComeback", state.BestComebackIds);
        }

        /// <summary>An award line with every tied name; hidden when nobody earned it.</summary>
        private void ShowAward(TextMeshProUGUI line, string key, IReadOnlyList<string> ids)
        {
            var state = _context.Game.State;
            var names = new List<string>();
            foreach (var id in ids)
            {
                var p = state.FindById(id);
                if (p != null) names.Add(p.Name);
            }

            line.gameObject.SetActive(names.Count > 0);
            line.text = names.Count > 0 ? _context.Format(key, "names", string.Join(NameSeparator, names)) : string.Empty;
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 64, TvColors.DimText, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.88f, 0.95f, 0.97f);

            _winner = UiBuilder.Text("Winner", transform, 104, TvColors.Text, true, TextAlignmentOptions.Center);
            _winner.enableAutoSizing = true;
            _winner.fontSizeMin = 72;
            _winner.fontSizeMax = 104;
            _winner.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_winner.rectTransform, 0.03f, 0.74f, 0.97f, 0.87f);

            _list = LeaderboardList.Create(transform);
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.4f, 0.95f, 0.72f);

            _award = AwardLine("MostCorrect", 0.28f, 0.37f);
            _comeback = AwardLine("BestComeback", 0.18f, 0.27f);

            _lobbyIn = UiBuilder.Text("LobbyIn", transform, 44, TvColors.DimText, false, TextAlignmentOptions.Center);
            _lobbyIn.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_lobbyIn.rectTransform, 0.05f, 0.055f, 0.95f, 0.115f);

            _bar = TimerBar.Create(transform);
            UiBuilder.Anchor(_bar.Root, 0.2f, 0.03f, 0.8f, 0.042f);
        }

        private TextMeshProUGUI AwardLine(string name, float minY, float maxY)
        {
            var line = UiBuilder.Text(name, transform, 52, TvColors.Text, true, TextAlignmentOptions.Center);
            line.enableAutoSizing = true;
            line.fontSizeMin = 40;
            line.fontSizeMax = 52;
            UiBuilder.Anchor(line.rectTransform, 0.04f, minY, 0.96f, maxY);
            return line;
        }
    }
}
