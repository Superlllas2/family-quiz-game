namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// "Reset seen questions" from the TV settings panel (docs/02_show_flow.md, "TV settings panel"): clears the seen
    /// question and race statement lists at once. Logged as settingsChanged with source tv and setting resetSeen.
    /// </summary>
    public sealed class ResetSeen : IGameCommand
    {
    }
}
