using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>One player's pick on a Reveal tile (D142): their chip, name and the points they earned.</summary>
    public sealed class RevealPick
    {
        public RevealPick(string playerId, string name, int characterId, int seat, int points, bool correct)
        {
            PlayerId = playerId;
            Name = name;
            CharacterId = characterId;
            Seat = seat;
            Points = points;
            Correct = correct;
        }

        public string PlayerId { get; }
        public string Name { get; }
        public int CharacterId { get; }

        /// <summary>The player's index in the roster (for their token letters).</summary>
        public int Seat { get; }

        public int Points { get; }
        public bool Correct { get; }
    }

    /// <summary>
    /// Who picked which multiple choice tile, for the Reveal (D142): each player who locked a choice sits on that tile,
    /// in seat order; a player without an answer sits on none. Plain C#, so it is tested without the engine.
    /// </summary>
    public static class RevealPicks
    {
        public static List<RevealPick>[] ByTile(GameState state, int tiles)
        {
            var result = new List<RevealPick>[tiles];
            for (int i = 0; i < tiles; i++) result[i] = new List<RevealPick>();
            if (state == null) return result;
            for (int seat = 0; seat < state.Players.Count; seat++)
            {
                var p = state.Players[seat];
                var turn = p.Turn;
                if (turn == null || !turn.Locked || !turn.Choice.HasValue) continue;
                int choice = turn.Choice.Value;
                if (choice < 0 || choice >= tiles) continue;
                result[choice].Add(new RevealPick(p.PlayerId, p.Name, p.CharacterId, seat, turn.Points, turn.Correct));
            }

            return result;
        }
    }
}
