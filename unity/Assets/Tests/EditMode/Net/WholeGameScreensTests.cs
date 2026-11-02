using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using FamilyQuiz.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>
    /// The real Core and the real PhoneOutbox together: three phones tap Ready, play a whole game, land back in the
    /// Lobby and start a second one (D76), and each phone receives the screens of docs/06_phone_screens.md in order.
    /// </summary>
    public sealed class WholeGameScreensTests
    {
        private LobbyFixture _f;
        private CapturingRelaySender _sender;
        private PhoneOutbox _outbox;

        [SetUp]
        public void SetUp()
        {
            _f = new LobbyFixture();
            _sender = new CapturingRelaySender();
            _outbox = new PhoneOutbox(_f.Game, _sender, () => "ABCD", null);
        }

        private IReadOnlyList<IGameEvent> Do(IReadOnlyList<IGameEvent> events)
        {
            _outbox.OnEvents(events);
            return events;
        }

        private void Step(long ms)
        {
            _f.Clock.Advance(ms);
            Do(_f.Game.Tick());
        }

        private void RunUntil(GamePhase phase)
        {
            for (int i = 0; i < 100000 && _f.State.Phase != phase; i++) Step(10);
            Assert.That(_f.State.Phase, Is.EqualTo(phase));
        }

        private List<string> ScreenIds(string conn)
        {
            var ids = new List<string>();
            foreach (var m in _sender.MessagesTo(conn))
            {
                var type = (string)m["type"];
                var id = type == "screen" ? (string)m["id"] : type == "snapshot" ? (string)m["screen"]?["id"] : null;
                if (id != null && (ids.Count == 0 || ids[ids.Count - 1] != id)) ids.Add(id);
            }

            return ids;
        }

        private void JoinAndPick(string conn, string name, string lang)
        {
            Do(_f.Game.Handle(new ConnectionOpened(conn)));
            Do(_f.Game.Handle(new PlayerJoin(conn, name, lang)));
            var player = _f.State.FindByConn(conn);
            Do(_f.Game.Handle(new PickCharacter(conn, _f.NextInputId(), player.CharacterId)));
        }

        private void Ready(string conn)
        {
            Do(_f.Game.Handle(new LobbyAction(conn, _f.NextInputId(), LobbyActions.Ready)));
        }

        private JObject LastScreen(string conn)
        {
            return _sender.MessagesTo(conn, "screen").Last();
        }

        [Test]
        public void ThreePhonesSeeEveryScreenInOrderBackToTheLobby()
        {
            JoinAndPick("c1", "Mama", "ru");
            JoinAndPick("c2", "Papa", "en");
            JoinAndPick("c3", "Kid", "ru");

            // Each Ready refreshes every lobby screen with the marks; the last one starts the countdown.
            Ready("c2");
            var mamaSees = LastScreen("c1");
            Assert.That((string)mamaSees["id"], Is.EqualTo("lobby"));
            Assert.That((bool)mamaSees["data"]["ready"], Is.False);
            Assert.That(((JArray)mamaSees["data"]["players"]).Select(p => (bool)p["ready"]), Is.EqualTo(new[] { false, true, false }));
            Assert.That((bool)LastScreen("c2")["data"]["ready"], Is.True);
            Ready("c1");
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Ready("c3");
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));

            for (int q = 0; q < _f.State.QuestionCount || _f.State.Phase == GamePhase.StartCountdown; q++)
            {
                RunUntil(GamePhase.Answer);
                // c1 and c2 answer at once, c3 never answers.
                Do(_f.Game.Handle(new SubmitAnswer("c1", _f.NextInputId(), _f.State.PhaseId, InputKinds.Choice, _f.Right, 900)));
                Do(_f.Game.Handle(new SubmitAnswer("c2", _f.NextInputId(), _f.State.PhaseId, InputKinds.Choice, _f.Wrong, 1200)));
                RunUntil(GamePhase.Reveal);
            }

            RunUntil(GamePhase.Results);
            var results = _sender.MessagesTo("c1", "screen").Last(m => (string)m["id"] == "results");
            RunUntil(GamePhase.Lobby);

            var expected = new List<string> { "pickCharacter", "lobby", "countdown" };
            for (int q = 0; q < _f.State.QuestionCount; q++) expected.AddRange(new[] { "getReady", "answerChoice", "waiting", "result" });
            expected.AddRange(new[] { "results", "lobby" });
            Assert.That(ScreenIds("c1"), Is.EqualTo(expected), "c1");
            Assert.That(ScreenIds("c2"), Is.EqualTo(expected), "c2");

            // c3 never answers: last call after the others locked in, then the result. No waiting screen.
            var c3 = new List<string> { "pickCharacter", "lobby", "countdown" };
            for (int q = 0; q < _f.State.QuestionCount; q++) c3.AddRange(new[] { "getReady", "answerChoice", "result" });
            c3.AddRange(new[] { "results", "lobby" });
            Assert.That(ScreenIds("c3"), Is.EqualTo(c3), "c3");

            // Texts in each phone's language; c1 always right, so it has the most points.
            var lastResult = _sender.MessagesTo("c2", "screen").Last(m => (string)m["id"] == "result");
            StringAssert.StartsWith("Right answer:", (string)lastResult["data"]["correctText"]);
            Assert.That((int)results["data"]["place"], Is.EqualTo(1));
            Assert.That((int)results["data"]["correctCount"], Is.EqualTo(_f.State.QuestionCount));
            Assert.That(((JArray)results["data"]["awards"]).Select(a => (string)a), Is.EqualTo(new[] { "Больше всех верных" }));
            // Back in the Lobby: nobody ready, scores at 0, characters kept.
            var lobby = LastScreen("c3");
            Assert.That((bool)lobby["data"]["ready"], Is.False);
            Assert.That(((JArray)lobby["data"]["players"]).All(p => !(bool)p["ready"]), Is.True);
            Assert.That((int)_sender.MessagesTo("c1", "player").Last()["player"]["score"], Is.EqualTo(0), "scores reset");

            // Everyone taps Ready again: the countdown, then the first question of the second game.
            _sender.Clear();
            Ready("c3");
            Ready("c1");
            Ready("c2");
            RunUntil(GamePhase.QuestionShow);
            Assert.That(ScreenIds("c1"), Is.EqualTo(new[] { "lobby", "countdown", "getReady" }));
        }

        [Test]
        public void LastCallResendsTheAnswerScreenWithTheSamePhaseId()
        {
            JoinAndPick("c1", "Mama", "ru");
            JoinAndPick("c2", "Papa", "ru");
            Ready("c1");
            Ready("c2");
            RunUntil(GamePhase.Answer);
            long phaseId = _f.State.PhaseId;
            _sender.Clear();
            Do(_f.Game.Handle(new SubmitAnswer("c1", _f.NextInputId(), phaseId, InputKinds.Choice, _f.Right, 900)));
            var screens = _sender.MessagesTo("c2", "screen");
            Assert.That(screens.Count, Is.EqualTo(1));
            Assert.That((string)screens[0]["id"], Is.EqualTo("answerChoice"));
            Assert.That((long)screens[0]["phaseId"], Is.EqualTo(phaseId));
            Assert.That((long)screens[0]["endsInMs"], Is.LessThanOrEqualTo(_f.LastCallMs));
        }
    }
}
