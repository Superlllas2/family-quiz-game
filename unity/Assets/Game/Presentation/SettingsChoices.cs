using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// What the TV settings panel sends and when it shows "applies from the next game" (docs/02_show_flow.md, "TV settings
    /// panel"): the power up settings apply at the next game start, so a row whose pending value differs from the running
    /// game's gets the note. Plain C#, so it is tested without the engine.
    /// </summary>
    public static class SettingsChoices
    {
        /// <summary>The full list of types switched on after switching one type on or off, in the order of PowerUpTypes.All.</summary>
        public static List<string> WithType(IReadOnlyList<string> current, string type, bool on)
        {
            var wanted = new HashSet<string>(current ?? new string[0]);
            if (on) wanted.Add(type);
            else wanted.Remove(type);
            var result = new List<string>();
            foreach (var t in PowerUpTypes.All)
            {
                if (wanted.Contains(t)) result.Add(t);
            }

            return result;
        }

        /// <summary>Only while a game runs: before a game, the pending values are what the next game takes anyway.</summary>
        public static bool PowerUpsDiffer(GameState state)
        {
            return state.GameRunning && state.PendingSettings.PowerUpsEnabled != state.Settings.PowerUpsEnabled;
        }

        public static bool TypeDiffers(GameState state, string type)
        {
            return state.GameRunning && state.PendingSettings.IsTypeOn(type) != state.Settings.IsTypeOn(type);
        }

        public static bool TargetingDiffers(GameState state)
        {
            return state.GameRunning && state.PendingSettings.Targeting != state.Settings.Targeting;
        }
    }
}
