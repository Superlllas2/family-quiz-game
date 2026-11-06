using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Results (docs/02_show_flow.md): the title, the one winner and a line that their prize is on their phone (D87), the
    /// two awards (Most correct and Best comeback, every tied name), every other player with their funny tag in seat
    /// order, never a place (D86), and a draining bar with the seconds until the Poll, or with the poll off until everyone
    /// is back in the Lobby (D78).
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TitleRibbon _ribbon;
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
            _bar.Set(span > 0 ? (float)left / span : 0f, TvColors.Mustard);

            long seconds = Math.Max(1, (left + 999) / 1000);
            if (seconds == _shownSeconds) return;
            _shownSeconds = seconds;
            _lobbyIn.text = _context.Format(CountdownKey(), "seconds", seconds.ToString());
        }

        /// <summary>"Poll in N" when the Poll follows, else "Back to the lobby in N" (D78).</summary>
        private string CountdownKey()
        {
            var game = _context.Game;
            return ResultsCountdown.Key(game.Config.Poll.Enabled, game.Content.Poll.Count);
        }

        public void Refresh()
        {
            var state = _context.Game.State;
            _shownSeconds = -1; // the countdown line is redrawn on the next Tick, in the language now set
            _title.text = _context.T("tv.results.title");
            _ribbon.Set(_title.text, null);

            var winner = state.WinnerId != null ? state.FindById(state.WinnerId) : null;
            _winner.gameObject.SetActive(winner != null);
            _prize.gameObject.SetActive(winner != null && state.WinnerFact != null);
            if (winner != null)
            {
                _winner.text = _context.Format("tv.results.winner", "name", winner.Name);
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

        /// <summary>The winner's card: the winner, the prize line and the two awards (decisions/0017_game_night_ui.md).</summary>
        private static readonly Rect WinnerCard = new Rect(307f, 520f, 1306f, 250f);

        /// <summary>Up to seven tagged players (players.max 8, minus the winner), under the winner's card.</summary>
        private static readonly Rect TagList = new Rect(TvLayout.Margin, 110f, TvLayout.Width - 2 * TvLayout.Margin, 390f);

        private void Build()
        {
            _ribbon = TitleRibbon.Create(transform, TvColors.Mustard);
            // Kept for Refresh, drawn by the ribbon.
            _title = UiBuilder.Text("TitleText", transform, 10, TvColors.Text, false, TextAlignmentOptions.Center);
            _title.gameObject.SetActive(false);

            var zone = TvLayout.Put(UiBuilder.Rect("WinnerZone", transform), WinnerCard);
            var card = TvPiece.Card("Card", zone, TvColors.Cream, 22f, 10f).Tilt(-1f);
            UiBuilder.Anchor(card.Root, 0, 0, 1, 1);
            var star = TvPiece.Star("Star", zone, TvColors.Mustard);
            var s = star.Root;
            s.anchorMin = s.anchorMax = new Vector2(0f, 1f);
            s.pivot = new Vector2(0.5f, 0.5f);
            s.anchoredPosition = new Vector2(20f, -20f);
            s.sizeDelta = new Vector2(120f, 120f);
            var one = UiBuilder.Display("One", star.Content, 48, TvColors.Ink, TextAlignmentOptions.Center);
            one.text = "1";
            UiBuilder.Anchor(one.rectTransform, 0, 0, 1, 1);

            _winner = UiBuilder.Display("Winner", card.Content, 64, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(_winner, 36, 64, 1);
            UiBuilder.Anchor(_winner.rectTransform, 0.08f, 0.64f, 0.92f, 0.94f);

            _prize = UiBuilder.Hand("Prize", card.Content, 36, TvColors.Teal, TextAlignmentOptions.Center);
            UiBuilder.Fit(_prize, 22, 36, 1);
            UiBuilder.Anchor(_prize.rectTransform, 0.08f, 0.47f, 0.92f, 0.64f);

            _award = AwardLine("MostCorrect", card.Content, 0.25f, 0.46f);
            _comeback = AwardLine("BestComeback", card.Content, 0.05f, 0.25f);

            _list = LeaderboardList.Create(transform);
            TvLayout.Put(_list.Rect, TagList);

            _lobbyIn = UiBuilder.Text("LobbyIn", transform, 30, TvColors.Text, true, TextAlignmentOptions.Center);
            _lobbyIn.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_lobbyIn.rectTransform, 0.2f, 0.045f, 0.8f, 0.095f);

            _bar = TimerBar.Create(transform);
            UiBuilder.Anchor(_bar.Root, 0.3f, 0.025f, 0.7f, 0.037f);
        }

        private TextMeshProUGUI AwardLine(string name, Transform parent, float minY, float maxY)
        {
            var line = UiBuilder.Text(name, parent, 34, TvColors.Ink, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(line, 22, 34, 1);
            UiBuilder.Anchor(line.rectTransform, 0.08f, minY, 0.92f, maxY);
            return line;
        }
    }
}
