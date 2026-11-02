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
        public void LobbyAfterThePickListsThePlayersAndCanStart()
        {
            AssertJson(@"{'v':1,'type':'screen','phaseId':1,'id':'lobby','endsInMs':null,'data':{'players':[
                {'playerId':'p1','name':'Mama','characterId':0},
                {'playerId':'p2','name':'Papa','characterId':1},
                {'playerId':'p3','name':'Kid','characterId':2}],'canStart':true}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void LobbyCannotStartWithOnePlayer()
        {
            var t = new NetTestGame();
            var solo = t.AddPlayer("p1", "Mama", "ru", 0, "c1");
            AssertJson(@"{'v':1,'type':'screen','phaseId':1,'id':'lobby','endsInMs':null,'data':{'players':[
                {'playerId':'p1','name':'Mama','characterId':0}],'canStart':false}}",
                ScreenMessage(t.Screens.CurrentScreen(solo)));
        }

        [Test]
        public void LobbyBeforeThePickShowsPickCharacterWithTheOthersTaken()
        {
            var newcomer = _t.AddPlayer("p4", "Gran", "en", 5, "c4", characterPicked: false);
            AssertJson("{'v':1,'type':'screen','phaseId':1,'id':'pickCharacter','endsInMs':null,'data':{'taken':[0,1,2],'current':5}}",
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
            AssertJson("{'v':1,'type':'screen','phaseId':2,'id':'countdown','endsInMs':2000,'data':{'text':'" + text + "'}}",
                ScreenMessage(_t.Screens.CurrentScreen(player)));
        }

        [TestCase("ru", "Какая река самая длинная в Европе?")]
        [TestCase("en", "Which is the longest river in Europe?")]
        public void QuestionShowSendsGetReadyWithTheQuestion(string lang, string text)
        {
            var player = lang == "ru" ? Mama : Papa;
            _t.StartQuestion(4);
            _t.SetPhase(GamePhase.QuestionShow, 3, 4000);
            AssertJson("{'v':1,'type':'screen','phaseId':3,'id':'getReady','endsInMs':4000,'data':{'text':'" + text + "'}}",
                ScreenMessage(_t.Screens.CurrentScreen(player)));
        }

        [Test]
        public void AnswerChoiceHasTheOptionsInDisplayOrderInRussian()
        {
            _t.EnterAnswer(4);
            _t.Clock.Advance(2500);
            AssertJson(@"{'v':1,'type':'screen','phaseId':4,'id':'answerChoice','endsInMs':12500,'data':{
                'question':'Какая река самая длинная в Европе?','options':['Рейн','Волга','Днепр','Дунай'],'windowMs':15000}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void AnswerChoiceHasTheOptionsInDisplayOrderInEnglish()
        {
            _t.EnterAnswer(4);
            AssertJson(@"{'v':1,'type':'screen','phaseId':4,'id':'answerChoice','endsInMs':15000,'data':{
                'question':'Which is the longest river in Europe?','options':['Rhine','Volga','Dnieper','Danube'],'windowMs':15000}}",
                ScreenMessage(_t.Screens.CurrentScreen(Papa)));
        }

        [Test]
        public void AnswerChoiceWindowIsThePersonalWindow()
        {
            var slow = _t.AddPlayer("p4", "Gran", "en", 3, "c4", timerMultiplier: 1.5);
            _t.EnterAnswer(4);
            var screen = ScreenMessage(_t.Screens.CurrentScreen(slow));
            Assert.That((long)screen["data"]["windowMs"], Is.EqualTo(22500));
            Assert.That((long)screen["endsInMs"], Is.EqualTo(22500));
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
        public void WaitingShowsTheLockedLetterAndTextAndWhoIsStillAnswering()
        {
            _t.EnterAnswer(4);
            _t.Lock(Mama, 1, 3000);
            AssertJson(@"{'v':1,'type':'screen','phaseId':4,'id':'waiting','endsInMs':15000,'data':{
                'locked':'B · Волга','pending':[{'name':'Papa','characterId':1},{'name':'Kid','characterId':2}]}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void WaitingInEnglish()
        {
            _t.EnterAnswer(4);
            _t.Lock(Papa, 3, 3000);
            _t.Lock(Mama, 0, 3000);
            AssertJson(@"{'locked':'D · Danube','pending':[{'name':'Kid','characterId':2}]}",
                ScreenMessage(_t.Screens.CurrentScreen(Papa))["data"]);
        }

        [Test]
        public void PendingLeavesOutDisconnectedPlayersAndThosePastDeadlinePlusGrace()
        {
            _t.AddPlayer("p4", "Away", "ru", 4, "c4", connected: false);
            var slow = _t.AddPlayer("p5", "Gran", "ru", 5, "c5", timerMultiplier: 2);
            _t.EnterAnswer(4);
            _t.Lock(Mama, 0, 1000);

            // Inside the grace after the 1x deadline: Papa and Kid can still send (D73).
            _t.Clock.Advance(_t.BaseWindowMs + _t.Config.Net.InputGraceMs - 1);
            AssertJson("[{'name':'Papa','characterId':1},{'name':'Kid','characterId':2},{'name':'Gran','characterId':5}]",
                ScreenMessage(_t.Screens.CurrentScreen(Mama))["data"]["pending"]);

            // At deadline plus grace they are done; only the 2x player is still answering.
            _t.Clock.Advance(1);
            AssertJson("[{'name':'Gran','characterId':5}]", ScreenMessage(_t.Screens.CurrentScreen(Mama))["data"]["pending"]);
            Assert.That(slow.Turn.Locked, Is.False);
        }

        [TestCase(GamePhase.Reveal)]
        [TestCase(GamePhase.Scores)]
        public void ResultInRussianForACorrectAnswer(GamePhase phase)
        {
            _t.EnterAnswer(4);
            _t.Lock(Mama, 1, 2000);
            Score(Mama, 900, true, 900, 2);
            Score(Papa, 900, true, 900, 1);
            Score(Kid, 400, false, 0, 0);
            _t.SetPhase(phase, 5, 3500);
            AssertJson(@"{'v':1,'type':'screen','phaseId':5,'id':'result','endsInMs':3500,'data':{
                'correct':true,'points':900,'correctText':'Правильный ответ: B · Волга','streak':2,'rank':1,
                'powerUp':null,'targets':[]}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void ResultInEnglishForAWrongAnswerHasTheSharedRank()
        {
            _t.EnterAnswer(4);
            Score(Mama, 900, true, 900, 1);
            Score(Papa, 400, false, 0, 0);
            Score(Kid, 900, true, 900, 1);
            _t.SetPhase(GamePhase.Reveal, 5, 3500);
            AssertJson(@"{'correct':false,'points':0,'correctText':'Right answer: B · Volga','streak':0,'rank':3,
                'powerUp':null,'targets':[]}",
                ScreenMessage(_t.Screens.CurrentScreen(Papa))["data"]);
        }

        [Test]
        public void ResultsHaveThePlaceTheCorrectCountAndTheAwards()
        {
            Mama.Score = 2000;
            Mama.CorrectCount = 3;
            Papa.Score = 2000;
            Papa.CorrectCount = 3;
            Kid.Score = 500;
            Kid.CorrectCount = 1;
            _t.State.MutableMostCorrectIds.Add("p1");
            _t.State.MutableMostCorrectIds.Add("p2");
            _t.SetPhase(GamePhase.Results, 9, 20000);
            AssertJson("{'v':1,'type':'screen','phaseId':9,'id':'results','endsInMs':20000,'data':{'place':1,'correctCount':3,'awards':['Больше всех верных']}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
            AssertJson("{'place':1,'correctCount':3,'awards':['Most correct']}", ScreenMessage(_t.Screens.CurrentScreen(Papa))["data"]);
            AssertJson("{'place':3,'correctCount':1,'awards':[]}", ScreenMessage(_t.Screens.CurrentScreen(Kid))["data"]);
        }

        [Test]
        public void PlayAgainShowsPollDone()
        {
            _t.SetPhase(GamePhase.PlayAgain, 10, null);
            AssertJson("{'v':1,'type':'screen','phaseId':10,'id':'pollDone','endsInMs':null,'data':{'canPlayAgain':true}}",
                ScreenMessage(_t.Screens.CurrentScreen(Mama)));
        }

        [Test]
        public void PlayAgainNeedsTwoPlayers()
        {
            var t = new NetTestGame();
            var solo = t.AddPlayer("p1", "Mama", "ru", 0, "c1");
            t.SetPhase(GamePhase.PlayAgain, 10, null);
            AssertJson("{'canPlayAgain':false}", ScreenMessage(t.Screens.CurrentScreen(solo))["data"]);
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

        private static void Score(PlayerState player, int score, bool correct, int points, int streak)
        {
            player.Score = score;
            player.Streak = streak;
            player.Turn.Revealed = true;
            player.Turn.Correct = correct;
            player.Turn.Points = points;
        }
    }
}
