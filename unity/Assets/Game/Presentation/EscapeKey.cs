namespace FamilyQuiz.Presentation
{
    /// <summary>What Escape on the laptop does (decisions/0018, D105, D106). Plain C#, so it is tested without the engine.</summary>
    public static class EscapeKey
    {
        public enum Action
        {
            /// <summary>Open the pause menu: send PauseGame.</summary>
            Pause,

            /// <summary>Close the settings panel opened from the pause menu: the menu shows again, still paused.</summary>
            CloseSettings,

            /// <summary>Close the pause menu: send ResumeGame.</summary>
            Resume
        }

        /// <param name="paused">The game is paused (the pause menu is up).</param>
        /// <param name="settingsOpen">The TV settings panel is open.</param>
        public static Action For(bool paused, bool settingsOpen)
        {
            if (!paused) return Action.Pause;
            return settingsOpen ? Action.CloseSettings : Action.Resume;
        }
    }
}
