using FamilyQuiz.Core.Rules;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Scores (docs/02_show_flow.md): the leaderboard with shared ranks (1, 1, 3), each player's chip, score and the
    /// points just earned. Rows start in the order before the question and glide to their new places.
    /// </summary>
    public sealed class LeaderboardView : MonoBehaviour
    {
        private TvContext _context;
        private TextMeshProUGUI _title;
        private LeaderboardList _list;

        public static LeaderboardView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("LeaderboardView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<LeaderboardView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Called on PhaseStarted(Scores): rows from the old order to the new one.</summary>
        public void Enter()
        {
            _title.text = _context.T("tv.scores.title");
            var state = _context.Game.State;
            var before = Standings.ByScoreBefore(state);
            var after = Standings.ByScore(state);
            _list.Rebuild(after);
            float height = _list.RowHeight();
            for (int i = 0; i < after.Count; i++)
            {
                var player = after[i];
                var row = _list.Find(player.PlayerId);
                Fill(row, player);
                int oldIndex = before.IndexOf(player);
                int rankBefore = player.Turn != null && player.Turn.RankBefore > 0 ? player.Turn.RankBefore : Ranking.SharedRank(state, player);
                row.Move(_list.Y(oldIndex), _list.Y(i), height, Standings.ScoreBefore(player), player.Score,
                    rankBefore, Ranking.SharedRank(state, player), TvMotion.RowMoveDelaySeconds);
            }
        }

        /// <summary>Roster or connection changes: a leave rebuilds the rows at their final places; otherwise only the looks update.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var after = Standings.ByScore(state);
            bool rebuild = !_list.Matches(after);
            if (rebuild) _list.Rebuild(after);
            float height = _list.RowHeight();
            for (int i = 0; i < after.Count; i++)
            {
                var row = _list.Find(after[i].PlayerId);
                Fill(row, after[i]);
                if (rebuild)
                {
                    row.Place(_list.Y(i), height);
                    row.SetScore(after[i].Score);
                }
            }
        }

        private void Fill(LeaderboardRow row, Core.State.PlayerState player)
        {
            int points = Standings.PointsNow(player);
            string extra = points > 0 ? _context.Format("tv.reveal.points", "points", points.ToString()) : string.Empty;
            row.Set(Ranking.SharedRank(_context.Game.State, player), player.Name, _context.ChipColor(player.CharacterId),
                player.Connected, extra, TvColors.Correct);
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 80, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.87f, 0.95f, 0.98f);
            _list = LeaderboardList.Create(transform);
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.03f, 0.95f, 0.85f);
        }
    }
}
