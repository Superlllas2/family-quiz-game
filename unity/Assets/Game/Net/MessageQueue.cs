using System.Collections.Concurrent;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// Thread safe queue from the socket thread to the main thread. The main thread drains it once per frame
    /// (CLAUDE.md, "Network messages are queued and handled once per frame on Unity's main thread").
    /// </summary>
    public sealed class MessageQueue
    {
        private readonly ConcurrentQueue<RelayInboxItem> _items = new ConcurrentQueue<RelayInboxItem>();

        public void Enqueue(RelayInboxItem item)
        {
            _items.Enqueue(item);
        }

        public bool TryDequeue(out RelayInboxItem item)
        {
            return _items.TryDequeue(out item);
        }
    }
}
