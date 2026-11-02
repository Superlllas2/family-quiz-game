using System.Linq;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;
using static FamilyQuiz.Tests.NetScreens.NetJson;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>Which phone gets which message for each Core event.</summary>
    public sealed class PhoneOutboxTests
    {
        private NetTestGame _t;

        [SetUp]
        public void SetUp()
        {
            _t = new NetTestGame();
            _t.AddFamily();
        }

        private PlayerState Mama => _t.State.FindById("p1");
        private PlayerState Papa => _t.State.FindById("p2");
        private PlayerState Kid => _t.State.FindById("p3");

        [Test]
        public void PhaseStartedSendsEveryReachablePlayerTheirScreen()
        {
            _t.AddPlayer("p4", "Away", "ru", 4, "c4", connected: false);
            _t.SetPhase(GamePhase.StartCountdown, 2, 3000);
            _t.Outbox.OnEvents(new IGameEvent[] { new PhaseEnded(GamePhase.Lobby, 1, PhaseEndReasons.AllDone, 5000), new PhaseStarted(GamePhase.StartCountdown, 2, 3000) });

            AssertJson("{'v':1,'type':'screen','phaseId':2,'id':'countdown','endsInMs':3000,'data':{'text':'Игра начинается'}}",
                _t.Sender.MessagesTo("c1").Single());
            AssertJson("{'v':1,'type':'screen','phaseId':2,'id':'countdown','endsInMs':3000,'data':{'text':'Starting'}}",
                _t.Sender.MessagesTo("c2").Single());
            Assert.That(_t.Sender.MessagesTo("c3").Count, Is.EqualTo(1));
            Assert.That(_t.Sender.MessagesTo("c4"), Is.Empty, "a disconnected player gets nothing");
        }

        [Test]
        public void AnswerPhaseStartedSendsAnswerChoice()
        {
            _t.EnterAnswer(4);
            _t.Outbox.OnEvent(new PhaseStarted(GamePhase.Answer, 4, _t.BaseWindowMs));
            foreach (var conn in new[] { "c1", "c2", "c3" })
            {
                var screen = _t.Sender.MessagesTo(conn, "screen").Single();
                Assert.That((string)screen["id"], Is.EqualTo("answerChoice"));
                Assert.That((long)screen["phaseId"], Is.EqualTo(4));
            }
        }

        [Test]
        public void AnswerLockedSendsWaitingToTheLockerAndRefreshesOtherLockedPlayers()
        {
            _t.EnterAnswer(4);
            _t.Lock(Mama, 0, 1000);
            _t.Lock(Papa, 2, 2000);
            _t.Outbox.OnEvents(new IGameEvent[]
            {
                new InputAcked("c2", "in_p2", true, null),
                new AnswerLocked("p2", 4, InputKinds.Choice, 2, 2000, false, 0)
            });

            var papa = _t.Sender.MessagesTo("c2");
            Assert.That(papa.Select(m => (string)m["type"]), Is.EqualTo(new[] { "ack", "screen" }), "ack first, then the screen");
            AssertJson("{'v':1,'type':'screen','phaseId':4,'id':'waiting','endsInMs':15000,'data':{'locked':'C · Dnieper','pending':[{'name':'Kid','characterId':2}]}}",
                papa[1]);
            AssertJson("{'v':1,'type':'screen','phaseId':4,'id':'waiting','endsInMs':15000,'data':{'locked':'A · Рейн','pending':[{'name':'Kid','characterId':2}]}}",
                _t.Sender.MessagesTo("c1").Single());
            Assert.That(_t.Sender.MessagesTo("c3"), Is.Empty, "a player still answering keeps their buttons");
        }

        [Test]
        public void LastCallResendsAnswerChoiceWithTheSamePhaseIdAndTheNewTimeLeft()
        {
            _t.EnterAnswer(4);
            _t.Clock.Advance(2000);
            _t.Lock(Mama, 0, 2000);
            _t.Lock(Papa, 1, 2000);
            var cut = _t.Clock.NowMs + (long)(_t.Config.Timers.LastCall * 1000);
            Kid.Turn.DeadlineMs = cut;
            Kid.Turn.LastCall = true;
            _t.Outbox.OnEvents(new IGameEvent[]
            {
                new AnswerLocked("p2", 4, InputKinds.Choice, 1, 2000, true, 930),
                new LastCallStarted("p3", 4, cut)
            });

            AssertJson(@"{'v':1,'type':'screen','phaseId':4,'id':'answerChoice','endsInMs':5000,'data':{
                'question':'Какая река самая длинная в Европе?','options':['Рейн','Волга','Днепр','Дунай'],'windowMs':15000}}",
                _t.Sender.MessagesTo("c3").Single());
        }

        [Test]
        public void ConnectionChangeInAnswerRefreshesOnlyTheWaitingLists()
        {
            _t.EnterAnswer(4);
            _t.Lock(Mama, 0, 1000);
            Kid.Connected = false;
            _t.Outbox.OnEvent(new PlayerConnectionChanged("p3", false));

            AssertJson("{'locked':'A · Рейн','pending':[{'name':'Papa','characterId':1}]}", _t.Sender.MessagesTo("c1").Single()["data"]);
            Assert.That(_t.Sender.MessagesTo("c2"), Is.Empty);
            Assert.That(_t.Sender.MessagesTo("c3"), Is.Empty);
        }

        [Test]
        public void RosterChangeInAnswerRefreshesOnlyTheWaitingLists()
        {
            _t.EnterAnswer(4);
            _t.Lock(Papa, 0, 1000);
            _t.State.MutablePlayers.Remove(Kid);
            _t.Outbox.OnEvents(new IGameEvent[] { new PlayerLeft("p3", "Kid", "c3", PlayerLeft.ReasonLeft), new RosterChanged() });

            AssertJson("{'locked':'A · Rhine','pending':[{'name':'Mama','characterId':0}]}", _t.Sender.MessagesTo("c2").Single()["data"]);
            Assert.That(_t.Sender.MessagesTo("c1"), Is.Empty);
        }

        [Test]
        public void ConnectionRosterAndReadyChangesInTheLobbyResendEveryScreen()
        {
            _t.SetPhase(GamePhase.Lobby, 1, null);
            _t.Outbox.OnEvent(new PlayerConnectionChanged("p2", true));
            Assert.That(_t.Sender.MessagesTo("c1", "screen").Count, Is.EqualTo(1));
            Assert.That(_t.Sender.MessagesTo("c3", "screen").Count, Is.EqualTo(1));

            _t.Sender.Clear();
            _t.Outbox.OnEvent(new RosterChanged());
            Assert.That(_t.Sender.MessagesTo("c1", "screen").Count, Is.EqualTo(1));
            Assert.That(_t.Sender.MessagesTo("c2", "screen").Count, Is.EqualTo(1));

            _t.Sender.Clear();
            _t.Outbox.OnEvent(new ReadyChanged("p2", true));
            Assert.That(_t.Sender.MessagesTo("c1", "screen").Count, Is.EqualTo(1));
            Assert.That(_t.Sender.MessagesTo("c3", "screen").Count, Is.EqualTo(1));
        }

        [Test]
        public void RosterChangeInOtherPhasesSendsNoScreens()
        {
            _t.SetPhase(GamePhase.Results, 9, 20000);
            _t.Outbox.OnEvent(new RosterChanged());
            Assert.That(_t.Sender.Sent, Is.Empty);
        }

        [Test]
        public void CharacterTakenResendsPickCharacter()
        {
            var gran = _t.AddPlayer("p4", "Gran", "en", 5, "c4", characterPicked: false);
            _t.Outbox.OnEvents(new IGameEvent[]
            {
                new InputAcked("c4", "k1", false, AckReasons.CharacterTaken),
                new InputRefused("p4", "character", 1, AckReasons.CharacterTaken)
            });

            var messages = _t.Sender.MessagesTo("c4");
            AssertJson("{'v':1,'type':'ack','inputId':'k1','ok':false,'reason':'characterTaken'}", messages[0]);
            AssertJson("{'v':1,'type':'screen','phaseId':1,'id':'pickCharacter','endsInMs':null,'data':{'taken':[0,1,2],'current':5}}", messages[1]);
            Assert.That(messages.Count, Is.EqualTo(2));
            Assert.That(gran.CharacterPicked, Is.False);
        }

        [Test]
        public void AfterACharacterPickThePlayerGetsTheLobby()
        {
            var gran = _t.AddPlayer("p4", "Gran", "en", 5, "c4", characterPicked: false);
            gran.CharacterId = 6;
            gran.CharacterPicked = true;
            _t.Outbox.OnEvents(new IGameEvent[] { new InputAcked("c4", "k2", true, null), new PlayerUpdated("p4"), new RosterChanged() });

            var messages = _t.Sender.MessagesTo("c4");
            Assert.That(messages.Select(m => (string)m["type"]), Is.EqualTo(new[] { "ack", "player", "screen" }));
            Assert.That((int)messages[1]["player"]["characterId"], Is.EqualTo(6));
            Assert.That((string)messages[2]["id"], Is.EqualTo("lobby"));
            Assert.That(_t.Sender.MessagesTo("c1", "screen").Count, Is.EqualTo(1), "the others see the new character too");
        }

        [Test]
        public void PlayerUpdatedSendsThePlayerMessage()
        {
            Mama.Score = 850;
            Mama.Streak = 1;
            _t.Outbox.OnEvent(new PlayerUpdated("p1"));
            AssertJson(@"{'v':1,'type':'player','player':{'playerId':'p1','name':'Mama','characterId':0,'lang':'ru',
                'score':850,'rank':1,'streak':1,'heldPowerUp':null,'timerMultiplier':1.0}}",
                _t.Sender.MessagesTo("c1").Single());
            Assert.That(_t.Sender.MessagesTo("c2"), Is.Empty);
        }

        [Test]
        public void ALanguageChangeResendsTheScreenInTheNewLanguage()
        {
            _t.SetPhase(GamePhase.StartCountdown, 2, 3000);
            Mama.Lang = "en";
            _t.Outbox.OnEvents(new IGameEvent[]
            {
                new SettingsChanged(SettingsChanged.SourcePhone, "p1", "lang", "en"),
                new PlayerUpdated("p1")
            });

            var messages = _t.Sender.MessagesTo("c1");
            AssertJson("{'v':1,'type':'screen','phaseId':2,'id':'countdown','endsInMs':3000,'data':{'text':'Starting'}}", messages[0]);
            Assert.That((string)messages[1]["player"]["lang"], Is.EqualTo("en"));
            Assert.That(messages.Count, Is.EqualTo(2));
        }

        [Test]
        public void ATimerMultiplierChangeSendsOnlyThePlayerMessage()
        {
            _t.SetPhase(GamePhase.StartCountdown, 2, 3000);
            Mama.TimerMultiplier = 2;
            _t.Outbox.OnEvents(new IGameEvent[]
            {
                new SettingsChanged(SettingsChanged.SourcePhone, "p1", "timerMultiplier", "2"),
                new PlayerUpdated("p1")
            });

            var messages = _t.Sender.MessagesTo("c1");
            Assert.That(messages.Select(m => (string)m["type"]), Is.EqualTo(new[] { "player" }));
            Assert.That((double)messages[0]["player"]["timerMultiplier"], Is.EqualTo(2.0));
        }

        [Test]
        public void JoinStillGetsWelcomeAndASnapshotWithPickCharacter()
        {
            _t.AddPlayer("p4", "Gran", "ru", 3, "c4", characterPicked: false);
            _t.Outbox.OnEvent(new PlayerJoined("p4", "c4"));

            var messages = _t.Sender.MessagesTo("c4");
            AssertJson("{'v':1,'type':'welcome','playerId':'p4','token':'token_p4','room':'KXQT'}", messages[0]);
            AssertJson(@"{'v':1,'type':'snapshot',
                'player':{'playerId':'p4','name':'Gran','characterId':3,'lang':'ru','score':0,'rank':1,'streak':0,'heldPowerUp':null,'timerMultiplier':1.0},
                'screen':{'phaseId':1,'id':'pickCharacter','endsInMs':null,'data':{'taken':[0,1,2],'current':3}}}",
                messages[1]);
        }

        [Test]
        public void RejoinInAnswerRestoresTheWaitingScreenWithTheTimeLeft()
        {
            _t.EnterAnswer(4);
            _t.Lock(Kid, 2, 4000);
            _t.Clock.Advance(6000);
            _t.Outbox.OnEvent(new PlayerRejoined("p3", "c3", 3000, false));

            var snapshot = _t.Sender.MessagesTo("c3", "snapshot").Single();
            Assert.That((string)snapshot["screen"]["id"], Is.EqualTo("waiting"));
            Assert.That((long)snapshot["screen"]["endsInMs"], Is.EqualTo(9000));
        }

        [Test]
        public void SupersededSocketIsClosedWith4002()
        {
            _t.Outbox.OnEvent(new ConnectionSuperseded("p1", "c_old", "c1"));
            AssertJson("{'close':'c_old','code':4002}", _t.Sender.Frames.Single());
        }

        [Test]
        public void RefusedIsSentToTheConn()
        {
            _t.Outbox.OnEvent(new JoinRefused("c9", "nameTaken"));
            AssertJson("{'to':'c9','msg':{'v':1,'type':'refused','reason':'nameTaken'}}", _t.Sender.Frames.Single());
        }

        [Test]
        public void ADuplicateAckIsOkWithItsReason()
        {
            _t.Outbox.OnEvent(new InputAcked("c1", "a1", true, AckReasons.Duplicate));
            AssertJson("{'v':1,'type':'ack','inputId':'a1','ok':true,'reason':'duplicate'}", _t.Sender.MessagesTo("c1").Single());
        }
    }
}
