namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A setting changed (log event settingsChanged). Source "phone" with PlayerId (lang, timerMultiplier) or
    /// "tv" with PlayerId null (Day 4 settings panel). Value is the new value as text.
    /// </summary>
    public sealed class SettingsChanged : IGameEvent
    {
        public const string SourcePhone = "phone";
        public const string SourceTv = "tv";

        /// <summary>Phone setting names (docs/15_schemas.md, "settingsChanged.source").</summary>
        public const string SettingLang = "lang";
        public const string SettingTimerMultiplier = "timerMultiplier";

        /// <summary>TV setting names (docs/15_schemas.md, "settingsChanged.source").</summary>
        public const string SettingResetSeen = "resetSeen";
        public const string SettingGameLang = "gameLang";
        public const string SettingPowerUps = "powerUps";
        public const string SettingPowerUpTypes = "powerUpTypes";
        public const string SettingTargeting = "targeting";
        public const string SettingBackToLobby = "backToLobby";
        public const string SettingFeedback = "feedback";
        public const string SettingNewGame = "newGame";

        public SettingsChanged(string source, string playerId, string setting, string value)
        {
            Source = source;
            PlayerId = playerId;
            Setting = setting;
            Value = value;
        }

        public string Source { get; }
        public string PlayerId { get; }
        public string Setting { get; }
        public string Value { get; }
    }
}
