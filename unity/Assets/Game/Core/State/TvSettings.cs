using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.State
{
    /// <summary>
    /// The power up settings of the TV settings panel (docs/02_show_flow.md, "TV settings panel"): power ups on or off,
    /// each type on or off, and targeting. The panel changes the pending copy; a game uses the copy taken when it started.
    /// </summary>
    public sealed class TvSettings
    {
        private readonly List<string> _types = new List<string>();

        public bool PowerUpsEnabled { get; internal set; }

        /// <summary>The types switched on, in the order of <see cref="PowerUpTypes.All"/>.</summary>
        public IReadOnlyList<string> PowerUpTypes => _types;

        /// <summary>free or aimUp (<see cref="TargetingModes"/>).</summary>
        public string Targeting { get; internal set; } = TargetingModes.Free;

        /// <summary>The question bank (decisions/0020, D116): full or curated. A game plays the bank it started with.</summary>
        public string Bank { get; internal set; } = BankIds.Full;

        /// <summary>The types a draw picks from: the switched on types, or none when power ups are off.</summary>
        public IReadOnlyList<string> DrawableTypes => PowerUpsEnabled ? (IReadOnlyList<string>)_types : new string[0];

        /// <summary>The defaults from config.json (powerUps.enabled, powerUps.types, powerUps.targeting).</summary>
        public static TvSettings FromConfig(PowerUpsConfig config, string bank = BankIds.Full)
        {
            var s = new TvSettings { PowerUpsEnabled = config.Enabled, Targeting = config.Targeting, Bank = bank };
            s.SetTypes(config.Types);
            return s;
        }

        public TvSettings Clone()
        {
            var s = new TvSettings { PowerUpsEnabled = PowerUpsEnabled, Targeting = Targeting, Bank = Bank };
            s.SetTypes(_types);
            return s;
        }

        public bool IsTypeOn(string type)
        {
            return _types.Contains(type);
        }

        /// <summary>Keeps the known types of the list, in the order of <see cref="Content.PowerUpTypes.All"/>.</summary>
        internal void SetTypes(IEnumerable<string> types)
        {
            var wanted = new HashSet<string>(types ?? new string[0]);
            _types.Clear();
            foreach (var t in Content.PowerUpTypes.All)
            {
                if (wanted.Contains(t)) _types.Add(t);
            }
        }
    }
}
