using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using FamilyQuiz.Net.Protocol;

namespace FamilyQuiz.Net
{
    /// <summary>Builds the per phone screen and player objects from the game state (read only).</summary>
    public sealed class ScreenBuilder
    {
        public const string LobbyScreenId = "lobby";

        private readonly Game _game;

        public ScreenBuilder(Game game)
        {
            _game = game;
        }

        /// <summary>The screen this player should see now. Day 1 only has the Lobby.</summary>
        public ScreenDto CurrentScreen(PlayerState player)
        {
            var state = _game.State;
            var players = new List<LobbyPlayerDto>(state.Players.Count);
            foreach (var p in state.Players)
            {
                players.Add(new LobbyPlayerDto { PlayerId = p.PlayerId, Name = p.Name, CharacterId = p.CharacterId });
            }

            return new ScreenDto
            {
                PhaseId = state.PhaseId,
                Id = LobbyScreenId,
                EndsInMs = null,
                Data = new LobbyScreenData { Players = players, CanStart = _game.CanStart }
            };
        }

        public PlayerDto Player(PlayerState player)
        {
            return new PlayerDto
            {
                PlayerId = player.PlayerId,
                Name = player.Name,
                CharacterId = player.CharacterId,
                Lang = player.Lang,
                Score = player.Score,
                Rank = Ranking.SharedRank(_game.State, player),
                Streak = player.Streak,
                HeldPowerUp = player.HeldPowerUp,
                TimerMultiplier = player.TimerMultiplier
            };
        }
    }
}
