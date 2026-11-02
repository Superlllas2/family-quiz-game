using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>PROTOCOL.md section 5 (leave, seat hold), docs/18_edge_cases.md (player taps Leave, seat hold runs out).</summary>
    public sealed class LeaveAndSeatHoldTests
    {
        [Test]
        public void LeaveIsAckedAndFreesTheSeatAndTheName()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "Mama");
            var events = f.Send(new PlayerLeave("c1", "bye1"));

            var ack = events.OfType<InputAcked>().Single();
            Assert.That(ack.InputId, Is.EqualTo("bye1"));
            Assert.That(ack.Ok, Is.True);
            var left = events.OfType<PlayerLeft>().Single();
            Assert.That(left.Reason, Is.EqualTo(PlayerLeft.ReasonLeft));
            Assert.That(events.OfType<RosterChanged>().Count(), Is.EqualTo(1));
            Assert.That(f.State.Players, Is.Empty);

            Assert.That(f.Join("c2", "mama").OfType<PlayerJoined>().Count(), Is.EqualTo(1));
            Assert.That(LobbyFixture.RefusalOf(f.Send(new PlayerRejoin("c3", a.Token))), Is.EqualTo(RefusalReasons.BadToken));
        }

        [Test]
        public void LeaveResentAfterTheSeatIsFreedIsStillAckedOk()
        {
            var f = new LobbyFixture();
            f.Seat("c1", "Mama");
            f.Send(new PlayerLeave("c1", "bye1"));
            var events = f.Send(new PlayerLeave("c1", "bye1"));
            Assert.That(events.OfType<InputAcked>().Single().Ok, Is.True);
            Assert.That(events.OfType<PlayerLeft>(), Is.Empty);
        }

        [Test]
        public void TheSeatIsHeldForSeatHoldSecondsThenFreed()
        {
            var f = new LobbyFixture();
            var holdMs = (long)(f.Config.Players.SeatHoldSeconds * 1000);
            var a = f.Seat("c1", "Mama");
            f.Seat("c2", "Papa");
            f.Send(new ConnectionClosed("c1"));

            f.Clock.Advance(holdMs - 1);
            Assert.That(f.Game.Tick(), Is.Empty);
            Assert.That(f.State.FindById(a.PlayerId), Is.Not.Null);

            f.Clock.Advance(1);
            var events = f.Game.Tick();
            var left = events.OfType<PlayerLeft>().Single();
            Assert.That(left.PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(left.Reason, Is.EqualTo(PlayerLeft.ReasonSeatExpired));
            Assert.That(events.OfType<RosterChanged>().Count(), Is.EqualTo(1));
            Assert.That(f.State.Players.Count, Is.EqualTo(1));
            Assert.That(f.Join("c3", "MAMA").OfType<PlayerJoined>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ARejoinInsideTheHoldKeepsTheSeat()
        {
            var f = new LobbyFixture();
            var holdMs = (long)(f.Config.Players.SeatHoldSeconds * 1000);
            var a = f.Seat("c1", "Mama");
            f.Send(new ConnectionClosed("c1"));
            f.Clock.Advance(holdMs - 1000);
            f.Send(new PlayerRejoin("c2", a.Token));
            f.Clock.Advance(holdMs);
            Assert.That(f.Game.Tick(), Is.Empty);
            Assert.That(f.State.FindById(a.PlayerId), Is.Not.Null);
        }

        [Test]
        public void AConnectedPlayerIsNeverExpired()
        {
            var f = new LobbyFixture();
            f.Seat("c1", "Mama");
            f.Clock.Advance((long)(f.Config.Players.SeatHoldSeconds * 1000) * 10);
            Assert.That(f.Game.Tick(), Is.Empty);
        }
    }
}
