using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>PROTOCOL.md section 3 (inputId, duplicate), section 4 (ack).</summary>
    public sealed class InputTests
    {
        private static SubmitAnswer Choice(string conn, string inputId, int choice, long ms, long phaseId = 1)
        {
            return new SubmitAnswer(conn, inputId, phaseId, InputKinds.Choice, choice, ms);
        }

        [Test]
        public void AChoiceIsRecordedAndAcked()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var events = f.Send(Choice("c1", "a1", 2, 3410, f.State.PhaseId));

            var ack = events.OfType<InputAcked>().Single();
            Assert.That(ack.Conn, Is.EqualTo("c1"));
            Assert.That(ack.InputId, Is.EqualTo("a1"));
            Assert.That(ack.Ok, Is.True);
            Assert.That(ack.Reason, Is.Null);
            Assert.That(events.OfType<AnswerReceived>().Single().PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(a.LastAnswer.Choice, Is.EqualTo(2));
            Assert.That(a.LastAnswer.Ms, Is.EqualTo(3410));
            Assert.That(a.LastAnswer.InputId, Is.EqualTo("a1"));
        }

        [Test]
        public void TheLastInputPerPlayerIsKept()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            f.Send(Choice("c1", "a1", 0, 1000));
            f.Send(Choice("c2", "b1", 3, 2000));
            f.Send(Choice("c1", "a2", 1, 1500));

            Assert.That(a.LastAnswer.Choice, Is.EqualTo(1));
            Assert.That(a.LastAnswer.Ms, Is.EqualTo(1500));
            Assert.That(b.LastAnswer.Choice, Is.EqualTo(3));
        }

        [Test]
        public void ADuplicateInputIdIsAckedAgainButNotAppliedTwice()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            f.Send(Choice("c1", "a1", 2, 3410));
            var first = a.LastAnswer;

            var events = f.Send(Choice("c1", "a1", 0, 9999));
            var ack = events.OfType<InputAcked>().Single();
            Assert.That(ack.Ok, Is.True);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Duplicate));
            Assert.That(events.OfType<AnswerReceived>(), Is.Empty);
            Assert.That(a.LastAnswer, Is.SameAs(first));
        }

        [Test]
        public void ADuplicateKeepsWorkingAfterARejoinOnANewSocket()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            f.Send(Choice("c1", "a1", 2, 3410));
            f.Send(new ConnectionClosed("c1"));
            f.Send(new PlayerRejoin("c2", a.Token));

            var ack = f.Send(Choice("c2", "a1", 2, 3410)).OfType<InputAcked>().Single();
            Assert.That(ack.Conn, Is.EqualTo("c2"));
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Duplicate));
        }

        [Test]
        public void TheSameInputIdFromTwoPlayersIsAppliedForBoth()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            f.Send(Choice("c1", "x", 1, 100));
            var ack = f.Send(Choice("c2", "x", 2, 200)).OfType<InputAcked>().Single();
            Assert.That(ack.Reason, Is.Null);
            Assert.That(b.LastAnswer.Choice, Is.EqualTo(2));
            Assert.That(a.LastAnswer.Choice, Is.EqualTo(1));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void AChoiceOutsideZeroToThreeIsInvalid(int choice)
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var ack = f.Send(Choice("c1", "a1", choice, 100)).OfType<InputAcked>().Single();
            Assert.That(ack.Ok, Is.False);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(a.LastAnswer, Is.Null);
        }

        [Test]
        public void AnInputWithoutASeatIsInvalid()
        {
            var f = new LobbyFixture();
            var ack = f.Send(Choice("nobody", "a1", 1, 100)).OfType<InputAcked>().Single();
            Assert.That(ack.Ok, Is.False);
            Assert.That(ack.Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void LobbyAndCharacterAreInvalidForNow()
        {
            var f = new LobbyFixture();
            f.Seat("c1", "A");
            var lobby = f.Send(new LobbyAction("c1", "l1", "start")).OfType<InputAcked>().Single();
            var character = f.Send(new PickCharacter("c1", "k1", 3)).OfType<InputAcked>().Single();
            Assert.That(lobby.Ok, Is.False);
            Assert.That(lobby.Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(character.Ok, Is.False);
            Assert.That(character.Reason, Is.EqualTo(AckReasons.Invalid));

            var again = f.Send(new LobbyAction("c1", "l1", "start")).OfType<InputAcked>().Single();
            Assert.That(again.Reason, Is.EqualTo(AckReasons.Invalid));
        }
    }
}
