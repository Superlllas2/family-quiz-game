namespace FamilyQuiz.Net
{
    public enum RelayInboxKind
    {
        /// <summary>The TV socket is open; RoomCode says which room.</summary>
        SocketOpened,

        /// <summary>The TV socket closed or failed to open; the client tries again by itself.</summary>
        SocketClosed,

        /// <summary>A text frame from the relay.</summary>
        Text
    }
}
