using System.Collections.Generic;

namespace FamilyQuiz.Core.Events
{
    /// <summary>A game started at the end of the start countdown (log event gameStarted). Scores are reset.</summary>
    public sealed class GameStarted : IGameEvent
    {
        public const string ModeStandard = "standard";

        public GameStarted(int game, int seed, string mode, IReadOnlyList<string> playerIds, string gameLang)
        {
            Game = game;
            Seed = seed;
            Mode = mode;
            PlayerIds = playerIds;
            GameLang = gameLang;
        }

        public int Game { get; }
        public int Seed { get; }
        public string Mode { get; }
        public IReadOnlyList<string> PlayerIds { get; }
        public string GameLang { get; }
    }
}
