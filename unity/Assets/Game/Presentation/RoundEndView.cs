using System.Collections.Generic;
using FamilyQuiz.Core.Rules;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// RoundEnd (docs/02_show_flow.md, decisions/0017_game_night_ui.md): "Round N is over" on the title ribbon with the
    /// round winner or winners under it (the most points this round, all tied names; nobody when nobody scored), and the
    /// leaderboard by total score with each player's points this round, under the hosts' and title zones.
    /// </summary>
    public sealed class RoundEndView : MonoBehaviour
    {
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TitleRibbon _title;
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
            _title.Rise();
        }

        /// <summary>Everything from the state: texts, the winners and the rows at their places.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var round = state.Round;
            var names = new List<string>();
            foreach (var p in Standings.RoundWinners(state)) names.Add(p.Name);
            _title.Set(round != null ? _context.Format("tv.roundEnd.title", "round", (round.Index + 1).ToString()) : string.Empty,
                names.Count > 0 ? _context.Format("tv.roundEnd.winner", "names", string.Join(NameSeparator, names)) : null);

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
            _list = LeaderboardList.Create(transform);
            TvLayout.Put(_list.Rect, TvLayout.Content);
            _title = TitleRibbon.Create(transform, TvColors.Mustard);
        }
    }
}
