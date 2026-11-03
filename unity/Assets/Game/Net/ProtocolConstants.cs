namespace FamilyQuiz.Net
{
    /// <summary>Values fixed by docs/PROTOCOL.md.</summary>
    public static class ProtocolConstants
    {
        /// <summary>PROTOCOL.md section 7: every message carries "v": 2 (version 2 since D88).</summary>
        public const int Version = 2;

        /// <summary>PROTOCOL.md section 2, close codes: wrong secret for this room (sent to the TV app).</summary>
        public const int CloseWrongSecret = 4001;

        /// <summary>PROTOCOL.md section 2, close codes: closed by the TV app, a newer socket took this seat.</summary>
        public const int CloseReplacedByNewerSocket = 4002;

        /// <summary>PROTOCOL.md section 2: room codes are 4 letters from this alphabet (no I or O).</summary>
        public const string RoomCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";

        public const int RoomCodeLength = 4;

        /// <summary>PROTOCOL.md section 2: tvSecret is a random 32 character string.</summary>
        public const int TvSecretLength = 32;

        public const string TvSecretAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>The "to" value that sends a message to every phone in the room.</summary>
        public const string ToAll = "all";
    }
}
