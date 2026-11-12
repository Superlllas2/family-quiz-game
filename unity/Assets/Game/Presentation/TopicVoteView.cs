using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// TopicVote (docs/02_show_flow.md, decisions/0017_game_night_ui.md): the round's banner with its multiplier sticker,
    /// the topic cards two by two like the phone's buttons (D128: 1 2 on top, 3 4 below; the last one is the mixed door) with the voters'
    /// tokens on them, and the status corner: the vote clock and each player's token with its check once they voted.
    /// TopicTie (D153, only on a tie or when nobody voted) keeps the view up without the clock: a highlight spins
    /// between the tied doors, slowing down, and stops on the chosen door, which then stands out.
    /// Reads GameState.Round; changes nothing.
    /// </summary>
    public sealed class TopicVoteView : MonoBehaviour
    {
        private const float CardGapX = 48f;
        private const float CardGapY = 30f;
        private const float CardTilt = 0.8f;

        private readonly List<TopicDoor> _doors = new List<TopicDoor>();
        private TvContext _context;
        private TvPiece _banner;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private TvPiece _sticker;
        private TextMeshProUGUI _stickerText;
        private RectTransform _doorsRoot;
        private StatusStrip _status;
        private bool _tie;

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
            _tie = false;
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

        /// <summary>Called on PhaseStarted(TopicTie): the vote view stays and the spin starts (D153).</summary>
        public void EnterTie()
        {
            _tie = true;
            Refresh();
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
            _hint.text = _context.T(_tie ? "tv.topicTie.hint" : "tv.topicVote.hint");
            bool multiplied = MultiplierSticker.Shows(round.Multiplier); // never while every multiplier is 1 (D151)
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
            if (_tie)
            {
                _status.SetClockVisible(false);
                Spin(state, game.NowMs);
                return;
            }

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

        /// <summary>The TopicTie spin from the core clock: the lit door, then the chosen one standing out (D153).</summary>
        private void Spin(FamilyQuiz.Core.State.GameState state, long now)
        {
            var round = state.Round;
            if (round == null || !round.ChosenDoor.HasValue) return;
            long span = state.PhaseEndsAtMs.HasValue ? state.PhaseEndsAtMs.Value - state.PhaseStartedAtMs : 0;
            float fraction = span > 0 ? Mathf.Clamp01((float)(now - state.PhaseStartedAtMs) / span) : 1f;
            int chosen = round.ChosenDoor.Value;
            int lit = TopicTieSpin.DoorAt(round.TiedDoors, chosen, fraction);
            bool stopped = TopicTieSpin.HopAt(fraction) >= TopicTieSpin.Hops;
            for (int d = 0; d < _doors.Count; d++)
            {
                bool tied = false;
                foreach (var t in round.TiedDoors) tied |= t == d;
                bool winner = stopped && d == chosen;
                _doors[d].SetSpin(!stopped && d == lit, !tied || (stopped && d != chosen), winner);
            }
        }

        /// <summary>
        /// The cards two by two in the cards' zone, in the phone's order (D128): row by row, left to right. Each card is
        /// turned a little, alternately, like cards laid on a table.
        /// </summary>
        private void SyncDoors(int count)
        {
            if (_doors.Count == count) return;
            foreach (var door in _doors) Destroy(door.gameObject);
            _doors.Clear();
            var zone = TvLayout.VoteCards;
            int rows = Mathf.Max(1, (count + 1) / 2);
            float width = (zone.width - CardGapX) / 2f;
            float height = (zone.height - CardGapY * (rows - 1)) / rows;
            for (int i = 0; i < count; i++)
            {
                var door = TopicDoor.Create(_doorsRoot, i);
                int row = i / 2;
                int column = i % 2;
                var rect = door.Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(column * (width + CardGapX) + width / 2f,
                    -(row * (height + CardGapY) + height / 2f));
                rect.sizeDelta = new Vector2(width, height);
                float tilt = (i % 2 == 0 ? -1f : 1f) * (row == 0 ? 1f : -1f) * CardTilt;
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
