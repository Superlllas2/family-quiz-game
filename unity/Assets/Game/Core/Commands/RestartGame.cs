namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// New game in the pause menu (decisions/0018, D107): abandons the running game and starts a new one with the same
    /// players, through the Lobby with every connected player who has a character marked ready.
    /// </summary>
    public sealed class RestartGame : IGameCommand
    {
    }
}
