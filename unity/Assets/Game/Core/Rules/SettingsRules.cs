using System;
using System.Collections.Generic;
using System.Globalization;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// The phone "settings" message (PROTOCOL.md section 3): lang (ru or en) and timerMultiplier (one of
    /// players.timerMultiplierOptions). Invalid or missing values are ignored. A new multiplier applies from the next
    /// Answer phase: the deadline is fixed when Answer starts.
    /// </summary>
    public sealed class SettingsRules
    {
        private const double MultiplierEpsilon = 1e-9;

        private readonly GameState _state;
        private readonly GameConfig _config;

        public SettingsRules(GameState state, GameConfig config)
        {
            _state = state;
            _config = config;
        }

        public void Change(ChangeSettings cmd, List<IGameEvent> output)
        {
            var player = _state.FindByConn(cmd.Conn);
            if (player == null) return;

            bool changed = false;
            if (Languages.IsSupported(cmd.Lang) && cmd.Lang != player.Lang)
            {
                player.Lang = cmd.Lang;
                output.Add(new SettingsChanged(SettingsChanged.SourcePhone, player.PlayerId, SettingsChanged.SettingLang, cmd.Lang));
                changed = true;
            }

            if (cmd.TimerMultiplier.HasValue && TryOption(cmd.TimerMultiplier.Value, out var option)
                && Math.Abs(option - player.TimerMultiplier) > MultiplierEpsilon)
            {
                player.TimerMultiplier = option;
                output.Add(new SettingsChanged(SettingsChanged.SourcePhone, player.PlayerId, SettingsChanged.SettingTimerMultiplier,
                    option.ToString(CultureInfo.InvariantCulture)));
                changed = true;
            }

            if (changed) output.Add(new PlayerUpdated(player.PlayerId));
        }

        private bool TryOption(double value, out double option)
        {
            foreach (var o in _config.Players.TimerMultiplierOptions)
            {
                if (Math.Abs(o - value) <= MultiplierEpsilon)
                {
                    option = o;
                    return true;
                }
            }

            option = 0;
            return false;
        }
    }
}
