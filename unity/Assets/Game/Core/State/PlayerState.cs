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
        public double TimerMultiplier { get; internal set; }

        /// <summary>True once the player picked a character on the pickCharacter screen; then they see the lobby.</summary>
        public bool CharacterPicked { get; internal set; }

        /// <summary>Correct answers in rounds in the current game ("results.correctCount", "Most correct").</summary>
        public int CorrectCount { get; internal set; }

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
