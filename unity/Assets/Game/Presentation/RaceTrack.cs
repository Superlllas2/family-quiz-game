using System.Collections.Generic;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The flat race track (docs/02_show_flow.md, "The final race"): one cell per step from 0 to race.finishSteps side by
    /// side, the finish line and "Finish" over the last cell, and one lane per player in join order with their chip on
    /// their step. Lanes are placed from the top like leaderboard rows, shrinking when many players do not fit.
    /// </summary>
    public sealed class RaceTrack : MonoBehaviour
    {
        private const float MaxLaneHeight = 100f;
        private const float FallbackHeight = 560f;
        private const float FinishLineWidth = 8f;
        private const float FinishLabelHeight = 56f;

        private readonly List<RaceRunner> _runners = new List<RaceRunner>();
        private readonly List<Image> _cells = new List<Image>();
        private TvContext _context;
        private RectTransform _lanes;
        private Image _finishLine;
        private TextMeshProUGUI _finishLabel;
        private int _finishSteps = -1;

        public RectTransform Rect => (RectTransform)transform;

        public IReadOnlyList<RaceRunner> Runners => _runners;

        public static RaceTrack Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Rect("RaceTrack", parent);
            var track = rect.gameObject.AddComponent<RaceTrack>();
            track._context = context;
            track.Build();
            return track;
        }

        public RaceRunner Find(string playerId)
        {
            foreach (var runner in _runners)
            {
                if (runner.PlayerId == playerId) return runner;
            }

            return null;
        }

        /// <summary>
        /// Matches the cells to finishSteps and the lanes to the seated players, and updates names, colors and
        /// connection. New runners stand on their step from the race state.
        /// </summary>
        public void Sync(GameState state, int finishSteps)
        {
            if (finishSteps != _finishSteps) BuildCells(finishSteps);
            _finishLabel.text = _context.T("tv.race.finish");

            var players = state.Players;
            if (!SameRoster(players))
            {
                foreach (var runner in _runners) Destroy(runner.gameObject);
                _runners.Clear();
                foreach (var p in players)
                {
                    var runner = RaceRunner.Create(_lanes, p.PlayerId);
                    runner.Place(state.Race != null ? state.Race.PositionOf(p.PlayerId) : 0, finishSteps);
                    _runners.Add(runner);
                }
            }

            float height = Rect.rect.height > 0f ? Rect.rect.height : FallbackHeight;
            float lane = Mathf.Min(MaxLaneHeight, (height - FinishLabelHeight) / Mathf.Max(1, _runners.Count));
            _lanes.sizeDelta = new Vector2(0f, lane * _runners.Count);
            for (int i = 0; i < _runners.Count; i++)
            {
                var p = players[i];
                _runners[i].SetLane(i * lane, lane);
                _runners[i].Chip.Set(p.Name, _context.ChipColor(p.CharacterId), p.Connected);
            }
        }

        /// <summary>Every runner on its step from the race state at once.</summary>
        public void PlaceAll(RaceState race, int finishSteps)
        {
            foreach (var runner in _runners) runner.Place(race.PositionOf(runner.PlayerId), finishSteps);
        }

        /// <summary>Runners whose step changed slide to it after delay seconds; the others stay.</summary>
        public void SlideAll(RaceState race, int finishSteps, float delay)
        {
            foreach (var runner in _runners) runner.Slide(race.PositionOf(runner.PlayerId), finishSteps, delay);
        }

        private void BuildCells(int finishSteps)
        {
            _finishSteps = finishSteps;
            foreach (var cell in _cells) Destroy(cell.gameObject);
            _cells.Clear();
            int cells = RaceTrackLayout.Cells(finishSteps);
            float width = RaceTrackLayout.CellWidth(finishSteps);
            for (int i = 0; i < cells; i++)
            {
                var color = i == cells - 1 ? TvColors.Finish : i % 2 == 0 ? TvColors.Card : TvColors.BarTrack;
                var cell = UiBuilder.Image("Cell " + i, _lanes, color);
                UiBuilder.Anchor(cell.rectTransform, i * width, 0f, (i + 1) * width, 1f);
                cell.transform.SetAsFirstSibling(); // behind the runners
                _cells.Add(cell);
            }

            float finish = RaceTrackLayout.FinishLine(finishSteps);
            var line = _finishLine.rectTransform;
            line.anchorMin = new Vector2(finish, 0f);
            line.anchorMax = new Vector2(finish, 1f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.anchoredPosition = Vector2.zero;
            line.sizeDelta = new Vector2(FinishLineWidth, 0f);

            var label = _finishLabel.rectTransform;
            label.anchorMin = new Vector2(finish, 1f);
            label.anchorMax = new Vector2(1f, 1f);
            label.pivot = new Vector2(0.5f, 0f);
            label.anchoredPosition = Vector2.zero;
            label.sizeDelta = new Vector2(0f, FinishLabelHeight);
        }

        private bool SameRoster(IReadOnlyList<PlayerState> players)
        {
            if (players.Count != _runners.Count) return false;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].PlayerId != _runners[i].PlayerId) return false;
            }

            return true;
        }

        private void Build()
        {
            // The lanes hang from the top of the track, under the "Finish" label; their height follows the player count.
            _lanes = UiBuilder.Rect("Lanes", transform);
            _lanes.anchorMin = new Vector2(0f, 1f);
            _lanes.anchorMax = new Vector2(1f, 1f);
            _lanes.pivot = new Vector2(0.5f, 1f);
            _lanes.anchoredPosition = new Vector2(0f, -FinishLabelHeight);
            _lanes.sizeDelta = Vector2.zero;

            _finishLine = UiBuilder.Image("FinishLine", _lanes, TvColors.Text);

            _finishLabel = UiBuilder.Text("Finish", _lanes, 44, TvColors.Text, true, TextAlignmentOptions.Bottom);
            _finishLabel.textWrappingMode = TextWrappingModes.NoWrap;
            _finishLabel.overflowMode = TextOverflowModes.Overflow;
        }
    }
}
