namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone input kind "effectCleared" (PROTOCOL.md section 3): the sabotage overlay was cleared, for telemetry.</summary>
    public sealed class ClearEffect : IGameCommand
    {
        public ClearEffect(string conn, string inputId, long phaseId, string effect, long? ms)
        {
            Conn = conn;
            InputId = inputId;
            PhaseId = phaseId;
            Effect = effect;
            Ms = ms;
        }

        public string Conn { get; }
        public string InputId { get; }
        public long PhaseId { get; }
        public string Effect { get; }

        /// <summary>Time to clear, as the phone measured it.</summary>
        public long? Ms { get; }
    }
}
