namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// Something in this player's "player" object changed (score, rank, streak, character, language, timer multiplier).
    /// Net sends that player a "player" message. Ranks of others may change too; Core raises one per affected player.
    /// </summary>
    public sealed class PlayerUpdated : IGameEvent
    {
        public PlayerUpdated(string playerId)
        {
            PlayerId = playerId;
        }

        public string PlayerId { get; }
    }
}
