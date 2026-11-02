using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>Shared rank: equal scores share the better rank, so 900, 900, 400 give 1, 1, 3 (D22).</summary>
    public static class Ranking
    {
        public static int SharedRank(GameState state, PlayerState player)
        {
            int better = 0;
            foreach (var other in state.Players)
            {
                if (other.Score > player.Score) better++;
            }

            return better + 1;
        }
    }
}
