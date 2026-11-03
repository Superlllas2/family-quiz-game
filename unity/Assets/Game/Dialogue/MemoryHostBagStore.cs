using System.Collections.Generic;

namespace FamilyQuiz.Dialogue
{
    /// <summary>Shuffle bags kept in memory only (tests, or a session that must not touch the disk).</summary>
    public sealed class MemoryHostBagStore : IHostBagStore
    {
        private List<string> _shown = new List<string>();

        /// <summary>How many times the bags were saved.</summary>
        public int Saves { get; private set; }

        /// <summary>The ids of the last save.</summary>
        public IReadOnlyList<string> Shown => _shown;

        public IReadOnlyCollection<string> Load()
        {
            return new List<string>(_shown);
        }

        public void Save(IReadOnlyCollection<string> shownIds)
        {
            _shown = new List<string>(shownIds);
            Saves++;
        }
    }
}
