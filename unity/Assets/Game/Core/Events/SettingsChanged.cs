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
