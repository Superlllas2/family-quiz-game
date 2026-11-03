using FamilyQuiz.Core.Commands;
using FamilyQuiz.Net;
using NUnit.Framework;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>Relay envelopes to Core commands (PROTOCOL.md sections 2 and 3).</summary>
    public sealed class ProtocolRouterTests
    {
        private CapturingRelaySender _sender;
        private ProtocolRouter _router;

        [SetUp]
        public void SetUp()
        {
            _sender = new CapturingRelaySender();
            _router = new ProtocolRouter(_sender, new ManualClock(), null);
        }

        private IGameCommand Route(string msgJson)
        {
            return _router.ToCommand(RelayInboxItem.Message("{\"relay\":\"message\",\"conn\":\"c1\",\"msg\":" + msgJson + "}"));
        }

        [Test]
        public void SettingsKeepsAFractionalTimerMultiplier()
        {
            var cmd = Route("{\"v\":2,\"type\":\"settings\",\"lang\":\"en\",\"timerMultiplier\":1.5}") as ChangeSettings;
            Assert.That(cmd, Is.Not.Null);
            Assert.That(cmd.Conn, Is.EqualTo("c1"));
            Assert.That(cmd.Lang, Is.EqualTo("en"));
            Assert.That(cmd.TimerMultiplier, Is.EqualTo(1.5));
            Assert.That(_sender.Sent, Is.Empty, "settings has no ack");
        }

        [Test]
        public void SettingsReadsAWholeTimerMultiplier()
        {
            var cmd = (ChangeSettings)Route("{\"v\":2,\"type\":\"settings\",\"lang\":\"ru\",\"timerMultiplier\":2}");
            Assert.That(cmd.Lang, Is.EqualTo("ru"));
            Assert.That(cmd.TimerMultiplier, Is.EqualTo(2.0));
        }

        [Test]
        public void SettingsWithMissingOrBrokenFieldsPassesNulls()
        {
            var cmd = (ChangeSettings)Route("{\"v\":2,\"type\":\"settings\",\"lang\":5,\"timerMultiplier\":\"fast\"}");
            Assert.That(cmd.Lang, Is.Null);
            Assert.That(cmd.TimerMultiplier, Is.Null);

            cmd = (ChangeSettings)Route("{\"v\":2,\"type\":\"settings\"}");
            Assert.That(cmd.Lang, Is.Null);
            Assert.That(cmd.TimerMultiplier, Is.Null);
        }

        [Test]
        public void ChoiceInputBecomesSubmitAnswer()
        {
            var cmd = (SubmitAnswer)Route("{\"v\":2,\"type\":\"input\",\"inputId\":\"a2\",\"phaseId\":16,\"kind\":\"choice\",\"choice\":1,\"ms\":3410}");
            Assert.That(cmd.InputId, Is.EqualTo("a2"));
            Assert.That(cmd.PhaseId, Is.EqualTo(16));
            Assert.That(cmd.Kind, Is.EqualTo("choice"));
            Assert.That(cmd.Choice, Is.EqualTo(1));
            Assert.That(cmd.Ms, Is.EqualTo(3410));
        }

        [Test]
        public void AnUnknownInputKindIsPassedOnForCoreToRefuse()
        {
            var cmd = (SubmitAnswer)Route("{\"v\":2,\"type\":\"input\",\"inputId\":\"a1\",\"phaseId\":14,\"kind\":\"vote\",\"door\":2}");
            Assert.That(cmd.Kind, Is.EqualTo("vote"));
            Assert.That(cmd.Choice, Is.Null);
            Assert.That(cmd.Ms, Is.Null);
        }

        [Test]
        public void AChoiceOutsideTheIntRangeIsNull()
        {
            var cmd = (SubmitAnswer)Route("{\"v\":2,\"type\":\"input\",\"inputId\":\"a3\",\"phaseId\":16,\"kind\":\"choice\",\"choice\":4294967297,\"ms\":10}");
            Assert.That(cmd.Choice, Is.Null);
        }

        [Test]
        public void CharacterBecomesPickCharacter()
        {
            var cmd = (PickCharacter)Route("{\"v\":2,\"type\":\"character\",\"inputId\":\"k1\",\"characterId\":4}");
            Assert.That(cmd.CharacterId, Is.EqualTo(4));
            Assert.That(cmd.InputId, Is.EqualTo("k1"));
        }

        [Test]
        public void AWrongVersionIsRefusedAndNotPassedOn()
        {
            Assert.That(Route("{\"v\":1,\"type\":\"settings\",\"lang\":\"en\",\"timerMultiplier\":1}"), Is.Null);
            Assert.That(_sender.Sent, Is.EqualTo(new[] { "{\"to\":\"c1\",\"msg\":{\"v\":2,\"type\":\"refused\",\"reason\":\"badVersion\"}}" }));
        }
    }
}
