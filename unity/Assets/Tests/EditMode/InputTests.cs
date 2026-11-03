using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>PROTOCOL.md section 3 (inputId, duplicate), section 4 (ack), the Day 2 refusal rules.</summary>
    public sealed class InputTests
    {
        [Test]
        public void AChoiceIsLockedAndAcked()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            var events = f.Choice("c1", 2, 3410, "a1");

            var ack = LobbyFixture.AckOf(events);
            Assert.That(ack.Conn, Is.EqualTo("c1"));
            Assert.That(ack.InputId, Is.EqualTo("a1"));
            Assert.That(ack.Ok, Is.True);
            Assert.That(ack.Reason, Is.Null);
            var locked = events.OfType<AnswerLocked>().Single();
            Assert.That(locked.PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(locked.PhaseId, Is.EqualTo(f.State.PhaseId));
            Assert.That(locked.Choice, Is.EqualTo(2));
            Assert.That(locked.Ms, Is.EqualTo(3410));
            Assert.That(seats[0].Turn.Choice, Is.EqualTo(2));
            Assert.That(seats[0].Turn.Ms, Is.EqualTo(3410));
            Assert.That(seats[0].Turn.InputId, Is.EqualTo("a1"));
            Assert.That(seats[0].Turn.LockedAtMs, Is.EqualTo(f.Clock.NowMs));
        }

        [Test]
        public void ADuplicateInputIdIsAckedAgainButNotAppliedTwice()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", 2, 3410, "a1");

            var events = f.Choice("c1", 0, 9999, "a1");
            var ack = LobbyFixture.AckOf(events);
            Assert.That(ack.Ok, Is.True);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Duplicate));
            Assert.That(events.OfType<AnswerLocked>(), Is.Empty);
            Assert.That(seats[0].Turn.Choice, Is.EqualTo(2));
            Assert.That(seats[0].Turn.Ms, Is.EqualTo(3410));
        }

        [Test]
        public void ADuplicateKeepsWorkingAfterARejoinOnANewSocket()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", 2, 3410, "a1");
            f.Send(new ConnectionClosed("c1"));
            f.Send(new PlayerRejoin("c9", seats[0].Token));

            var ack = LobbyFixture.AckOf(f.Choice("c9", 2, 3410, "a1"));
            Assert.That(ack.Conn, Is.EqualTo("c9"));
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Duplicate));
        }

        [Test]
        public void TheSameInputIdFromTwoPlayersIsAppliedForBoth()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", 1, 100, "x");
            var ack = LobbyFixture.AckOf(f.Choice("c2", 2, 200, "x"));
            Assert.That(ack.Reason, Is.Null);
            Assert.That(seats[1].Turn.Choice, Is.EqualTo(2));
            Assert.That(seats[0].Turn.Choice, Is.EqualTo(1));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void AChoiceOutsideZeroToThreeIsInvalid(int choice)
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var events = f.Choice("c1", choice, 100);
            var ack = LobbyFixture.AckOf(events);
            Assert.That(ack.Ok, Is.False);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
            var refused = events.OfType<InputRefused>().Single();
            Assert.That(refused.PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(refused.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(refused.Kind, Is.EqualTo(InputKinds.Choice));
            Assert.That(seats[0].Turn.Locked, Is.False);
        }

        [Test]
        public void AChoiceWithoutMsIsInvalid()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            var ack = LobbyFixture.AckOf(f.Choice("c1", 1, null));
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(seats[0].Turn.Locked, Is.False);
        }

        [Test]
        public void ASecondInputAfterLockingInIsInvalid()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", 1, 100);
            var ack = LobbyFixture.AckOf(f.Choice("c1", 2, 200));
            Assert.That(ack.Ok, Is.False);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(seats[0].Turn.Choice, Is.EqualTo(1));
        }

        [Test]
        public void AnotherInputKindInAnswerIsInvalid()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            var events = f.Send(new SubmitAnswer("c1", "v1", f.State.PhaseId, InputKinds.Vote, 1, 100));
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void AChoiceNamingAPhaseThatTakesNoChoiceIsInvalid()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "Lobby");
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Intro));
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "Intro");
            f.Advance(f.IntroMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "TopicVote");
            f.Advance(f.TopicVoteMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            Assert.That(LobbyFixture.AckOf(f.Choice("c1", 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "QuestionShow");
        }

        [Test]
        public void AnInputWithoutASeatIsInvalidAndNotLoggedAsRefused()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            var events = f.Send(new SubmitAnswer("nobody", "a1", f.State.PhaseId, InputKinds.Choice, 1, 100));
            var ack = LobbyFixture.AckOf(events);
            Assert.That(ack.Ok, Is.False);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(events.OfType<InputRefused>(), Is.Empty);
        }

        [Test]
        public void ARefusedInputResentGetsTheSameAckWithoutASecondRefusedEvent()
        {
            var f = new LobbyFixture();
            f.ToFirstAnswer(2);
            f.Choice("c1", 7, 100, "bad");
            var events = f.Choice("c1", 1, 100, "bad");
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(events.OfType<InputRefused>(), Is.Empty);
            Assert.That(events.OfType<AnswerLocked>(), Is.Empty);
        }
    }
}
