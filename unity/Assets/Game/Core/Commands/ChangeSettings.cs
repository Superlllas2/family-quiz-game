namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// Phone message "settings": lang and timerMultiplier (PROTOCOL.md section 3). No ack. Either may be null when
    /// missing. A new multiplier applies from the next Answer phase (D76).
    /// </summary>
    public sealed class ChangeSettings : IGameCommand
    {
        public ChangeSettings(string conn, string lang, double? timerMultiplier)
        {
            Conn = conn;
            Lang = lang;
            TimerMultiplier = timerMultiplier;
        }

        public string Conn { get; }
        public string Lang { get; }
        public double? TimerMultiplier { get; }
    }
}
