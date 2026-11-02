namespace FamilyQuiz.Core.State
{
    /// <summary>One applied input of a player.</summary>
    public sealed class AnswerRecord
    {
        public AnswerRecord(string inputId, long phaseId, string kind, int? choice, long? ms, long receivedAtMs)
        {
            InputId = inputId;
            PhaseId = phaseId;
            Kind = kind;
            Choice = choice;
            Ms = ms;
            ReceivedAtMs = receivedAtMs;
        }

        public string InputId { get; }
        public long PhaseId { get; }
        public string Kind { get; }
        public int? Choice { get; }

        /// <summary>The phone's ms from the options appearing to the tap, as sent.</summary>
        public long? Ms { get; }

        /// <summary>Core clock time when the TV app applied it.</summary>
        public long ReceivedAtMs { get; }
    }
}
