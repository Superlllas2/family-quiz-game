using System.Collections.Generic;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Scores and Attacks (docs/02_show_flow.md, decisions/0017_game_night_ui.md), on the Wide shot with the guests in
    /// full view:
    /// <list type="bullet">
    /// <item>Scores: a row of score cards along the bottom in sofa order (D159), each under its own character, so nobody
    /// reads a ranking direction into it; each card counts up to its new score and shows the points just earned on a
    /// sticker and its shared rank on a coin; the leaders (every player on rank 1) wear a crown. Above them the question
    /// card's back stays up: "Why?" and the fact. An arrow in the power up's colour flies from attacker to target for
    /// every power up used in this pick window (D149).</item>
    /// <item>Attacks (only after a power up was used, D149): the same cards; every arrow of the question flies again at
    /// once and each target's card shows the icons of the power ups waiting for it.</item>
    /// </list>
    /// The title ribbon at the top right; power up tags under it. Reads GameState; changes nothing.
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
        private AttackLayer _arrows;
        private GamePhase _mode = GamePhase.Scores;
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

        /// <summary>Called on PhaseStarted(Scores): the cards in sofa order count up to the new scores.</summary>
        public void Enter()
        {
            _mode = GamePhase.Scores;
            var state = _context.Game.State;
            var question = state.Question;
            _title.Set(_context.T("tv.scores.title"), question != null
                ? _context.Format("tv.scores.subtitle", "number", (question.Slot + 1).ToString(), "total", state.QuestionCount.ToString())
                : null);
            _title.Rise();
            ShowFact(question);

            var order = SofaOrder.Players(state, _context.Seats.SeatOf);
            Rebuild(order);
            Measure(order.Count, out float width, out float height);
            var labels = Labels(state);
            var leaders = Leaders(state);
            for (int i = 0; i < order.Count; i++)
            {
                var player = order[i];
                var card = Find(player.PlayerId);
                Fill(card, player, labels);
                int rankBefore = player.Turn != null && player.Turn.RankBefore > 0 ? player.Turn.RankBefore : Ranking.SharedRank(state, player);
                float x = X(i, width);
                card.Move(x, x, width, height, Standings.ScoreBefore(player), player.Score,
                    rankBefore, Ranking.SharedRank(state, player), TvMotion.RowMoveDelaySeconds);
                card.SetCrown(leaders.Contains(player.PlayerId), true, TvMotion.RowMoveDelaySeconds + TvMotion.RowMoveSeconds);
                card.SetEffects(null);
            }

            _arrows.Clear();
            RefreshHits();
        }

        /// <summary>
        /// Called on PhaseStarted(Attacks) (D149): the Scores cards stay; every arrow of this pick window flies again at
        /// once and the targets show their power up icons. The fact card makes room for the arrows.
        /// </summary>
        public void EnterAttacks()
        {
            _mode = GamePhase.Attacks;
            _title.Set(_context.T("tv.attacks.title"), _context.T("tv.attacks.subtitle"));
            _title.Rise();
            _fact.gameObject.SetActive(false);
            if (_cards.Count != _context.Game.State.Players.Count) Refresh();
            _arrows.Clear();
            RefreshHits();
        }

        /// <summary>Roster or connection changes: a leave or join rebuilds the cards at their final places; otherwise only the looks update.</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            var order = SofaOrder.Players(state, _context.Seats.SeatOf);
            bool rebuild = order.Count != _cards.Count;
            for (int i = 0; !rebuild && i < order.Count; i++) rebuild = _cards[i].PlayerId != order[i].PlayerId;
            if (rebuild) Rebuild(order);
            Measure(order.Count, out float width, out float height);
            var labels = Labels(state);
            var leaders = Leaders(state);
            for (int i = 0; i < order.Count; i++)
            {
                var card = _cards[i];
                Fill(card, order[i], labels);
                if (rebuild)
                {
                    card.Place(X(i, width), width, height, order[i].Score, Ranking.SharedRank(state, order[i]));
                    card.SetCrown(leaders.Contains(order[i].PlayerId), false);
                }
            }

            RefreshHits();
        }

        /// <summary>A power up was drawn or used in the pick window: its tag and its arrow show at once (docs/03_rules.md, "Using").</summary>
        public void RefreshHits()
        {
            var state = _context.Game.State;
            _hits.Refresh(state, _mode);
            _arrows.Show(_context.Arrows(), AnchorOf, Vector2.up);
            foreach (var card in _cards)
            {
                if (_mode != GamePhase.Attacks)
                {
                    card.SetEffects(null);
                    continue;
                }

                var player = state.FindById(card.PlayerId);
                var effects = new List<string>();
                foreach (var hit in PowerUpHit.All(player, GamePhase.Attacks)) effects.Add(hit.PowerUp);
                card.SetEffects(effects);
            }
        }

        private (RectTransform Rect, Vector2 Local)? AnchorOf(string playerId)
        {
            var card = Find(playerId);
            if (card == null) return null;
            return (card.Rect, card.ArrowPoint);
        }

        private void ShowFact(QuestionInPlay question)
        {
            var fact = question?.Def.Fact;
            string factText = fact != null ? fact.Get(_context.Lang) : null;
            bool hasFact = !string.IsNullOrEmpty(factText);
            _fact.gameObject.SetActive(hasFact);
            if (!hasFact) return;
            _fact.ShowFront(string.Empty, TopicTitle(question.Def.Topic), null, null, null);
            _fact.ShowBack(_context.T("tv.reveal.why"), factText);
        }

        private static HashSet<string> Leaders(GameState state)
        {
            var ids = new HashSet<string>();
            foreach (var p in Standings.Leaders(state)) ids.Add(p.PlayerId);
            return ids;
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

        /// <summary>The left edge of the card at this sofa place (0 = leftmost seat).</summary>
        private float X(int index, float width)
        {
            int slot = Mathf.Clamp(index, 0, _rowCount - 1);
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
            _arrows = AttackLayer.Create(transform);
        }
    }
}
