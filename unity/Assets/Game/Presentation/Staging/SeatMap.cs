using System.Collections.Generic;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// Which seat of the 3D set each player sits in (docs/21_staging.md, "Seats"): seat 0 is the first in fill order.
    /// A new player takes the first free seat and keeps it while in the roster, including the return to the Lobby
    /// after a game and Back to lobby. Plain C#; the seat positions themselves are markers in the studio scene.
    /// </summary>
    public sealed class SeatMap
    {
        private readonly string[] _seats;
        private readonly Dictionary<string, int> _byPlayer = new Dictionary<string, int>();

        public SeatMap(int seats)
        {
            _seats = new string[seats];
        }

        /// <summary>The changes of one Sync: players who sat down and players who left, in roster order.</summary>
        public sealed class Changes
        {
            public readonly List<string> Joined = new List<string>();
            public readonly List<string> Left = new List<string>();
        }

        /// <summary>The seat index of a player, or -1 when they have none (not in the roster, or every seat taken).</summary>
        public int SeatOf(string playerId)
        {
            return playerId != null && _byPlayer.TryGetValue(playerId, out int seat) ? seat : -1;
        }

        /// <summary>Matches the seats to the roster: frees the seats of players who left, seats the new ones.</summary>
        public Changes Sync(IEnumerable<string> roster)
        {
            var changes = new Changes();
            var present = new HashSet<string>(roster);
            for (int i = 0; i < _seats.Length; i++)
            {
                if (_seats[i] == null || present.Contains(_seats[i])) continue;
                changes.Left.Add(_seats[i]);
                _byPlayer.Remove(_seats[i]);
                _seats[i] = null;
            }

            foreach (var id in roster)
            {
                if (_byPlayer.ContainsKey(id)) continue;
                int free = System.Array.IndexOf(_seats, null);
                if (free < 0) continue;
                _seats[free] = id;
                _byPlayer[id] = free;
                changes.Joined.Add(id);
            }

            return changes;
        }
    }
}
