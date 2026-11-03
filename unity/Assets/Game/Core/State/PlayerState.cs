using System.Collections.Generic;

namespace FamilyQuiz.Core.State
{
    /// <summary>One seat. Only Core changes it; listeners read it.</summary>
    public sealed class PlayerState
    {
        private readonly Dictionary<string, AckRecord> _acks = new Dictionary<string, AckRecord>();

        internal PlayerState(string playerId, string name, string lang, int characterId, string token, double timerMultiplier)
        {
            PlayerId = playerId;
            Name = name;
            Lang = lang;
            CharacterId = characterId;
            Token = token;
            TimerMultiplier = timerMultiplier;
        }

        public string PlayerId { get; }
        public string Name { get; }
        public string Lang { get; internal set; }
        public int CharacterId { get; internal set; }
        public string Token { get; }

        /// <summary>The relay conn of the seat's newest socket, or null.</summary>
        public string Conn { get; internal set; }

        public bool Connected { get; internal set; }

        /// <summary>Core clock time when the player last disconnected.</summary>
        public long DisconnectedAtMs { get; internal set; }

        public int Score { get; internal set; }
        public int Streak { get; internal set; }
        public string HeldPowerUp { get; internal set; }

        /// <summary>Power ups used on this player that land at the start of the next Answer phase (docs/03_rules.md, "Caps").</summary>
        public IReadOnlyList<PendingEffect> PendingEffects => _pendingEffects;

        internal List<PendingEffect> MutablePendingEffects => _pendingEffects;

        private readonly List<PendingEffect> _pendingEffects = new List<PendingEffect>();

        /// <summary>The game question number (QuestionInPlay.Number) a power up last landed on, for immunity; null if never hit this game.</summary>
        public int? LastHitQuestion { get; internal set; }

        /// <summary>The phaseId of the pick window (Reveal and Scores) in which this player used a power up, else 0.</summary>
        public long UsedPowerUpInPhaseId { get; internal set; }

        /// <summary>The power up this player used in that pick window and its target, for the waiting screen.</summary>
        public string UsedPowerUp { get; internal set; }

        public string UsedPowerUpTarget { get; internal set; }
        public double TimerMultiplier { get; internal set; }

        /// <summary>True once the player picked a character on the pickCharacter screen; then they see the lobby.</summary>
        public bool CharacterPicked { get; internal set; }

        /// <summary>Tapped Ready in the Lobby (D76). Cleared when a game starts.</summary>
        public bool Ready { get; internal set; }

        /// <summary>Correct answers in rounds in the current game ("results.correctCount", "Most correct").</summary>
        public int CorrectCount { get; internal set; }

        /// <summary>What the player did in the current game, for the funny tags on Results (D86).</summary>
        public PlayerStats Stats { get; internal set; } = new PlayerStats();

        /// <summary>This player's part of the current question, or null outside question cycles.</summary>
        public PlayerTurn Turn { get; internal set; }

        internal bool TryGetAck(string inputId, out AckRecord ack)
        {
            return _acks.TryGetValue(inputId, out ack);
        }

        internal void RememberAck(string inputId, AckRecord ack)
        {
            _acks[inputId] = ack;
        }
    }
}
