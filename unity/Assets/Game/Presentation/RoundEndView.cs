using System.Collections.Generic;
using FamilyQuiz.Core.Rules;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// RoundEnd (docs/02_show_flow.md): "Round N is over", the round winner or winners (the most points this round, all
    /// tied names; nobody when nobody scored), and the leaderboard by total score with each player's points this round.
    /// </summary>
    public sealed class RoundEndView : MonoBehaviour
    {
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _winner;
        private LeaderboardList _list;

        public static RoundEndView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("RoundEndView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<RoundEndView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            Refresh();
        }

        /// <summary>Everything from the state: texts, the winners and the rows at their places.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var round = state.Round;
            _title.text = round != null ? _context.Format("tv.roundEnd.title", "round", (round.Index + 1).ToString()) : string.Empty;

            var names = new List<string>();
            foreach (var p in Standings.RoundWinners(state)) names.Add(p.Name);
            _winner.gameObject.SetActive(names.Count > 0);
            _winner.text = names.Count > 0 ? _context.Format("tv.roundEnd.winner", "names", string.Join(NameSeparator, names)) : string.Empty;

            var order = Standings.ByScore(state);
            if (!_list.Matches(order)) _list.Rebuild(order);
            float height = _list.RowHeight();
            for (int i = 0; i < order.Count; i++)
            {
                var p = order[i];
                var row = _list.Find(p.PlayerId);
                int points = Standings.RoundPoints(state, p);
                string extra = points > 0 ? _context.Format("tv.reveal.points", "points", points.ToString()) : string.Empty;
                row.Set(Ranking.SharedRank(state, p), p.Name, _context.ChipColor(p.CharacterId), p.Connected, extra, TvColors.Correct);
                row.SetScore(p.Score);
                row.Place(_list.Y(i), height);
            }
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 80, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.88f, 0.95f, 0.98f);

            _winner = UiBuilder.Text("Winner", transform, 64, TvColors.Crown, true, TextAlignmentOptions.Center);
            _winner.enableAutoSizing = true;
            _winner.fontSizeMin = 44;
            _winner.fontSizeMax = 64;
            _winner.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Anchor(_winner.rectTransform, 0.03f, 0.79f, 0.97f, 0.88f);

            _list = LeaderboardList.Create(transform);
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.03f, 0.95f, 0.77f);
        }
    }
}
