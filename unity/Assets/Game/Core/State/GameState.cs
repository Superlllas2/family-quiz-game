using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.State
{
    /// <summary>The whole game state. Only Core changes it; listeners read it.</summary>
    public sealed class GameState
    {
        private readonly List<PlayerState> _players = new List<PlayerState>();
        private readonly HashSet<string> _openConns = new HashSet<string>();

        public GamePhase Phase { get; internal set; }

        /// <summary>Id of the current phase; every phone input names it (PROTOCOL.md section 3).</summary>
        public long PhaseId { get; internal set; }

        /// <summary>The game language for TV texts, Russian by default (D34).</summary>
        public string GameLanguage { get; internal set; } = Languages.Default;

        /// <summary>Seated players in join order.</summary>
        public IReadOnlyList<PlayerState> Players => _players;

        internal List<PlayerState> MutablePlayers => _players;

        internal HashSet<string> OpenConns => _openConns;

        public bool IsOpen(string conn)
        {
            return conn != null && _openConns.Contains(conn);
        }

        public PlayerState FindById(string playerId)
        {
            return _players.Find(p => p.PlayerId == playerId);
        }

        public PlayerState FindByConn(string conn)
        {
            return conn == null ? null : _players.Find(p => p.Conn == conn);
        }

        public PlayerState FindByToken(string token)
        {
            return string.IsNullOrEmpty(token) ? null : _players.Find(p => string.Equals(p.Token, token, StringComparison.Ordinal));
        }
    }
}
