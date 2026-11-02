using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    public sealed class JoinTests
    {
        [Test]
        public void LobbyStartsWithAPhaseId()
        {
            var f = new LobbyFixture();
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(f.State.PhaseId, Is.GreaterThan(0));
        }

        [Test]
        public void JoinSeatsThePlayerWithTrimmedNameAndToken()
        {
            var f = new LobbyFixture();
            var events = f.Join("c1", "  Mama ", "en");
            Assert.That(events.OfType<PlayerJoined>().Count(), Is.EqualTo(1));
            Assert.That(events.OfType<RosterChanged>().Count(), Is.EqualTo(1));

            var p = f.State.Players.Single();
            Assert.That(p.Name, Is.EqualTo("Mama"));
            Assert.That(p.Lang, Is.EqualTo("en"));
            Assert.That(p.Conn, Is.EqualTo("c1"));
            Assert.That(p.Connected, Is.True);
            Assert.That(p.Token.Length, Is.EqualTo(ProtocolLimits.TokenLength));
            Assert.That(p.Score, Is.EqualTo(0));
            Assert.That(p.TimerMultiplier, Is.EqualTo(f.Config.Players.TimerMultiplierOptions[0]));
        }

        [Test]
        public void UnknownLanguageFallsBackToRussian()
        {
            var f = new LobbyFixture();
            f.Join("c1", "Mama", "de");
            Assert.That(f.State.Players.Single().Lang, Is.EqualTo(Languages.Russian));
        }

        [Test]
        public void TokensAreUnique()
        {
            var f = new LobbyFixture();
            for (int i = 0; i < f.Config.Players.Max; i++) f.Seat("c" + i, "Player " + i);
            var tokens = f.State.Players.Select(p => p.Token).ToList();
            Assert.That(tokens.Distinct().Count(), Is.EqualTo(tokens.Count));
        }

        [TestCase("")]
        [TestCase("Mama\U0001F600")]
        [TestCase("Ma$ma")]
        [TestCase("abcdefghijklmnopq")]
        public void BadNamesAreRefused(string name)
        {
            var f = new LobbyFixture();
            var events = f.Join("c1", name);
            Assert.That(LobbyFixture.RefusalOf(events), Is.EqualTo(RefusalReasons.BadName));
            Assert.That(f.State.Players, Is.Empty);
        }

        [TestCase("Mama", "mama")]
        [TestCase("Мама", "МАМА")]
        [TestCase("Ёлка", "ёлка")]
        [TestCase("Anna", "  ANNA  ")]
        public void DuplicateNamesIgnoringCaseAreRefused(string first, string second)
        {
            var f = new LobbyFixture();
            f.Seat("c1", first);
            var events = f.Join("c2", second);
            Assert.That(LobbyFixture.RefusalOf(events), Is.EqualTo(RefusalReasons.NameTaken));
            Assert.That(f.State.Players.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheJoinAfterPlayersMaxIsRoomFull()
        {
            var f = new LobbyFixture();
            for (int i = 0; i < f.Config.Players.Max; i++) f.Seat("c" + i, "Player " + i);
            var events = f.Join("extra", "One more");
            Assert.That(LobbyFixture.RefusalOf(events), Is.EqualTo(RefusalReasons.RoomFull));
            Assert.That(f.State.Players.Count, Is.EqualTo(f.Config.Players.Max));
        }

        [Test]
        public void EachJoinGetsTheFirstFreeCharacter()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            var c = f.Seat("c3", "C");
            Assert.That(a.CharacterId, Is.EqualTo(f.Characters[0].Id));
            Assert.That(b.CharacterId, Is.EqualTo(f.Characters[1].Id));
            Assert.That(c.CharacterId, Is.EqualTo(f.Characters[2].Id));
        }

        [Test]
        public void AFreedCharacterIsReused()
        {
            var f = new LobbyFixture();
            f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            f.Seat("c3", "C");
            f.Send(new Core.Commands.PlayerLeave("c2", "leave1"));
            var d = f.Seat("c4", "D");
            Assert.That(d.CharacterId, Is.EqualTo(b.CharacterId));
        }

        [Test]
        public void PlayerIdsAreNotReused()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            f.Send(new Core.Commands.PlayerLeave("c1", "leave1"));
            var b = f.Seat("c2", "B");
            Assert.That(b.PlayerId, Is.Not.EqualTo(a.PlayerId));
        }

        [Test]
        public void JoinAgainFromTheSameSocketReturnsTheSameSeat()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var events = f.Join("c1", "A");
            var rejoined = events.OfType<PlayerRejoined>().Single();
            Assert.That(rejoined.PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(f.State.Players.Count, Is.EqualTo(1));
        }

        [Test]
        public void EnoughPlayersCountsConnectedPlayers()
        {
            var f = new LobbyFixture();
            for (int i = 0; i < f.Config.Players.Min; i++)
            {
                Assert.That(f.Game.EnoughPlayers, Is.False);
                f.Seat("c" + i, "P " + i);
            }

            Assert.That(f.Game.EnoughPlayers, Is.True);
            f.Send(new FamilyQuiz.Core.Commands.ConnectionClosed("c0"));
            Assert.That(f.Game.EnoughPlayers, Is.False, "a dropped seat does not count");
        }
    }
}
