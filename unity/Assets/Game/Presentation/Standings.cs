using System;
using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Players ordered for the TV leaderboards: by score, highest first; equal scores keep join order. Also the round
    /// winners for RoundEnd and the top places for Results.
    /// </summary>
    public static class Standings
    {
        public static List<PlayerState> ByScore(GameState state)
        {
            return By(state, p => p.Score);
        }

        /// <summary>The order before the current question: each score minus the points this question gave.</summary>
        public static List<PlayerState> ByScoreBefore(GameState state)
        {
            return By(state, ScoreBefore);
        }

        public static int ScoreBefore(PlayerState player)
        {
            return player.Score - PointsNow(player);
        }

        /// <summary>Points the player earned on the current question (0 outside a question or before Reveal).</summary>
        public static int PointsNow(PlayerState player)
        {
            return player.Turn != null && player.Turn.Revealed ? player.Turn.Points : 0;
        }

        /// <summary>Points the player earned in the current round so far (0 outside a round).</summary>
        public static int RoundPoints(GameState state, PlayerState player)
        {
            var round = state.Round;
            return round != null && round.Points.TryGetValue(player.PlayerId, out var points) ? points : 0;
        }

        /// <summary>
        /// The round winners on RoundEnd: the most points this round, every tied player, in join order. Nobody when
        /// nobody scored this round.
        /// </summary>
        public static List<PlayerState> RoundWinners(GameState state)
        {
            int best = 0;
            foreach (var p in state.Players) best = Math.Max(best, RoundPoints(state, p));
            var winners = new List<PlayerState>();
            if (best <= 0) return winners;
            foreach (var p in state.Players)
            {
                if (RoundPoints(state, p) == best) winners.Add(p);
            }

            return winners;
        }

        /// <summary>
        /// The first places of the final order (GameState.Placement, winner first), at most count of them; players who
        /// are no longer seated are passed over. Place n is index n - 1.
        /// </summary>
        public static List<PlayerState> Top(GameState state, int count)
        {
            var top = new List<PlayerState>();
            foreach (var id in state.Placement)
            {
                if (top.Count >= count) break;
                var p = state.FindById(id);
                if (p != null) top.Add(p);
            }

            return top;
        }

        private static List<PlayerState> By(GameState state, Func<PlayerState, int> score)
        {
            var players = state.Players;
            var order = new List<int>(players.Count);
            for (int i = 0; i < players.Count; i++) order.Add(i);
            // List.Sort is not stable: break ties by join index.
            order.Sort((a, b) =>
            {
                int byScore = score(players[b]).CompareTo(score(players[a]));
                return byScore != 0 ? byScore : a.CompareTo(b);
            });
            var result = new List<PlayerState>(players.Count);
            foreach (var i in order) result.Add(players[i]);
            return result;
        }
    }
}
