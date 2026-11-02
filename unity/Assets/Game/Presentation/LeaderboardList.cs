using System.Collections.Generic;
using FamilyQuiz.Core.State;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A column of leaderboard rows inside its rect. Rows are placed by index from the top, with the step shrinking
    /// when many players do not fit. Used by the Scores and Results views.
    /// </summary>
    public sealed class LeaderboardList : MonoBehaviour
    {
        private const float MaxStep = 112f;
        private const float RowGap = 14f;
        private const float FallbackHeight = 860f;

        private readonly List<LeaderboardRow> _rows = new List<LeaderboardRow>();

        public static LeaderboardList Create(Transform parent)
        {
            var rect = UiBuilder.Rect("Rows", parent);
            return rect.gameObject.AddComponent<LeaderboardList>();
        }

        public RectTransform Rect => (RectTransform)transform;

        public IReadOnlyList<LeaderboardRow> Rows => _rows;

        /// <summary>Destroys the rows and makes one per player, in the given order.</summary>
        public void Rebuild(IReadOnlyList<PlayerState> players)
        {
            foreach (var row in _rows) Destroy(row.gameObject);
            _rows.Clear();
            foreach (var p in players) _rows.Add(LeaderboardRow.Create(transform, p.PlayerId));
        }

        public bool Matches(IReadOnlyList<PlayerState> players)
        {
            if (players.Count != _rows.Count) return false;
            foreach (var p in players)
            {
                if (Find(p.PlayerId) == null) return false;
            }

            return true;
        }

        public LeaderboardRow Find(string playerId)
        {
            foreach (var row in _rows)
            {
                if (row.PlayerId == playerId) return row;
            }

            return null;
        }

        /// <summary>Distance from the top of the list to the row at this index.</summary>
        public float Y(int index)
        {
            return index * Step();
        }

        public float RowHeight()
        {
            return Step() - RowGap;
        }

        private float Step()
        {
            float height = Rect.rect.height;
            if (height <= 0f) height = FallbackHeight;
            int count = Mathf.Max(1, _rows.Count);
            return Mathf.Min(MaxStep, height / count);
        }
    }
}
