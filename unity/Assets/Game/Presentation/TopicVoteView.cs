using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// TopicVote (docs/02_show_flow.md, decisions/0017_game_night_ui.md): the round's banner with its multiplier sticker,
    /// the topic cards fanned out like a board game's category cards (the last one is the mixed door) with the voters'
    /// tokens on them, and the status corner: the vote clock and each player's token with its check once they voted.
    /// Reads GameState.Round; changes nothing.
    /// </summary>
    public sealed class TopicVoteView : MonoBehaviour
    {
        private const float CardMaxWidth = 350f;
        private const float CardHeight = 520f;
        private const float FanDegrees = 4.5f;
        private const float FanLift = 34f;

        private readonly List<TopicDoor> _doors = new List<TopicDoor>();
        private TvContext _context;
        private TvPiece _banner;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private TvPiece _sticker;
        private TextMeshProUGUI _stickerText;
        private RectTransform _doorsRoot;
        private StatusStrip _status;

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
            foreach (var door in _doors) Destroy(door.gameObject);
            _doors.Clear();
            _status.Sync(_context.Game.State);
            _status.ClearMarks();
            Refresh();
            TvTween.On(_banner.Root).Rise(0f);
            if (_sticker.Root.gameObject.activeSelf) TvTween.On(_sticker.Root).Slam(0.5f);
            for (int i = 0; i < _doors.Count; i++) TvTween.On(_doors[i].Rect).Pop(0.15f + 0.1f * i, 0.5f);
            Tick();
        }

        /// <summary>Texts, cards, voters and tokens, all from the state.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var round = state.Round;
            _status.Sync(state);
            if (round == null)
            {
                _title.text = string.Empty;
                _sticker.Root.gameObject.SetActive(false);
                SyncDoors(0);
                return;
            }

            _title.text = _context.Format("tv.topicVote.title", "round", (round.Index + 1).ToString());
            _hint.text = _context.T("tv.topicVote.hint");
            bool multiplied = round.Multiplier > 1.0001;
            _sticker.Root.gameObject.SetActive(multiplied);
            _stickerText.text = multiplied
                ? _context.Format("tv.question.multiplier", "multiplier", NumberText.Format(round.Multiplier))
                : string.Empty;

            SyncDoors(round.Doors.Count);
            var lang = _context.Lang;
            var counts = round.VoteCounts();
            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);
            for (int d = 0; d < _doors.Count; d++)
            {
                var door = round.Doors[d];
                _doors[d].Set(door.Title.Get(lang), TvColors.Door(d, door.Mixed),
                    _context.Format("tv.topicVote.votes", "count", counts[d].ToString()));
                var voters = new List<(string, string, Color, bool)>();
                for (int i = 0; i < state.Players.Count; i++)
                {
                    var p = state.Players[i];
                    if (round.Votes.TryGetValue(p.PlayerId, out int choice) && choice == d)
                        voters.Add((p.PlayerId, labels[i], _context.ChipColor(p.CharacterId), p.Connected));
                }

                _doors[d].SetVoters(voters);
            }

            for (int i = 0; i < state.Players.Count && i < _status.Tokens.Count; i++)
            {
                _status.Tokens[i].SetLocked(round.Votes.ContainsKey(state.Players[i].PlayerId));
            }
        }

        /// <summary>A vote (or a changed vote): the cards and tokens from the state, and the voter's check pops.</summary>
        public void OnVoteCast(string playerId)
        {
            Refresh();
            var token = _status.Find(playerId);
            if (token != null)
            {
                token.SetLocked(false);
                token.SetLocked(true, true);
            }
        }

        /// <summary>Per frame: the vote clock runs down with the core clock (the grace after it, D84, is not shown).</summary>
        public void Tick()
        {
            var game = _context.Game;
            var state = game.State;
            if (!state.PhaseEndsAtMs.HasValue)
            {
                _status.SetClockVisible(false);
                return;
            }

            _status.SetClockVisible(true);
            long end = state.Round != null ? state.Round.VoteDeadlineMs : state.PhaseEndsAtMs.Value;
            long span = end - state.PhaseStartedAtMs;
            long left = System.Math.Max(0, end - game.NowMs);
            _status.SetClock(span > 0 ? (float)left / span : 0f, (left + 999) / 1000, left <= 3000);
        }

        /// <summary>The cards fanned across the cards' zone: turned a little each, the middle ones a little higher.</summary>
        private void SyncDoors(int count)
        {
            if (_doors.Count == count) return;
            foreach (var door in _doors) Destroy(door.gameObject);
            _doors.Clear();
            float zone = TvLayout.VoteCards.width;
            float width = Mathf.Min(CardMaxWidth, (zone - 40f * (count + 1)) / Mathf.Max(1, count));
            float step = count > 1 ? Mathf.Min(width + 50f, (zone - width) / (count - 1)) : 0f;
            for (int i = 0; i < count; i++)
            {
                var door = TopicDoor.Create(_doorsRoot, i);
                float t = count > 1 ? (float)i / (count - 1) * 2f - 1f : 0f; // -1 .. 1
                var rect = door.Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2((i - (count - 1) / 2f) * step, 30f + FanLift * (1f - t * t));
                rect.sizeDelta = new Vector2(width, CardHeight);
                float tilt = -FanDegrees * t;
                rect.localRotation = Quaternion.Euler(0, 0, tilt);
                TvTween.On(rect).Tilt = tilt;
                _doors.Add(door);
            }
        }

        private void Build()
        {
            _doorsRoot = TvLayout.Put(UiBuilder.Rect("Doors", transform), TvLayout.VoteCards);

            var bannerRoot = TvLayout.Put(UiBuilder.Rect("BannerZone", transform), TvLayout.VoteTitle);
            _banner = TvPiece.Card("Banner", bannerRoot, TvColors.Plum, 14f, 7f).Tilt(-1.5f);
            TvTween.On(_banner.Root).Tilt = -1.5f;
            _banner.Root.anchorMin = new Vector2(0f, 0.1f);
            _banner.Root.anchorMax = new Vector2(0.84f, 0.9f);
            _banner.Root.offsetMin = Vector2.zero;
            _banner.Root.offsetMax = Vector2.zero;
            _title = UiBuilder.Display("Title", _banner.Content, 32, TvColors.Cream, TextAlignmentOptions.Left);
            UiBuilder.Fit(_title, 20, 32, 1);
            UiBuilder.Anchor(_title.rectTransform, 0.03f, 0.1f, 0.62f, 0.9f);
            _hint = UiBuilder.Text("Hint", _banner.Content, 26, TvColors.Cream, false, TextAlignmentOptions.Right);
            UiBuilder.Fit(_hint, 16, 26, 1);
            UiBuilder.Anchor(_hint.rectTransform, 0.62f, 0.1f, 0.97f, 0.9f);

            _sticker = TvPiece.Star("Multiplier", bannerRoot, TvColors.Red);
            var s = _sticker.Root;
            s.anchorMin = s.anchorMax = new Vector2(0.92f, 0.5f);
            s.pivot = new Vector2(0.5f, 0.5f);
            s.anchoredPosition = Vector2.zero;
            s.sizeDelta = new Vector2(92f, 92f);
            s.localRotation = Quaternion.Euler(0, 0, 12f);
            TvTween.On(s).Tilt = 12f;
            _stickerText = UiBuilder.Display("Text", _sticker.Content, 24, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_stickerText, 14, 24, 1);
            UiBuilder.Anchor(_stickerText.rectTransform, 0.16f, 0.2f, 0.84f, 0.8f);

            _status = StatusStrip.Create(transform, _context);
        }
    }
}
