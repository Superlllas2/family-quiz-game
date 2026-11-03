using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Results (docs/02_show_flow.md): the title, the one winner and a line that their prize is on their phone (D87), the
    /// two awards (Most correct and Best comeback, every tied name), every other player with their funny tag in seat
    /// order, never a place (D86), and a draining bar with the seconds until everyone is back in the Lobby (D78).
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _winner;
        private TextMeshProUGUI _prize;
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
            _prize.gameObject.SetActive(winner != null && state.WinnerFact != null);
            if (winner != null)
            {
                _winner.text = _context.Format("tv.results.winner", "name", winner.Name);
                _winner.color = _context.ChipColor(winner.CharacterId);
                _prize.text = _context.T("tv.results.prize");
            }

            var tagged = new List<FamilyQuiz.Core.State.PlayerState>();
            foreach (var p in state.Players)
            {
                if (state.TagsByPlayer.ContainsKey(p.PlayerId)) tagged.Add(p);
            }

            _list.Rebuild(tagged);
            float height = _list.RowHeight();
            for (int i = 0; i < tagged.Count; i++)
            {
                var p = tagged[i];
                var row = _list.Find(p.PlayerId);
                row.SetTag(p.Name, _context.ChipColor(p.CharacterId), p.Connected,
                    _context.T("tv.tags." + state.TagsByPlayer[p.PlayerId]), TvColors.Text);
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
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.9f, 0.95f, 0.97f);

            _winner = UiBuilder.Text("Winner", transform, 104, TvColors.Text, true, TextAlignmentOptions.Center);
            _winner.enableAutoSizing = true;
            _winner.fontSizeMin = 72;
            _winner.fontSizeMax = 104;
            _winner.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_winner.rectTransform, 0.03f, 0.78f, 0.97f, 0.9f);

            _prize = UiBuilder.Text("Prize", transform, 44, TvColors.DimText, false, TextAlignmentOptions.Center);
            _prize.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_prize.rectTransform, 0.05f, 0.72f, 0.95f, 0.78f);

            _award = AwardLine("MostCorrect", 0.64f, 0.71f);
            _comeback = AwardLine("BestComeback", 0.57f, 0.64f);

            // Up to seven tagged players (players.max 8, minus the winner).
            _list = LeaderboardList.Create(transform);
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.13f, 0.95f, 0.555f);

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
