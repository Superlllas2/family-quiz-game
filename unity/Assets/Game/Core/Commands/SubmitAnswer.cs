namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// Phone message "input" (PROTOCOL.md section 3). Only kind "choice" exists so far; the other kinds
    /// arrive with the phases that use them.
    /// </summary>
    public sealed class SubmitAnswer : IGameCommand
    {
        public SubmitAnswer(string conn, string inputId, long phaseId, string kind, int? choice, long? ms)
        {
            Conn = conn;
            InputId = inputId;
            PhaseId = phaseId;
            Kind = kind;
            Choice = choice;
            Ms = ms;
        }

        public string Conn { get; }
        public string InputId { get; }
        public long PhaseId { get; }
        public string Kind { get; }
        public int? Choice { get; }
        public long? Ms { get; }
    }
}
