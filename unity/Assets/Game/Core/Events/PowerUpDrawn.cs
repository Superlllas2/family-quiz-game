namespace FamilyQuiz.Core.Events
{
    /// <summary>A player drew a power up after a correct answer at Reveal (log event powerUpDrawn).</summary>
    public sealed class PowerUpDrawn : IGameEvent
    {
        public PowerUpDrawn(string playerId, string powerUp, int rank, double chance)
        {
            PlayerId = playerId;
            PowerUp = powerUp;
            Rank = rank;
            Chance = chance;
        }

        public string PlayerId { get; }
        public string PowerUp { get; }

        /// <summary>The shared rank before the question, which set the chance.</summary>
        public int Rank { get; }

        public double Chance { get; }
    }
}
