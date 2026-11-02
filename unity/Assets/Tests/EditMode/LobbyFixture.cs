using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>A game in the Lobby on a fake clock, with helpers that send commands like the phones would.</summary>
    public sealed class LobbyFixture
    {
        public LobbyFixture()
        {
            Config = TestContent.Config();
            Characters = TestContent.Characters(Config);
            Clock = new ManualClock();
            Game = new Game(new GameContent(Config, Characters, new List<QuestionDef>(), null), Clock, new SeededRandom(1234), () => 42);
            Game.Start();
        }

        public GameConfig Config { get; }
        public IReadOnlyList<CharacterDef> Characters { get; }
        public ManualClock Clock { get; }
        public Game Game { get; }
        public GameState State => Game.State;

        public IReadOnlyList<IGameEvent> Send(IGameCommand command)
        {
            return Game.Handle(command);
        }

        public IReadOnlyList<IGameEvent> Join(string conn, string name, string lang = "ru")
        {
            Game.Handle(new ConnectionOpened(conn));
            return Game.Handle(new PlayerJoin(conn, name, lang));
        }

        /// <summary>Joins and asserts the join worked; returns the new seat.</summary>
        public PlayerState Seat(string conn, string name)
        {
            var events = Join(conn, name);
            var joined = events.OfType<PlayerJoined>().SingleOrDefault();
            Assert.That(joined, Is.Not.Null, "join of " + name + " failed: " + Describe(events));
            return State.FindById(joined.PlayerId);
        }

        public static string RefusalOf(IReadOnlyList<IGameEvent> events)
        {
            return events.OfType<JoinRefused>().Select(r => r.Reason).SingleOrDefault();
        }

        public static string Describe(IReadOnlyList<IGameEvent> events)
        {
            return string.Join(", ", events.Select(e => e is JoinRefused r ? "refused " + r.Reason : e.GetType().Name));
        }
    }
}
