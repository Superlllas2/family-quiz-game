using System;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// The relay URLs (PROTOCOL.md section 2) from local.json "relayHost". A bare host means wss and https;
    /// a value with "://" keeps its scheme (http becomes ws, https becomes wss), for example http://localhost:8787.
    /// </summary>
    public sealed class RelayEndpoint
    {
        private readonly string _socketBase;
        private readonly string _pageBase;

        private RelayEndpoint(string socketBase, string pageBase)
        {
            _socketBase = socketBase;
            _pageBase = pageBase;
        }

        public static RelayEndpoint Parse(string relayHost)
        {
            if (string.IsNullOrWhiteSpace(relayHost)) throw new FormatException("relayHost is empty");
            var value = relayHost.Trim().TrimEnd('/');
            string secure = "s";
            var schemeEnd = value.IndexOf("://", StringComparison.Ordinal);
            if (schemeEnd >= 0)
            {
                var scheme = value.Substring(0, schemeEnd).ToLowerInvariant();
                value = value.Substring(schemeEnd + 3);
                if (scheme == "http" || scheme == "ws") secure = "";
                else if (scheme != "https" && scheme != "wss") throw new FormatException("relayHost has an unknown scheme: " + scheme);
            }

            if (value.Length == 0) throw new FormatException("relayHost has no host");
            return new RelayEndpoint("ws" + secure + "://" + value, "http" + secure + "://" + value);
        }

        public Uri TvSocket(RoomCredentials room)
        {
            return new Uri(_socketBase + "/ws/tv?room=" + Uri.EscapeDataString(room.Code)
                           + "&secret=" + Uri.EscapeDataString(room.Secret));
        }

        /// <summary>The phone page link the QR code encodes.</summary>
        public string JoinUrl(string roomCode)
        {
            return _pageBase + "/?room=" + Uri.EscapeDataString(roomCode);
        }
    }
}
