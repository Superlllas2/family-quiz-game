using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>
    /// The Day 4 phone messages (PROTOCOL.md section 4): the result screen for a holder in Reveal and Scores, waiting
    /// after the pick, the effect message at the next Answer (again after a reload), the router for powerUp,
    /// effectCleared and poll, and the poll and pollDone screens.
    /// </summary>
    public sealed class PowerUpScreensTests
    {
        private LobbyFixture _f;
        private CapturingRelaySender _sender;
        private PhoneOutbox _outbox;
        private ProtocolRouter _router;

        [SetUp]
        public void SetUp()
        {
            _f = new LobbyFixture(content: TestContent.WithConfig(c => c["powerUps"]["drawChanceLeader"] = 1.0));
            _sender = new CapturingRelaySender();
            _outbox = new PhoneOutbox(_f.Game, _sender, () => "ABCD", null);
            _router = new ProtocolRouter(_sender, _f.Clock, null);
        }

        private IReadOnlyList<IGameEvent> Do(IReadOnlyList<IGameEvent> events)
        {
            _outbox.OnEvents(events);
            return events;
        }

        private IReadOnlyList<IGameEvent> Phone(string conn, string json)
        {
            var envelope = new JObject { ["relay"] = "message", ["conn"] = conn, ["msg"] = JObject.Parse(json) };
            var command = _router.ToCommand(RelayInboxItem.Message(envelope.ToString(Newtonsoft.Json.Formatting.None)));
            Assert.That(command, Is.Not.Null, json);
            return Do(_f.Game.Handle(command));
        }

        private void Step(long ms)
        {
            _f.Clock.Advance(ms);
            Do(_f.Game.Tick());
        }

        private void RunUntil(GamePhase phase)
        {
            for (int i = 0; i < 1000000 && _f.State.Phase != phase; i++) Step(10);
            Assert.That(_f.State.Phase, Is.EqualTo(phase));
        }

        private JObject Last(string conn, string type = "screen")
        {
            return _sender.MessagesTo(conn, type).LastOrDefault();
        }

        /// <summary>Mama (c1, ru) right fast, Papa (c2, en) wrong, Kid (c3, ru) right slow on question 1: Reveal is open.</summary>
        private void ToFirstReveal()
        {
            foreach (var (conn, name, lang) in new[] { ("c1", "Mama", "ru"), ("c2", "Papa", "en"), ("c3", "Kid", "ru") })
            {
                Do(_f.Game.Handle(new ConnectionOpened(conn)));
                Do(_f.Game.Handle(new PlayerJoin(conn, name, lang)));
            }

            Do(_f.ReadyAll());
            RunUntil(GamePhase.Answer);
            Do(_f.Choice("c1", _f.Right, 500));
            Do(_f.Choice("c2", _f.Wrong, 500));
            Do(_f.Choice("c3", _f.Right, 9000));
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Reveal));
        }

        [Test]
        public void AHolderGetsTheResultScreenAndTheOthersWatch()
        {
            ToFirstReveal();
            var mama = _f.State.FindByConn("c1");
            var screen = Last("c1");
            Assert.That((string)screen["id"], Is.EqualTo("result"));
            Assert.That((long)screen["phaseId"], Is.EqualTo(_f.State.PhaseId));
            Assert.That((string)screen["data"]["powerUp"], Is.EqualTo(mama.HeldPowerUp));
            Assert.That(screen["data"]["targets"].Select(t => (string)t["name"]), Is.EqualTo(new[] { "Papa", "Kid" }));
            Assert.That(screen["data"]["targets"].All(t => (bool)t["available"]), Is.True);
            Assert.That((string)Last("c2")["id"], Is.EqualTo("watch"), "no power up: watch");
            Assert.That((string)Last("c1", "player")["player"]["heldPowerUp"], Is.EqualTo(mama.HeldPowerUp));
        }

        [Test]
        public void ThePickTurnsTheScreenToWaitingAndGreysTheTargetForOthers()
        {
            ToFirstReveal();
            var mama = _f.State.FindByConn("c1");
            var powerUp = mama.HeldPowerUp;
            var events = Phone("c1", "{'v':2,'type':'input','inputId':'u1','phaseId':" + _f.State.PhaseId
                                     + ",'kind':'powerUp','powerUp':'" + powerUp + "','target':'p2'}");
            Assert.That(events.OfType<PowerUpUsed>().Count(), Is.EqualTo(1));
            Assert.That((bool)Last("c1", "ack")["ok"], Is.True);
            var waiting = Last("c1");
            Assert.That((string)waiting["id"], Is.EqualTo("waiting"));
            var names = new Dictionary<string, string> { { "freeze", "Заморозка" }, { "slime", "Слизь" }, { "fog", "Туман" } };
            Assert.That((string)waiting["data"]["locked"], Is.EqualTo(names[powerUp] + " → Papa"));
            var kid = Last("c3");
            Assert.That((string)kid["id"], Is.EqualTo("result"));
            Assert.That(kid["data"]["targets"].Single(t => (string)t["playerId"] == "p2")["available"].Value<bool>(), Is.False);

            // Scores keeps the screens and the phaseId.
            Step(_f.RevealMs);
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Scores));
            Assert.That((string)Last("c1")["id"], Is.EqualTo("waiting"));
            Assert.That((string)Last("c3")["id"], Is.EqualTo("result"));
        }

        [Test]
        public void TheEffectComesRightAfterTheAnswerScreenAndAgainAfterAReload()
        {
            ToFirstReveal();
            var powerUp = _f.State.FindByConn("c1").HeldPowerUp;
            Phone("c1", "{'v':2,'type':'input','inputId':'u1','phaseId':" + _f.State.PhaseId
                        + ",'kind':'powerUp','powerUp':'" + powerUp + "','target':'p2'}");
            RunUntil(GamePhase.Answer);
            var toPapa = _sender.MessagesTo("c2").ToList();
            var effect = toPapa.Last();
            Assert.That((string)effect["type"], Is.EqualTo("effect"));
            Assert.That((string)toPapa[toPapa.Count - 2]["type"], Is.EqualTo("screen"), "the answer screen comes first");
            Assert.That((string)effect["effect"], Is.EqualTo(powerUp));
            Assert.That((string)effect["from"], Is.EqualTo("Mama"));
            var cfg = _f.Config.PowerUps;
            switch (powerUp)
            {
                case PowerUpTypes.Freeze: Assert.That((int)effect["params"]["taps"], Is.EqualTo(cfg.Freeze.Taps)); break;
                case PowerUpTypes.Slime: Assert.That((int)effect["params"]["blobs"], Is.EqualTo(cfg.Slime.Blobs)); break;
                default: Assert.That((double)effect["params"]["seconds"], Is.EqualTo(cfg.Fog.Seconds)); break;
            }

            Assert.That(_sender.MessagesTo("c3", "effect"), Is.Empty);

            // A reload before clearing: the snapshot, then the effect again.
            var token = _f.State.FindByConn("c2").Token;
            Do(_f.Game.Handle(new ConnectionOpened("c2b")));
            Do(_f.Game.Handle(new PlayerRejoin("c2b", token)));
            Assert.That((string)_sender.MessagesTo("c2b").Last()["type"], Is.EqualTo("effect"));

            // Cleared: telemetry only, the phone still answers.
            var cleared = Phone("c2b", "{'v':2,'type':'input','inputId':'e1','phaseId':" + _f.State.PhaseId
                                       + ",'kind':'effectCleared','effect':'" + powerUp + "','ms':2650}");
            Assert.That(cleared.OfType<EffectCleared>().Single().Ms, Is.EqualTo(2650));
            Assert.That((bool)Last("c2b", "ack")["ok"], Is.True);
            Do(_f.Game.Handle(new ConnectionOpened("c2c")));
            Do(_f.Game.Handle(new PlayerRejoin("c2c", token)));
            Assert.That(_sender.MessagesTo("c2c", "effect"), Is.Empty, "no overlay after it was cleared");
        }

        [Test]
        public void ThePollScreensAndThePollMessage()
        {
            ToFirstReveal();
            RunUntil(GamePhase.Poll);
            var poll = Last("c2");
            Assert.That((string)poll["id"], Is.EqualTo("poll"));
            var questions = (JArray)poll["data"]["questions"];
            Assert.That(questions.Count, Is.EqualTo(_f.Content.Poll.Count));
            Assert.That((string)questions[0]["text"], Is.EqualTo("The most fun moment?"), "Papa plays in English");
            Assert.That(((JObject)questions[0]).ContainsKey("options"), Is.False, "text questions have no options");
            var waiting = questions.Single(q => (string)q["id"] == "waiting");
            Assert.That((string)waiting["kind"], Is.EqualTo("choice"));
            Assert.That(waiting["options"].Select(o => (string)o), Is.EqualTo(new[] { "Never", "Between questions", "During results", "In the final" }));

            var events = Phone("c2", "{'v':2,'type':'poll','inputId':'p1','answers':{'fun':'The freeze','waiting':1,'again':'x'}}");
            var submitted = events.OfType<PollSubmitted>().Single();
            Assert.That(submitted.Answers.Select(a => a.Key), Is.EqualTo(new[] { "fun", "waiting" }));
            Assert.That((bool)Last("c2", "ack")["ok"], Is.True);
            Assert.That((string)Last("c2")["id"], Is.EqualTo("pollDone"));
            Assert.That((string)Last("c1")["id"], Is.EqualTo("poll"));

            Phone("c1", "{'v':2,'type':'poll','inputId':'p2','answers':{}}");
            Phone("c3", "{'v':2,'type':'poll','inputId':'p3','answers':{'again':0}}");
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Lobby), "everyone submitted");
            Assert.That((string)Last("c2")["id"], Is.EqualTo("lobby"));
        }

        [Test]
        public void ARouterPollWithoutAnObjectIsInvalid()
        {
            ToFirstReveal();
            RunUntil(GamePhase.Poll);
            Phone("c1", "{'v':2,'type':'poll','inputId':'p1','answers':[1,2]}");
            Assert.That((string)Last("c1", "ack")["reason"], Is.EqualTo("invalid"));
        }
    }
}
