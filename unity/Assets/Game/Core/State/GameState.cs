using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.State
{
    /// <summary>The whole game state. Only Core changes it; listeners read it.</summary>
    public sealed class GameState
    {
        private readonly List<PlayerState> _players = new List<PlayerState>();
        private readonly HashSet<string> _openConns = new HashSet<string>();
        private readonly HashSet<string> _seenQuestionIds = new HashSet<string>();
        private readonly List<string> _mostCorrectIds = new List<string>();

        public GamePhase Phase { get; internal set; }

        /// <summary>Id of the current phase; every phone input names it (PROTOCOL.md section 3). Reveal and Scores share one.</summary>
        public long PhaseId { get; internal set; }

        /// <summary>Core clock time the current phase started.</summary>
        public long PhaseStartedAtMs { get; internal set; }

        /// <summary>
        /// Core clock time the current phase ends at the latest, or null for phases without a timer (Lobby, PlayAgain).
        /// In Answer: the latest personal deadline plus net.inputGraceMs (D73), moved earlier by last call.
        /// </summary>
        public long? PhaseEndsAtMs { get; internal set; }

        /// <summary>The phase budget in ms with the staging extra included (phaseStarted.budgetMs); for Answer the base window.</summary>
        public long PhaseBudgetMs { get; internal set; }

        /// <summary>The staging extra in ms included in PhaseBudgetMs (phaseStarted.stagingExtraMs).</summary>
        public long PhaseStagingExtraMs { get; internal set; }

        /// <summary>The game language for TV texts, Russian by default (D34).</summary>
        public string GameLanguage { get; internal set; } = Languages.Default;

        /// <summary>Where a cancelled StartCountdown returns: Lobby or PlayAgain.</summary>
        public GamePhase CountdownReturnPhase { get; internal set; } = GamePhase.Lobby;

        /// <summary>Games started this session, 1 based; 0 before the first.</summary>
        public int GameNumber { get; internal set; }

        /// <summary>True from the end of the start countdown until the game ends (Results over).</summary>
        public bool GameRunning { get; internal set; }

        /// <summary>Seed of the current game's random generator (logged on gameStarted).</summary>
        public int Seed { get; internal set; }

        /// <summary>Core clock time the current game started (the end of the start countdown).</summary>
        public long GameStartedAtMs { get; internal set; }

        /// <summary>The question of the current cycle, or null outside question cycles.</summary>
        public QuestionInPlay Question { get; internal set; }

        /// <summary>Questions in this game (rounds.questionsPerRound on Day 2).</summary>
        public int QuestionCount { get; internal set; }

        /// <summary>Question ids already shown. In memory on Day 2; saved to disk from Day 3.</summary>
        public IReadOnlyCollection<string> SeenQuestionIds => _seenQuestionIds;

        internal HashSet<string> MutableSeenQuestionIds => _seenQuestionIds;

        /// <summary>Set when Results starts: the winner (highest score; ties by seeded random), or null.</summary>
        public string WinnerId { get; internal set; }

        /// <summary>Set when Results starts: the players with the most correct answers ("Most correct"; ties: all of them).</summary>
        public IReadOnlyList<string> MostCorrectIds => _mostCorrectIds;

        internal List<string> MutableMostCorrectIds => _mostCorrectIds;

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
