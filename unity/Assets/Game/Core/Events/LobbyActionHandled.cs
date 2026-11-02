namespace FamilyQuiz.Core.Events
{
    /// <summary>A "lobby" message from a seated player was handled (log event lobbyAction).</summary>
    public sealed class LobbyActionHandled : IGameEvent
    {
        public LobbyActionHandled(string playerId, string action, bool ok)
        {
            PlayerId = playerId;
            Action = action;
            Ok = ok;
        }

        public string PlayerId { get; }
        public string Action { get; }
        public bool Ok { get; }
    }
}
