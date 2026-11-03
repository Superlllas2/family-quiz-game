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
        private readonly HashSet<string> _seenRaceIds = new HashSet<string>();
        private readonly List<string> _mostCorrectIds = new List<string>();
        private readonly List<string> _bestComebackIds = new List<string>();
        private readonly Dictionary<string, int> _ranksAfterRound2 = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _predictions = new Dictionary<string, string>();

        public GamePhase Phase { get; internal set; }

        /// <summary>Id of the current phase; every phone input names it (PROTOCOL.md section 3). Reveal and Scores share one.</summary>
        public long PhaseId { get; internal set; }

        /// <summary>Core clock time the current phase started.</summary>
        public long PhaseStartedAtMs { get; internal set; }

        /// <summary>
        /// Core clock time the current phase ends at the latest, or null for phases without a timer (Lobby).
        /// In Answer: the latest personal deadline plus net.inputGraceMs (D73), moved earlier by last call.
        /// </summary>
        public long? PhaseEndsAtMs { get; internal set; }

        /// <summary>The phase budget in ms with the staging extra included (phaseStarted.budgetMs); for Answer the base window.</summary>
        public long PhaseBudgetMs { get; internal set; }

        /// <summary>The staging extra in ms included in PhaseBudgetMs (phaseStarted.stagingExtraMs).</summary>
        public long PhaseStagingExtraMs { get; internal set; }

        /// <summary>The game language for TV texts, Russian by default (D34).</summary>
        public string GameLanguage { get; internal set; } = Languages.Default;


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

        /// <summary>Questions per round (rounds.questionsPerRound).</summary>
        public int QuestionCount { get; internal set; }

        /// <summary>Rounds in this game (the round segments of the show script).</summary>
        public int RoundCount { get; internal set; }

        /// <summary>The current round, or null outside rounds.</summary>
        public RoundState Round { get; internal set; }

        /// <summary>The final race of this game, or null before it starts.</summary>
        public RaceState Race { get; internal set; }

        /// <summary>Question ids already shown, across games and app restarts until "Reset seen questions" (SeenChanged).</summary>
        public IReadOnlyCollection<string> SeenQuestionIds => _seenQuestionIds;

        internal HashSet<string> MutableSeenQuestionIds => _seenQuestionIds;

        /// <summary>Race statement ids already shown, kept like <see cref="SeenQuestionIds"/>.</summary>
        public IReadOnlyCollection<string> SeenRaceIds => _seenRaceIds;

        internal HashSet<string> MutableSeenRaceIds => _seenRaceIds;

        /// <summary>Set when Results starts: the race winner (docs/03_rules.md, "Winner and ties"), or null.</summary>
        public string WinnerId { get; internal set; }

        /// <summary>
        /// Set when Results starts: every player in final order, winner first (the race order; without a race, by
        /// score). results.place and the top 3 come from it.
        /// </summary>
        public IReadOnlyList<string> Placement => _placement;

        internal List<string> MutablePlacement => _placement;

        private readonly List<string> _placement = new List<string>();

        /// <summary>Set when Results starts: the players with the most correct answers ("Most correct"; ties: all of them).</summary>
        public IReadOnlyList<string> MostCorrectIds => _mostCorrectIds;

        internal List<string> MutableMostCorrectIds => _mostCorrectIds;

        /// <summary>Set when Results starts: the players with the biggest rank gain from the end of round 2 ("Best comeback").</summary>
        public IReadOnlyList<string> BestComebackIds => _bestComebackIds;

        internal List<string> MutableBestComebackIds => _bestComebackIds;

        /// <summary>Shared rank of each player when round 2 ended (for "Best comeback").</summary>
        public IReadOnlyDictionary<string, int> RanksAfterRound2 => _ranksAfterRound2;

        internal Dictionary<string, int> MutableRanksAfterRound2 => _ranksAfterRound2;

        /// <summary>Intro predictions "Who wins tonight?": playerId to the playerId they tapped.</summary>
        public IReadOnlyDictionary<string, string> Predictions => _predictions;

        internal Dictionary<string, string> MutablePredictions => _predictions;

        /// <summary>Seated players in join order.</summary>
        public IReadOnlyList<PlayerState> Players => _players;

        internal List<PlayerState> MutablePlayers => _players;

        internal HashSet<string> OpenConns => _openConns;

        public bool IsOpen(string conn)
        {
            return conn != null && _openConns.Contains(conn);
        }

        /// <summary>Seated players with an open socket.</summary>
        public int ConnectedPlayerCount()
        {
            int count = 0;
            foreach (var p in _players)
            {
                if (p.Connected) count++;
            }

            return count;
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
