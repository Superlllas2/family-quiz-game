using System;
using System.Collections.Generic;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The players as they sit on the TV from left to right (D159): the Scores row puts each card under its own
    /// character, so nobody reads a ranking direction into it. Seats are numbered in fill order (docs/21_staging.md,
    /// "Seats"; tools/blender/build_studio.py): 0 and 1 the middle of the sofa, 2 and 3 its outer seats, 4 and 5 the
    /// inner armchairs, 6 and 7 the outer ones, the even seat of each pair on the left. Plain C#, tested without the engine.
    /// </summary>
    public static class SofaOrder
    {
        /// <summary>Seat indices from the left of the screen to the right.</summary>
        public static readonly int[] SeatsLeftToRight = { 6, 4, 2, 0, 1, 3, 5, 7 };

        /// <summary>A seat's place from the left, or -1 for no seat.</summary>
        public static int Slot(int seat)
        {
            return Array.IndexOf(SeatsLeftToRight, seat);
        }

        /// <summary>The players from left to right by their seat; players without a seat come last, in join order.</summary>
        public static List<PlayerState> Players(GameState state, Func<string, int> seatOf)
        {
            var players = state.Players;
            var order = new List<int>(players.Count);
            var slots = new int[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                order.Add(i);
                int slot = Slot(seatOf != null ? seatOf(players[i].PlayerId) : -1);
                slots[i] = slot < 0 ? int.MaxValue : slot;
            }

            // List.Sort is not stable: break ties by join index.
            order.Sort((a, b) =>
            {
                int bySlot = slots[a].CompareTo(slots[b]);
                return bySlot != 0 ? bySlot : a.CompareTo(b);
            });
            var result = new List<PlayerState>(players.Count);
            foreach (var i in order) result.Add(players[i]);
            return result;
        }
    }
}
