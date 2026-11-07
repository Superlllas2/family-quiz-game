using System.Collections.Generic;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Scores (docs/02_show_flow.md, decisions/0017_game_night_ui.md), on the Wide shot with the guests in full view: a
    /// row of score cards along the bottom, one per player in rank order read left to right, 1st place on the right
    /// (D128; shared ranks 1, 1, 3), each with the points
    /// just earned on a sticker; the cards start in the order before the question and glide to their new places. Above
    /// them the question card's back stays up: "Why?" and the fact. The title ribbon at the top right; power up tags
    /// for the hits used in this pick window under it.
    /// </summary>
    public sealed class LeaderboardView : MonoBehaviour
    {
        private const float CardGap = 18f;
        private const float MaxCardWidth = 300f;

        private readonly List<ScoreCard> _cards = new List<ScoreCard>();
        private TvContext _context;
        private TitleRibbon _title;
        private QuestionCard _fact;
        private RectTransform _row;
        private HitList _hits;
        private float _rowStart;
        private int _rowCount;

        public static LeaderboardView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("LeaderboardView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<LeaderboardView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Called on PhaseStarted(Scores): cards from the old order to the new one.</summary>
        public void Enter()
        {
            var state = _context.Game.State;
            var question = state.Question;
            _title.Set(_context.T("tv.scores.title"), question != null
                ? _context.Format("tv.scores.subtitle", "number", (question.Slot + 1).ToString(), "total", state.QuestionCount.ToString())
                : null);
            _title.Rise();

            var fact = question?.Def.Fact;
            string factText = fact != null ? fact.Get(_context.Lang) : null;
            bool hasFact = !string.IsNullOrEmpty(factText);
            _fact.gameObject.SetActive(hasFact);
            if (hasFact)
            {
                _fact.ShowFront(string.Empty, TopicTitle(question.Def.Topic), null, null, null);
                _fact.ShowBack(_context.T("tv.reveal.why"), factText);
            }

            var before = Standings.ByScoreBefore(state);
            var after = Standings.ByScore(state);
            Rebuild(after);
            Measure(after.Count, out float width, out float height);
            var labels = Labels(state);
            for (int i = 0; i < after.Count; i++)
            {
                var player = after[i];
                var card = Find(player.PlayerId);
                Fill(card, player, labels);
                int oldIndex = IndexOf(before, player.PlayerId);
                int rankBefore = player.Turn != null && player.Turn.RankBefore > 0 ? player.Turn.RankBefore : Ranking.SharedRank(state, player);
                card.Move(X(oldIndex, width), X(i, width), width, height, Standings.ScoreBefore(player), player.Score,
                    rankBefore, Ranking.SharedRank(state, player), TvMotion.RowMoveDelaySeconds);
            }

            RefreshHits();
        }

        /// <summary>Roster or connection changes: a leave rebuilds the cards at their final places; otherwise only the looks update.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var after = Standings.ByScore(state);
            bool rebuild = after.Count != _cards.Count;
            foreach (var p in after) rebuild |= Find(p.PlayerId) == null;
            if (rebuild) Rebuild(after);
            Measure(after.Count, out float width, out float height);
            var labels = Labels(state);
            for (int i = 0; i < after.Count; i++)
            {
                var card = Find(after[i].PlayerId);
                Fill(card, after[i], labels);
                if (rebuild) card.Place(X(i, width), width, height, after[i].Score, Ranking.SharedRank(state, after[i]));
            }

            RefreshHits();
        }

        /// <summary>A power up was used in the pick window: its tag shows at once (docs/03_rules.md, "Using").</summary>
        public void RefreshHits()
        {
            _hits.Refresh(_context.Game.State, GamePhase.Scores);
        }

        private static int IndexOf(IReadOnlyList<PlayerState> players, string playerId)
        {
            for (int i = 0; i < players.Count; i++) if (players[i].PlayerId == playerId) return i;
            return -1;
        }

        private void Fill(ScoreCard card, PlayerState player, Dictionary<string, string> labels)
        {
            int points = Standings.PointsNow(player);
            string extra = points > 0 ? _context.Format("tv.reveal.points", "points", points.ToString()) : null;
            card.Set(labels.TryGetValue(player.PlayerId, out var l) ? l : "?", player.Name, _context.ChipColor(player.CharacterId),
                player.Connected, extra);
        }

        private static Dictionary<string, string> Labels(GameState state)
        {
            var names = new string[state.Players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = state.Players[i].Name;
            var labels = TokenLabels.For(names);
            var map = new Dictionary<string, string>();
            for (int i = 0; i < names.Length; i++) map[state.Players[i].PlayerId] = labels[i];
            return map;
        }

        private void Rebuild(IReadOnlyList<PlayerState> players)
        {
            foreach (var c in _cards) Destroy(c.gameObject);
            _cards.Clear();
            foreach (var p in players) _cards.Add(ScoreCard.Create(_row, p.PlayerId));
        }

        private ScoreCard Find(string playerId)
        {
            foreach (var c in _cards) if (c.PlayerId == playerId) return c;
            return null;
        }

        /// <summary>Cards as wide as fits (up to MaxCardWidth), the row centred in its zone.</summary>
        private void Measure(int count, out float width, out float height)
        {
            float zone = TvLayout.ScoreRow.width;
            int n = Mathf.Max(1, count);
            width = Mathf.Min(MaxCardWidth, (zone - CardGap * (n - 1)) / n);
            height = TvLayout.ScoreRow.height;
            _rowStart = (zone - (width * n + CardGap * (n - 1))) / 2f;
            _rowCount = n;
        }

        /// <summary>The left edge of the card at this rank index (0 = 1st): 1st place is the rightmost card (D128).</summary>
        private float X(int index, float width)
        {
            int slot = _rowCount - 1 - Mathf.Clamp(index, 0, _rowCount - 1);
            return _rowStart + slot * (width + CardGap);
        }

        private string TopicTitle(string topicId)
        {
            foreach (var t in _context.Game.Content.Topics)
            {
                if (t.Id == topicId) return t.Title.Get(_context.Lang);
            }

            return null;
        }

        private void Build()
        {
            _fact = QuestionCard.Create(transform);
            TvLayout.Put(_fact.Rect, TvLayout.ScoreFact);
            _fact.SetLarge(false);
            _row = TvLayout.Put(UiBuilder.Rect("Row", transform), TvLayout.ScoreRow);
            _title = TitleRibbon.Create(transform, TvColors.Red);
            _hits = HitList.Create(transform, _context);
        }
    }
}
