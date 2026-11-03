namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// Phone message "input" (PROTOCOL.md section 3). The kind decides which fields count: vote (Door), choice (Choice,
    /// Ms), value (Value, Ms), trueFalse (Answer, Ms) and prediction (PlayerId). Missing fields are null.
    /// </summary>
    public sealed class SubmitAnswer : IGameCommand
    {
        public SubmitAnswer(string conn, string inputId, long phaseId, string kind, int? choice, long? ms,
            int? door = null, double? value = null, bool? answer = null, string playerId = null)
        {
            Conn = conn;
            InputId = inputId;
            PhaseId = phaseId;
            Kind = kind;
            Choice = choice;
            Ms = ms;
            Door = door;
            Value = value;
            Answer = answer;
            PlayerId = playerId;
        }

        public string Conn { get; }
        public string InputId { get; }
        public long PhaseId { get; }
        public string Kind { get; }
        public int? Choice { get; }
        public long? Ms { get; }

        /// <summary>vote: the door, 0 to 3.</summary>
        public int? Door { get; }

        /// <summary>value: the closest value guess.</summary>
        public double? Value { get; }

        /// <summary>trueFalse: true or false.</summary>
        public bool? Answer { get; }

        /// <summary>prediction: the playerId the player thinks wins.</summary>
        public string PlayerId { get; }
    }
}
