using System.Collections.Generic;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Results (docs/02_show_flow.md, Day 2): the title, the winner, the top 3 (every player with shared rank 3 or
    /// better, so a tie for third is not cut) with their correct answers, and the Most correct award.
    /// </summary>
    public sealed class ResultsView : MonoBehaviour
    {
        private const int TopRanks = 3;
        private const string NameSeparator = ", ";

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _winner;
        private LeaderboardList _list;
        private TextMeshProUGUI _award;

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
            Refresh();
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

            var top = new List<PlayerState>();
            foreach (var p in Standings.ByScore(state))
            {
                if (Ranking.SharedRank(state, p) <= TopRanks) top.Add(p);
            }

            _list.Rebuild(top);
            float height = _list.RowHeight();
            for (int i = 0; i < top.Count; i++)
            {
                var p = top[i];
                var row = _list.Find(p.PlayerId);
                row.Set(Ranking.SharedRank(state, p), p.Name, _context.ChipColor(p.CharacterId), p.Connected,
                    _context.Format("tv.results.correctCount", "count", p.CorrectCount.ToString()), TvColors.DimText);
                row.SetScore(p.Score);
                row.Place(_list.Y(i), height);
            }

            var names = new List<string>();
            foreach (var id in state.MostCorrectIds)
            {
                var p = state.FindById(id);
                if (p != null) names.Add(p.Name);
            }

            _award.gameObject.SetActive(names.Count > 0);
            _award.text = names.Count > 0 ? _context.Format("tv.awards.mostCorrect", "names", string.Join(NameSeparator, names)) : string.Empty;
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
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.15f, 0.95f, 0.71f);

            _award = UiBuilder.Text("Award", transform, 52, TvColors.Text, true, TextAlignmentOptions.Center);
            _award.enableAutoSizing = true;
            _award.fontSizeMin = 40;
            _award.fontSizeMax = 52;
            UiBuilder.Anchor(_award.rectTransform, 0.04f, 0.03f, 0.96f, 0.13f);
        }
    }
}
