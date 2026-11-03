namespace FamilyQuiz.Core.Commands
{
    /// <summary>Phone input kind "powerUp" (PROTOCOL.md section 3): use the held power up on a target, in Reveal or Scores.</summary>
    public sealed class UsePowerUp : IGameCommand
    {
        public UsePowerUp(string conn, string inputId, long phaseId, string powerUp, string target)
        {
            Conn = conn;
            InputId = inputId;
            PhaseId = phaseId;
            PowerUp = powerUp;
            Target = target;
        }

        public string Conn { get; }
        public string InputId { get; }
        public long PhaseId { get; }
        public string PowerUp { get; }

        /// <summary>The target's playerId.</summary>
        public string Target { get; }
    }
}
