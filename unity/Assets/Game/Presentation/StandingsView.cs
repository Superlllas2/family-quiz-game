using FamilyQuiz.Core.Rules;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>PlayAgain (docs/02_show_flow.md): the final standings of every seated player and "Play again? Tap on your phone".</summary>
    public sealed class StandingsView : MonoBehaviour
    {
        private TvContext _context;
        private TextMeshProUGUI _title;
        private LeaderboardList _list;
        private TextMeshProUGUI _prompt;

        public static StandingsView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("StandingsView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<StandingsView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            Refresh();
        }

        /// <summary>Players can leave or drop while PlayAgain waits: the list follows.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _title.text = _context.T("tv.results.title");
            _prompt.text = _context.T("tv.playAgain.prompt");
            var order = Standings.ByScore(state);
            _list.Rebuild(order);
            float height = _list.RowHeight();
            for (int i = 0; i < order.Count; i++)
            {
                var p = order[i];
                var row = _list.Find(p.PlayerId);
                row.Set(Ranking.SharedRank(state, p), p.Name, _context.ChipColor(p.CharacterId), p.Connected,
                    _context.Format("tv.results.correctCount", "count", p.CorrectCount.ToString()), TvColors.DimText);
                row.SetScore(p.Score);
                row.Place(_list.Y(i), height);
            }
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 80, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.87f, 0.95f, 0.98f);
            _list = LeaderboardList.Create(transform);
            UiBuilder.Anchor(_list.Rect, 0.05f, 0.16f, 0.95f, 0.85f);
            _prompt = UiBuilder.Text("Prompt", transform, 64, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_prompt.rectTransform, 0.04f, 0.03f, 0.96f, 0.14f);
        }
    }
}
