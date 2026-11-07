using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// The TV settings panel (docs/02_show_flow.md, "TV settings panel"): the game language at once; power ups on or off,
    /// each type on or off and targeting into the pending settings, which the next game start copies. Every change is
    /// logged as settingsChanged with source tv; an unknown value or no change does nothing.
    /// </summary>
    public sealed class TvSettingsRules
    {
        private readonly GameState _state;
        public TvSettingsRules(GameState state)
        {
            _state = state;
        }

        public void Change(ChangeTvSettings cmd, List<IGameEvent> output)
        {
            var pending = _state.PendingSettings;
            if (Languages.IsSupported(cmd.GameLang) && cmd.GameLang != _state.GameLanguage)
            {
                _state.GameLanguage = cmd.GameLang;
                Changed(SettingsChanged.SettingGameLang, cmd.GameLang, output);
            }

            if (cmd.PowerUpsEnabled.HasValue && cmd.PowerUpsEnabled.Value != pending.PowerUpsEnabled)
            {
                pending.PowerUpsEnabled = cmd.PowerUpsEnabled.Value;
                Changed(SettingsChanged.SettingPowerUps, cmd.PowerUpsEnabled.Value ? "true" : "false", output);
            }

            if (cmd.PowerUpTypes != null)
            {
                var before = string.Join(",", pending.PowerUpTypes);
                pending.SetTypes(cmd.PowerUpTypes);
                var after = string.Join(",", pending.PowerUpTypes);
                if (after != before) Changed(SettingsChanged.SettingPowerUpTypes, after, output);
            }

            if (cmd.FeedbackEnabled.HasValue && cmd.FeedbackEnabled.Value != _state.FeedbackEnabled)
            {
                _state.FeedbackEnabled = cmd.FeedbackEnabled.Value;
                Changed(SettingsChanged.SettingFeedback, cmd.FeedbackEnabled.Value ? "true" : "false", output);
            }

            if (TargetingModes.IsValid(cmd.Targeting) && cmd.Targeting != pending.Targeting)
            {
                pending.Targeting = cmd.Targeting;
                Changed(SettingsChanged.SettingTargeting, cmd.Targeting, output);
            }
        }

        private static void Changed(string setting, string value, List<IGameEvent> output)
        {
            output.Add(new SettingsChanged(SettingsChanged.SourceTv, null, setting, value));
        }
    }
}
