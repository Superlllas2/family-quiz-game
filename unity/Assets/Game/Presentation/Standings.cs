using System;
using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>Players ordered for the TV leaderboards: by score, highest first; equal scores keep join order.</summary>
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
