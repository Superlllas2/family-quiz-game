using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// A seen list in the order things were seen, oldest first (docs/02_show_flow.md, "Seen questions"; D155). Adding an
    /// id that is already in the list moves it to the end, so the first one is always the least recently seen. Contains is
    /// O(1).
    /// </summary>
    public sealed class SeenList : ICollection<string>, IReadOnlyCollection<string>
    {
        private readonly Dictionary<string, long> _stamp = new Dictionary<string, long>();
        private long _next;

        public int Count => _stamp.Count;

        public bool IsReadOnly => false;

        /// <summary>Marks the id as seen now: added at the end, or moved there when it was seen before.</summary>
        public void Add(string id)
        {
            if (id == null) return;
            _stamp[id] = _next++;
        }

        public bool Contains(string id)
        {
            return id != null && _stamp.ContainsKey(id);
        }

        /// <summary>When the id was last seen, as an order number (smaller is longer ago), or null when never seen.</summary>
        public long? LastSeen(string id)
        {
            return id != null && _stamp.TryGetValue(id, out var stamp) ? stamp : (long?)null;
        }

        public bool Remove(string id)
        {
            return id != null && _stamp.Remove(id);
        }

        public void Clear()
        {
            _stamp.Clear();
            _next = 0;
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            foreach (var id in this) array[arrayIndex++] = id;
        }

        /// <summary>Oldest first.</summary>
        public IEnumerator<string> GetEnumerator()
        {
            return _stamp.OrderBy(p => p.Value).Select(p => p.Key).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// When an item was last seen, for picking the least recently seen: from a <see cref="SeenList"/> its order number,
        /// from any other collection 0 for every seen id (all equally old).
        /// </summary>
        public static long? LastSeenIn(IReadOnlyCollection<string> seen, string id)
        {
            if (seen is SeenList list) return list.LastSeen(id);
            if (seen is ICollection<string> collection) return collection.Contains(id) ? 0 : (long?)null;
            return seen.Contains(id) ? 0 : (long?)null;
        }
    }
}
