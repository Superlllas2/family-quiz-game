using System;

namespace FamilyQuiz.Net
{
    /// <summary>One thing that happened on the relay socket, queued for the main thread.</summary>
    public sealed class RelayInboxItem
    {
        private RelayInboxItem(RelayInboxKind kind, string text, string roomCode, int? closeCode, string detail)
        {
            Kind = kind;
            Text = text;
            RoomCode = roomCode;
            CloseCode = closeCode;
            Detail = detail;
            ReceivedAt = DateTime.Now;
        }

        public RelayInboxKind Kind { get; }
        public string Text { get; }
        public string RoomCode { get; }
        public int? CloseCode { get; }
        public string Detail { get; }

        /// <summary>Local time when the socket thread got it (for the latency log).</summary>
        public DateTime ReceivedAt { get; }

        public static RelayInboxItem Opened(string roomCode)
        {
            return new RelayInboxItem(RelayInboxKind.SocketOpened, null, roomCode, null, null);
        }

        public static RelayInboxItem Closed(int? closeCode, string detail)
        {
            return new RelayInboxItem(RelayInboxKind.SocketClosed, null, null, closeCode, detail);
        }

        public static RelayInboxItem Message(string text)
        {
            return new RelayInboxItem(RelayInboxKind.Text, text, null, null, null);
        }
    }
}
