using System.Collections.Generic;

namespace FamilyQuiz.Core.Events
{
    /// <summary>A game started at the end of the start countdown (log event gameStarted). Scores are reset.</summary>
    public sealed class GameStarted : IGameEvent
    {
        public const string ModeStandard = "standard";

        public GameStarted(int game, int seed, string mode, IReadOnlyList<string> playerIds, string gameLang,
            IReadOnlyList<string> powerUps = null, string targeting = null, string bank = null)
        {
            Bank = bank;
            PowerUps = powerUps ?? new string[0];
            Targeting = targeting;
            Game = game;
            Seed = seed;
            Mode = mode;
            PlayerIds = playerIds;
            GameLang = gameLang;
        }

        /// <summary>The question bank of this game (decisions/0020), full or curated.</summary>
        public string Bank { get; }

        public int Game { get; }
        public int Seed { get; }
        public string Mode { get; }
        public IReadOnlyList<string> PlayerIds { get; }
        public string GameLang { get; }

        /// <summary>The power ups this game draws from (empty with power ups off; log gameStarted.settings.powerUps).</summary>
        public IReadOnlyList<string> PowerUps { get; }

        /// <summary>free or aimUp for this game.</summary>
        public string Targeting { get; }
    }
}
