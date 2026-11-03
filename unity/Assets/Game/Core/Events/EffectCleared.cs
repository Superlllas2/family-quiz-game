namespace FamilyQuiz.Core.Events
{
    /// <summary>A target cleared the sabotage overlay on their phone (input kind effectCleared; log event effectCleared).</summary>
    public sealed class EffectCleared : IGameEvent
    {
        public EffectCleared(string playerId, string effect, long ms)
        {
            PlayerId = playerId;
            Effect = effect;
            Ms = ms;
        }

        public string PlayerId { get; }
        public string Effect { get; }

        /// <summary>Time to clear as the phone measured it.</summary>
        public long Ms { get; }
    }
}
