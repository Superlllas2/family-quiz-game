using FamilyQuiz.Core.State;
using NUnit.Framework;
using static FamilyQuiz.Tests.NetScreens.NetJson;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>screen.data per screen id (PROTOCOL.md section 4, docs/06_phone_screens.md), exact JSON in both languages.</summary>
    public sealed class ScreenBuilderTests
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
        public void LobbyAfterThePickListsThePlayersWithTheirReadyMarks()
        {
            Papa.Ready = true;
            AssertJson(@"{'v':2,'type':'screen','phaseId':1,'id':'lobby','endsInMs':null,'data':{'players':[
                {'playerId':'p1','name':'Mama','characterId':0,'ready':false},
                {'playerId':'p2','name':'Papa','characterId':1,'ready':true},
                {'playerId':'p3','name':'Kid','characterId':2,'ready':false}],'ready':false,'enoughPlayers':true}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
            Assert.That((bool)ScreenMessage(_t.Screens.CurrentScreen(Papa))["data"]["ready"], Is.True, "ready is this player's own mark");
        }

        [Test]
        public void LobbyWithOneConnectedPlayerIsNotEnough()
        {
            var t = new NetTestGame();
            var solo = t.AddPlayer("p1", "Mama", "ru", 0, "c1");
            AssertJson(@"{'v':2,'type':'screen','phaseId':1,'id':'lobby','endsInMs':null,'data':{'players':[
                {'playerId':'p1','name':'Mama','characterId':0,'ready':false}],'ready':false,'enoughPlayers':false}}",
                ScreenMessage(t.Screens.CurrentScreen(solo)));
        }

        [Test]
        public void EnoughPlayersCountsOnlyConnectedPlayers()
        {
            Papa.Connected = false;
            Kid.Connected = false;
            Assert.That((bool)ScreenMessage(_t.Screens.CurrentScreen(Mama))["data"]["enoughPlayers"], Is.False);
        }

        [Test]
        public void LobbyBeforeThePickShowsPickCharacterWithTheOthersTaken()
        {
            var newcomer = _t.AddPlayer("p4", "Gran", "en", 5, "c4", characterPicked: false);
            AssertJson("{'v':2,'type':'screen','phaseId':1,'id':'pickCharacter','endsInMs':null,'data':{'taken':[0,1,2],'current':5}}",
                ScreenMessage(_t.Screens.CurrentScreen(newcomer)));
        }

        [Test]
        public void TakenIncludesDisconnectedSeats()
        {
            _t.AddPlayer("p4", "Away", "ru", 7, "c4", connected: false);
            var newcomer = _t.AddPlayer("p5", "Gran", "ru", 3, "c5", characterPicked: false);
            AssertJson("{'taken':[0,1,2,7],'current':3}", ScreenMessage(_t.Screens.CurrentScreen(newcomer))["data"]);
        }

        [TestCase("ru", "Игра начинается")]
        [TestCase("en", "Starting")]
        public void CountdownCarriesTheTextAndTheTimeLeft(string lang, string text)
        {
            var player = lang == "ru" ? Mama : Papa;
            _t.SetPhase(GamePhase.StartCountdown, 2, 3000);
            _t.Clock.Advance(1000);
            AssertJson("{'v':2,'type':'screen','phaseId':2,'id':'countdown','endsInMs':2000,'data':{'text':'" + text + "'}}",
                ScreenMessage(_t.Screens.CurrentScreen(player)));
        }

        [TestCase(GamePhase.Intro)]
        [TestCase(GamePhase.QuestionShow)]
        [TestCase(GamePhase.Reveal)]
        [TestCase(GamePhase.Scores)]
        [TestCase(GamePhase.RoundEnd)]
        [TestCase(GamePhase.FinalIntro)]
        [TestCase(GamePhase.RaceReveal)]
        public void PhasesWithoutADecisionSendWatch(GamePhase phase)
        {
            // The phone is for decisions only (D88): no question, result, rank or track on it.
            _t.StartQuestion(4);
            _t.SetPhase(phase, 3, 4000);
            AssertJson("{'v':2,'type':'screen','phaseId':3,'id':'watch','endsInMs':4000,'data':{}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void AnswerChoiceHasTheOptionsInDisplayOrderInRussianAndNoQuestion()
        {
            _t.EnterAnswer(4);
            _t.Clock.Advance(2500);
            AssertJson(@"{'v':2,'type':'screen','phaseId':4,'id':'answerChoice','endsInMs':" + (_t.BaseWindowMs - 2500) + @",'data':{
                'options':['Рейн','Волга','Днепр','Дунай'],'windowMs':" + _t.BaseWindowMs + @"}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void AnswerChoiceHasTheOptionsInDisplayOrderInEnglish()
        {
            _t.EnterAnswer(4);
            AssertJson(@"{'v':2,'type':'screen','phaseId':4,'id':'answerChoice','endsInMs':" + _t.BaseWindowMs + @",'data':{
                'options':['Rhine','Volga','Dnieper','Danube'],'windowMs':" + _t.BaseWindowMs + @"}}",
                ScreenMessage(_t.Screens.CurrentScreen(Papa)));
        }

        [Test]
        public void AnswerChoiceWindowIsThePersonalWindow()
        {
            var slow = _t.AddPlayer("p4", "Gran", "en", 3, "c4", timerMultiplier: 1.5);
            _t.EnterAnswer(4);
            var screen = ScreenMessage(_t.Screens.CurrentScreen(slow));
            Assert.That((long)screen["data"]["windowMs"], Is.EqualTo(_t.BaseWindowMs * 3 / 2));
            Assert.That((long)screen["endsInMs"], Is.EqualTo(_t.BaseWindowMs * 3 / 2));
        }

        [Test]
        public void APlayerPastTheDeadlineWithoutAnAnswerKeepsAnswerChoiceAtZero()
        {
            _t.EnterAnswer(4);
            _t.Clock.Advance(_t.BaseWindowMs + 200);
            var screen = ScreenMessage(_t.Screens.CurrentScreen(Mama));
            Assert.That((string)screen["id"], Is.EqualTo("answerChoice"));
            Assert.That((long)screen["endsInMs"], Is.EqualTo(0));
            Assert.That((long)screen["phaseId"], Is.EqualTo(4));
        }

        [Test]
        public void WaitingShowsOnlyTheLockedLetterAndText()
        {
            _t.EnterAnswer(4);
            _t.Lock(Mama, 1, 3000);
            AssertJson(@"{'v':2,'type':'screen','phaseId':4,'id':'waiting','endsInMs':" + _t.BaseWindowMs + @",'data':{'locked':'B · Волга'}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void WaitingInEnglish()
        {
            _t.EnterAnswer(4);
            _t.Lock(Papa, 3, 3000);
            AssertJson(@"{'locked':'D · Danube'}", ScreenMessage(_t.Screens.CurrentScreen(Papa))["data"]);
        }

        [Test]
        public void ResultsSendTheFactToTheWinnerAndWatchToEveryoneElse()
        {
            _t.State.WinnerId = "p2";
            _t.State.WinnerFact = new FamilyQuiz.Core.Content.FactDef
            {
                Id = "f001",
                Text = new FamilyQuiz.Core.Content.LocalizedText { Ru = "У осьминога три сердца.", En = "An octopus has three hearts." },
                Source = "s"
            };
            _t.SetPhase(GamePhase.Results, 9, 20000);
            AssertJson("{'v':2,'type':'screen','phaseId':9,'id':'results','endsInMs':20000,'data':{'fact':'An octopus has three hearts.'}}",
                ScreenMessage(_t.Screens.CurrentScreen(Papa)));
            AssertJson("{'v':2,'type':'screen','phaseId':9,'id':'watch','endsInMs':20000,'data':{}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
            Mama.Lang = "ru";
            _t.State.WinnerId = "p1";
            AssertJson("{'fact':'У осьминога три сердца.'}", ScreenMessage(_t.Screens.CurrentScreen(Mama))["data"]);
        }

        [Test]
        public void PlayerObjectHasEveryField()
        {
            Mama.Score = 2350;
            Mama.Streak = 1;
            Mama.TimerMultiplier = 1.5;
            Papa.Score = 3000;
            AssertJson(@"{'playerId':'p1','name':'Mama','characterId':0,'lang':'ru','score':2350,'rank':2,'streak':1,
                'heldPowerUp':null,'timerMultiplier':1.5}",
                Newtonsoft.Json.Linq.JObject.Parse(FamilyQuiz.Net.ProtocolJson.Serialize(_t.Screens.Player(Mama))));
        }

    }
}
