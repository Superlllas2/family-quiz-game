using System.Collections.Generic;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The arrows of this pick window (D149), from the PowerUpUsed events the presenter collected, in the order the power
    /// ups were used. Both players must still be in the roster and differ. Plain C#, tested without the engine.
    /// </summary>
    public static class AttackArrows
    {
        public static List<AttackArrow> Build(IEnumerable<PowerUpUsed> used, GameState state)
        {
            var arrows = new List<AttackArrow>();
            if (used == null || state == null) return arrows;
            var pairs = new Dictionary<string, int>();
            var kept = new List<(PowerUpUsed Used, int Lane, int FromEnd, int ToEnd)>();
            var ends = new Dictionary<string, int>();
            foreach (var u in used)
            {
                if (u == null || u.PlayerId == u.TargetId) continue;
                if (state.FindById(u.PlayerId) == null || state.FindById(u.TargetId) == null) continue;
                string pair = string.CompareOrdinal(u.PlayerId, u.TargetId) < 0
                    ? u.PlayerId + "\n" + u.TargetId
                    : u.TargetId + "\n" + u.PlayerId;
                pairs.TryGetValue(pair, out int lane);
                pairs[pair] = lane + 1;
                ends.TryGetValue(u.PlayerId, out int fromEnd);
                ends[u.PlayerId] = fromEnd + 1;
                ends.TryGetValue(u.TargetId, out int toEnd);
                ends[u.TargetId] = toEnd + 1;
                kept.Add((u, lane, fromEnd, toEnd));
            }

            foreach (var k in kept)
            {
                arrows.Add(new AttackArrow(k.Used.PlayerId, k.Used.TargetId, k.Used.PowerUp, k.Lane,
                    k.FromEnd, ends[k.Used.PlayerId], k.ToEnd, ends[k.Used.TargetId]));
            }

            return arrows;
        }
    }
}
