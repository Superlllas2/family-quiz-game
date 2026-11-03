namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// "Back to lobby" in the TV settings panel (docs/02_show_flow.md): everyone returns to the Lobby from any phase with
    /// seats and characters kept; a running game is abandoned (gameEnded reason backToLobby).
    /// </summary>
    public sealed class BackToLobby : IGameCommand
    {
    }
}
