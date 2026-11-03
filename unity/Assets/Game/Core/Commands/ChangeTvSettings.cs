using System.Collections.Generic;

namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// The TV settings panel (docs/02_show_flow.md, "TV settings panel"). Null fields stay as they are. The game language
    /// applies at once; power ups on or off, the types and targeting apply from the next game start.
    /// </summary>
    public sealed class ChangeTvSettings : IGameCommand
    {
        public ChangeTvSettings(string gameLang = null, bool? powerUpsEnabled = null, IReadOnlyList<string> powerUpTypes = null,
            string targeting = null)
        {
            GameLang = gameLang;
            PowerUpsEnabled = powerUpsEnabled;
            PowerUpTypes = powerUpTypes;
            Targeting = targeting;
        }

        public string GameLang { get; }
        public bool? PowerUpsEnabled { get; }

        /// <summary>The full list of types switched on.</summary>
        public IReadOnlyList<string> PowerUpTypes { get; }

        public string Targeting { get; }
    }
}
