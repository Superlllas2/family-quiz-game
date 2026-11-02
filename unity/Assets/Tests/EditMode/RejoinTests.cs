using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>PROTOCOL.md section 5, docs/18_edge_cases.md (phone locks or reloads, two tabs).</summary>
    public sealed class RejoinTests
    {
        [Test]
        public void RejoinWithAValidTokenRestoresTheSameSeat()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            f.Send(new ConnectionClosed("c1"));
            Assert.That(a.Connected, Is.False);

            f.Clock.Advance(60000);
            f.Send(new ConnectionOpened("c2"));
            var events = f.Send(new PlayerRejoin("c2", a.Token));

            var rejoined = events.OfType<PlayerRejoined>().Single();
            Assert.That(rejoined.PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(rejoined.Conn, Is.EqualTo("c2"));
            Assert.That(rejoined.AwayMs, Is.EqualTo(60000));
            Assert.That(rejoined.IsRefresh, Is.False);
            Assert.That(events.OfType<ConnectionSuperseded>(), Is.Empty);
            Assert.That(f.State.Players.Count, Is.EqualTo(1));
            Assert.That(a.Conn, Is.EqualTo("c2"));
            Assert.That(a.Connected, Is.True);
            Assert.That(a.Name, Is.EqualTo("Mama"));
        }

        [Test]
        public void ABadTokenIsRefused()
        {
            var f = new LobbyFixture();
            f.Seat("c1", "Mama");
            var events = f.Send(new PlayerRejoin("c2", "notatokennotatokennotato"));
            Assert.That(LobbyFixture.RefusalOf(events), Is.EqualTo(RefusalReasons.BadToken));
            Assert.That(f.Send(new PlayerRejoin("c3", null)).OfType<JoinRefused>().Single().Reason, Is.EqualTo(RefusalReasons.BadToken));
        }

        [Test]
        public void RejoinWhileTheOlderSocketIsOpenClosesTheOlderSocket()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            f.Send(new ConnectionOpened("c2"));
            var events = f.Send(new PlayerRejoin("c2", a.Token));

            var superseded = events.OfType<ConnectionSuperseded>().Single();
            Assert.That(superseded.OldConn, Is.EqualTo("c1"));
            Assert.That(superseded.NewConn, Is.EqualTo("c2"));
            Assert.That(events.OfType<PlayerRejoined>().Single().PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(a.Conn, Is.EqualTo("c2"));
        }

        [Test]
        public void TheOlderSocketClosingAfterwardsDoesNotDisconnectThePlayer()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            f.Send(new ConnectionOpened("c2"));
            f.Send(new PlayerRejoin("c2", a.Token));

            var events = f.Send(new ConnectionClosed("c1"));
            Assert.That(events, Is.Empty);
            Assert.That(a.Connected, Is.True);
            Assert.That(a.Conn, Is.EqualTo("c2"));
        }

        [Test]
        public void RejoinFromTheCurrentSocketIsARefreshWithoutAClose()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            var events = f.Send(new PlayerRejoin("c1", a.Token));

            Assert.That(events.OfType<ConnectionSuperseded>(), Is.Empty);
            var rejoined = events.OfType<PlayerRejoined>().Single();
            Assert.That(rejoined.PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(rejoined.IsRefresh, Is.True);
            Assert.That(a.Connected, Is.True);
        }

        [Test]
        public void RejoinAfterTheOlderSocketClosedSendsNoClose()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            f.Send(new ConnectionClosed("c1"));
            var events = f.Send(new PlayerRejoin("c2", a.Token));
            Assert.That(events.OfType<ConnectionSuperseded>(), Is.Empty);
            Assert.That(events.OfType<PlayerConnectionChanged>().Single().Connected, Is.True);
        }

        [Test]
        public void LosingTheRelayDisconnectsEveryoneUntilTheirSocketIsReportedAgain()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            f.Send(new RelayConnectionLost());
            Assert.That(a.Connected, Is.False);
            Assert.That(b.Connected, Is.False);

            var events = f.Send(new ConnectionOpened("c1"));
            Assert.That(events.OfType<PlayerConnectionChanged>().Single().PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(a.Connected, Is.True);
            Assert.That(b.Connected, Is.False);

            // The phone sends rejoin on tvBack over the same socket: a refresh, no close.
            var rejoin = f.Send(new PlayerRejoin("c1", a.Token));
            Assert.That(rejoin.OfType<ConnectionSuperseded>(), Is.Empty);
        }
    }
}
