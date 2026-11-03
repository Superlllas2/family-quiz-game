using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
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

            PlayUntil(GamePhase.Results);
            int statements = _f.State.Race.StatementNumber;
            var results = _sender.MessagesTo("c1", "screen").Last(m => (string)m["id"] == "results");
            PlayUntil(GamePhase.Lobby);

            var rounds = _f.Config.Rounds;
            var closest = rounds.FormatOrder.Select(x => x == QuestionDef.ClosestValue).ToArray();
            var head = new List<string> { "pickCharacter", "lobby", "countdown" };
            var expected = head.Concat(Expected(rounds.Count, statements, true, true, closest)).ToList();
            Assert.That(ScreenIds("c1"), Is.EqualTo(expected), "c1");
            Assert.That(ScreenIds("c2"), Is.EqualTo(expected), "c2");
            // c3 never sends anything: no waiting screens (last call resends the answer screen with the same id).
            Assert.That(ScreenIds("c3"), Is.EqualTo(head.Concat(Expected(rounds.Count, statements, false, true, closest)).ToList()), "c3");

            // The Intro lists the players to predict; the vote shows four doors, the mixed one last.
            var intro = _sender.MessagesTo("c2", "screen").First(m => (string)m["id"] == "getReady");
            Assert.That(((JArray)intro["data"]["prediction"]).Count, Is.EqualTo(3));
            var vote = _sender.MessagesTo("c2", "screen").First(m => (string)m["id"] == "topicVote");
            Assert.That(((JArray)vote["data"]["doors"]).Count, Is.EqualTo(_f.Config.Topics.Doors));
            Assert.That((string)vote["data"]["doors"][3]["title"], Is.EqualTo("A bit of everything"), "the mixed door in English");
            var slider = _sender.MessagesTo("c2", "screen").First(m => (string)m["id"] == "answerSlider");
            Assert.That((double)slider["data"]["max"], Is.GreaterThan((double)slider["data"]["min"]));
            Assert.That((long)slider["data"]["windowMs"], Is.EqualTo(_f.ClosestWindowMs));
            Assert.That(slider["data"]["unit"], Is.Not.Null);
            var tf = _sender.MessagesTo("c2", "screen").First(m => (string)m["id"] == "answerTrueFalse");
            Assert.That((long)tf["data"]["windowMs"], Is.EqualTo(_f.RaceWindowMs));
            var roundEnds = _sender.MessagesTo("c1", "screen").Where(m => (string)m["id"] == "roundEnd").ToList();
            Assert.That(roundEnds.Select(m => m["data"]["nextRound"].Type == JTokenType.Null ? 0 : (int)m["data"]["nextRound"]),
                Is.EqualTo(new[] { 2, 3, 4, 0 }), "after round 4 the final race comes next (null)");
            var race = _sender.MessagesTo("c1", "screen").Last(m => (string)m["id"] == "raceResult");
            Assert.That((int)race["data"]["position"], Is.EqualTo(_f.Config.Race.FinishSteps));
            Assert.That((bool)race["data"]["correct"], Is.True);
            Assert.That((int)race["data"]["finish"], Is.EqualTo(_f.Config.Race.FinishSteps));
            var waitingTf = _sender.MessagesTo("c2", "screen").Last(m => (string)m["id"] == "waiting");
            Assert.That((string)waitingTf["data"]["locked"], Is.AnyOf("True", "False"));

            // Texts in each phone's language; c1 always right, so it wins the race and has the most correct answers.
            var lastResult = _sender.MessagesTo("c2", "screen").Last(m => (string)m["id"] == "result");
            StringAssert.StartsWith("Right answer:", (string)lastResult["data"]["correctText"]);
            Assert.That((int)results["data"]["place"], Is.EqualTo(1));
            Assert.That((int)results["data"]["correctCount"], Is.EqualTo(rounds.Count * rounds.QuestionsPerRound));
            Assert.That(((JArray)results["data"]["awards"]).Select(a => (string)a), Is.EqualTo(new[] { "Больше всех верных" }));
            // Back in the Lobby: nobody ready, scores at 0, characters kept.
            var lobby = LastScreen("c3");
            Assert.That((bool)lobby["data"]["ready"], Is.False);
            Assert.That(((JArray)lobby["data"]["players"]).All(p => !(bool)p["ready"]), Is.True);
            Assert.That((int)_sender.MessagesTo("c1", "player").Last()["player"]["score"], Is.EqualTo(0), "scores reset");

            // Everyone taps Ready again: the countdown, then straight to the topic vote (no Intro).
            _sender.Clear();
            Ready("c3");
            Ready("c1");
            Ready("c2");
            RunUntil(GamePhase.TopicVote);
            Assert.That(ScreenIds("c1"), Is.EqualTo(new[] { "lobby", "countdown", "topicVote" }));
        }

        /// <summary>c1 always answers right at once, c2 always wrong at once, c3 never sends anything.</summary>
        private void PlayInputs()
        {
            var state = _f.State;
            switch (state.Phase)
            {
                case GamePhase.TopicVote:
                    Do(_f.Vote("c1", 0));
                    Do(_f.Vote("c2", 1));
                    break;
                case GamePhase.Answer when state.Question.IsClosestValue:
                    Do(_f.Value("c1", state.Question.Def.Answer.Value, 900));
                    Do(_f.Value("c2", state.Question.Def.Range[0], 1200));
                    break;
                case GamePhase.Answer:
                    Do(_f.Choice("c1", _f.Right, 900));
                    Do(_f.Choice("c2", _f.Wrong, 1200));
                    break;
                case GamePhase.RaceQuestion:
                    Do(_f.TrueFalse("c1", state.Race.Statement.Answer, 900));
                    Do(_f.TrueFalse("c2", !state.Race.Statement.Answer, 1200));
                    break;
            }
        }

        /// <summary>Runs the game to the given phase; the phones answer at the start of every input phase.</summary>
        private void PlayUntil(GamePhase phase)
        {
            long lastPhaseId = -1;
            for (int i = 0; i < 1000000 && _f.State.Phase != phase; i++)
            {
                if (_f.State.PhaseId != lastPhaseId)
                {
                    lastPhaseId = _f.State.PhaseId;
                    PlayInputs();
                    if (_f.State.Phase == phase) break;
                }

                Step(10);
            }

            Assert.That(_f.State.Phase, Is.EqualTo(phase));
        }

        private static List<string> Expected(int rounds, int statements, bool answers, bool intro, bool[] closest)
        {
            var list = new List<string>();
            if (intro) list.Add("getReady");
            for (int r = 0; r < rounds; r++)
            {
                list.Add("topicVote");
                if (answers) list.Add("waiting");
                foreach (var cv in closest)
                {
                    list.Add("getReady");
                    list.Add(cv ? "answerSlider" : "answerChoice");
                    if (answers) list.Add("waiting");
                    list.Add("result");
                }

                list.Add("roundEnd");
            }

            list.Add("getReady");
            for (int s = 0; s < statements; s++)
            {
                list.Add("answerTrueFalse");
                if (answers) list.Add("waiting");
                list.Add("raceResult");
            }

            list.Add("results");
            list.Add("lobby");
            return list;
        }

        [Test]
        public void TheWaitingScreensShowWhatWasSent()
        {
            JoinAndPick("c1", "Mama", "ru");
            JoinAndPick("c2", "Papa", "en");
            Ready("c1");
            Ready("c2");
            RunUntil(GamePhase.TopicVote);
            Do(_f.Vote("c2", 3));
            var waiting = LastScreen("c2");
            Assert.That((string)waiting["id"], Is.EqualTo("waiting"));
            Assert.That((string)waiting["data"]["locked"], Is.EqualTo("A bit of everything"));
            Assert.That(((JArray)waiting["data"]["pending"]).Select(p => (string)p["name"]), Is.EqualTo(new[] { "Mama" }));
            Do(_f.Vote("c1", 3));

            while (!(_f.State.Phase == GamePhase.Answer && _f.State.Question.IsClosestValue)) Step(50);
            var answer = _f.State.Question.Def.Answer.Value;
            var unit = _f.State.Question.Def.Unit.En;
            Do(_f.Value("c2", answer, 2000));
            Assert.That((string)LastScreen("c2")["data"]["locked"], Is.EqualTo((NumberText.Format(answer) + " " + unit).Trim()));
            RunUntil(GamePhase.Reveal);
            var result = LastScreen("c2");
            Assert.That((string)result["data"]["correctText"], Is.EqualTo(("Right answer: " + NumberText.Format(answer) + " " + unit).Trim()));
            Assert.That((bool)result["data"]["correct"], Is.True);
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
