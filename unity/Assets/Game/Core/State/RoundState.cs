using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.State
{
    /// <summary>The current round: its topic vote and, once the vote is over, the chosen topic (docs/02_show_flow.md).</summary>
    public sealed class RoundState
    {
        private readonly Dictionary<string, int> _votes = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _points = new Dictionary<string, int>();

        internal RoundState(int index, double multiplier, IReadOnlyList<TopicDef> doors)
        {
            Index = index;
            Multiplier = multiplier;
            Doors = doors;
        }

        /// <summary>0 based round index in the game.</summary>
        public int Index { get; }

        /// <summary>rounds.multipliers[Index].</summary>
        public double Multiplier { get; }

        /// <summary>The topic doors in door order; the last one is the mixed door.</summary>
        public IReadOnlyList<TopicDef> Doors { get; }

        /// <summary>playerId to the door (0 based) they voted for.</summary>
        public IReadOnlyDictionary<string, int> Votes => _votes;

        internal Dictionary<string, int> MutableVotes => _votes;

        /// <summary>The door that won the vote, or null while the vote runs.</summary>
        public int? ChosenDoor { get; internal set; }

        /// <summary>The chosen door's topic, or null while the vote runs.</summary>
        public TopicDef Topic => ChosenDoor.HasValue ? Doors[ChosenDoor.Value] : null;

        /// <summary>Points each player earned in this round so far (for the round winner on RoundEnd).</summary>
        public IReadOnlyDictionary<string, int> Points => _points;

        internal Dictionary<string, int> MutablePoints => _points;

        /// <summary>Votes per door, in door order.</summary>
        public int[] VoteCounts()
        {
            var counts = new int[Doors.Count];
            foreach (var door in _votes.Values)
            {
                if (door >= 0 && door < counts.Length) counts[door]++;
            }

            return counts;
        }
    }
}
