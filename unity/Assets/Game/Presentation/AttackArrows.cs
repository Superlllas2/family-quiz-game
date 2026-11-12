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
            foreach (var u in used)
            {
                if (u == null || u.PlayerId == u.TargetId) continue;
                if (state.FindById(u.PlayerId) == null || state.FindById(u.TargetId) == null) continue;
                string pair = string.CompareOrdinal(u.PlayerId, u.TargetId) < 0
                    ? u.PlayerId + "\n" + u.TargetId
                    : u.TargetId + "\n" + u.PlayerId;
                pairs.TryGetValue(pair, out int lane);
                pairs[pair] = lane + 1;
                arrows.Add(new AttackArrow(u.PlayerId, u.TargetId, u.PowerUp, lane));
            }

            return arrows;
        }
    }
}
