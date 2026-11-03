using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// TopicVote (docs/02_show_flow.md): the round title and its multiplier, the topic doors side by side (the last one
    /// is the mixed door) with live vote counts, the vote timer bar, and each player's chip with its rim and a pop once
    /// they voted. Reads GameState.Round; changes nothing.
    /// </summary>
    public sealed class TopicVoteView : MonoBehaviour
    {
        private const float DoorGap = 32f;
        private const float ChipHeight = 86f;

        private readonly List<TopicDoor> _doors = new List<TopicDoor>();
        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _multiplier;
        private RectTransform _doorsRoot;
        private TimerBar _bar;
        private ChipRow _chips;

        public static TopicVoteView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TopicVoteView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<TopicVoteView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            Refresh();
            Tick();
        }

        /// <summary>Texts, door titles, vote counts and chips, all from the state.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var round = state.Round;
            _chips.Sync(state, ChipHeight);
            if (round == null)
            {
                _title.text = string.Empty;
                _multiplier.text = string.Empty;
                SyncDoors(0);
                return;
            }

            _title.text = _context.Format("tv.topicVote.title", "round", (round.Index + 1).ToString());
            _multiplier.text = _context.Format("tv.topicVote.multiplier", "multiplier", NumberText.Format(round.Multiplier));

            SyncDoors(round.Doors.Count);
            var lang = _context.Lang;
            var counts = round.VoteCounts();
            for (int i = 0; i < _doors.Count; i++)
            {
                _doors[i].Set(round.Doors[i].Title.Get(lang), _context.Format("tv.topicVote.votes", "count", counts[i].ToString()));
            }

            for (int i = 0; i < state.Players.Count && i < _chips.Chips.Count; i++)
            {
                _chips.Chips[i].SetLocked(round.Votes.ContainsKey(state.Players[i].PlayerId));
            }
        }

        /// <summary>A vote (or a changed vote): counts from the state, and the voter's chip pops.</summary>
        public void OnVoteCast(string playerId)
        {
            Refresh();
            var chip = _chips.Find(playerId);
            if (chip != null) chip.Pop();
        }

        /// <summary>Per frame: the vote timer bar drains with the core clock.</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            if (!state.PhaseEndsAtMs.HasValue)
            {
                _bar.SetVisible(false);
                return;
            }

            _bar.SetVisible(true);
            // The bar shows the vote timer; the grace after it (D84) is not shown.
            long end = state.Round != null ? state.Round.VoteDeadlineMs : state.PhaseEndsAtMs.Value;
            long span = end - state.PhaseStartedAtMs;
            long left = System.Math.Max(0, end - game.NowMs);
            _bar.Set(span > 0 ? (float)left / span : 0f, TvColors.Text);
        }

        private void SyncDoors(int count)
        {
            if (_doors.Count == count) return;
            foreach (var door in _doors) Destroy(door.gameObject);
            _doors.Clear();
            for (int i = 0; i < count; i++)
            {
                var door = TopicDoor.Create(_doorsRoot, i);
                float minX = (float)i / count;
                float maxX = (float)(i + 1) / count;
                var rect = UiBuilder.Anchor(door.Rect, minX, 0, maxX, 1);
                rect.offsetMin = new Vector2(i > 0 ? DoorGap / 2 : 0, 0);
                rect.offsetMax = new Vector2(i < count - 1 ? -DoorGap / 2 : 0, 0);
                _doors.Add(door);
            }
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 88, TvColors.Text, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_title.rectTransform, 0.04f, 0.86f, 0.96f, 0.97f);

            _multiplier = UiBuilder.Text("Multiplier", transform, 56, TvColors.DimText, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_multiplier.rectTransform, 0.04f, 0.79f, 0.96f, 0.86f);

            _doorsRoot = UiBuilder.Anchor(UiBuilder.Rect("Doors", transform), 0.04f, 0.27f, 0.96f, 0.76f);

            _bar = TimerBar.Create(transform);
            UiBuilder.Anchor(_bar.Root, 0.04f, 0.2f, 0.96f, 0.215f);

            _chips = ChipRow.Create(transform, _context);
            UiBuilder.Anchor((RectTransform)_chips.transform, 0.02f, 0.025f, 0.98f, 0.105f);
        }
    }
}
