namespace FamilyQuiz.Core.Events
{
    /// <summary>A player's ready mark in the Lobby changed (D76). Net refreshes the lobby screens; the TV redraws the marks.</summary>
    public sealed class ReadyChanged : IGameEvent
    {
        public ReadyChanged(string playerId, bool ready)
        {
            PlayerId = playerId;
            Ready = ready;
        }

        public string PlayerId { get; }
        public bool Ready { get; }
    }
}
