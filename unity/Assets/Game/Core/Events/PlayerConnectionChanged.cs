namespace FamilyQuiz.Core.Events
{
    /// <summary>A seated player's phone went away or came back. The seat is held while disconnected.</summary>
    public sealed class PlayerConnectionChanged : IGameEvent
    {
        public PlayerConnectionChanged(string playerId, bool connected)
        {
            PlayerId = playerId;
            Connected = connected;
        }

        public string PlayerId { get; }
        public bool Connected { get; }
    }
}
