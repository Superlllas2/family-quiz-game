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
    /// The pause menu and question feedback (decisions/0018, D105 to D112; PROTOCOL.md sections 3 and 4): the feedback
    /// setting, the rating card on the phones in Reveal and Scores, the rate input and its refusals, the paused screen,
    /// and New game. Real content and config, phones through the router and the outbox.
    /// </summary>
    public sealed class PauseAndFeedbackTests
    {
        private LobbyFixture _f;
        private CapturingRelaySender _sender;
        private PhoneOutbox _outbox;
        private ProtocolRouter _router;
        private int _rateIds;

        [SetUp]
        public void SetUp()
        {
            // Power ups off, so every phone gets watch in Reveal (the result screen has its own test below).
            Use(new LobbyFixture(content: TestContent.WithConfig(c => c["powerUps"]["enabled"] = false)));
        }

        private void Use(LobbyFixture f)
        {
            _f = f;
            _sender = new CapturingRelaySender();
            _outbox = new PhoneOutbox(_f.Game, _sender, () => "ABCD", null);
            _router = new ProtocolRouter(_sender, _f.Clock, null);
        }

        private IReadOnlyList<IGameEvent> Do(IReadOnlyList<IGameEvent> events)
        {
            _outbox.OnEvents(events);
            return events;
        }

        private IReadOnlyList<IGameEvent> Tv(IGameCommand command)
        {
            return Do(_f.Game.Handle(command));
        }

        private IReadOnlyList<IGameEvent> Phone(string conn, string json)
        {
            var envelope = new JObject { ["relay"] = "message", ["conn"] = conn, ["msg"] = JObject.Parse(json) };
            var command = _router.ToCommand(RelayInboxItem.Message(envelope.ToString(Newtonsoft.Json.Formatting.None)));
            Assert.That(command, Is.Not.Null, json);
            return Do(_f.Game.Handle(command));
        }

        private IReadOnlyList<IGameEvent> Rate(string conn, string item, object stars, long? phaseId = null)
        {
            var starsJson = stars is string s ? "'" + s + "'" : System.Convert.ToString(stars, System.Globalization.CultureInfo.InvariantCulture);
            return Phone(conn, "{'v':2,'type':'input','inputId':'r" + (++_rateIds) + "','phaseId':" + (phaseId ?? _f.State.PhaseId)
                               + ",'kind':'rate','item':'" + item + "','stars':" + starsJson + "}");
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

        private void Join()
        {
            foreach (var (conn, name, lang) in new[] { ("c1", "Mama", "ru"), ("c2", "Papa", "en"), ("c3", "Kid", "ru") })
            {
                Tv(new ConnectionOpened(conn));
                Tv(new PlayerJoin(conn, name, lang));
            }
        }

        /// <summary>Mama (c1, ru) right, Papa (c2, en) wrong, Kid (c3) does not answer question 1: Reveal is open.</summary>
        private void ToFirstReveal(bool feedback = true)
        {
            Join();
            if (feedback) Tv(new ChangeTvSettings(feedbackEnabled: true));
            Do(_f.ReadyAll());
            RunUntil(GamePhase.Answer);
            Do(_f.Choice("c1", _f.Right, 500));
            Do(_f.Choice("c2", _f.Wrong, 500));
            RunUntil(GamePhase.Reveal);
        }

        // ------------------------------------------------------------ the setting (D108)

        [Test]
        public void FeedbackStartsAtTheConfigDefaultAndTheSettingIsLogged()
        {
            Assert.That(_f.State.FeedbackEnabled, Is.EqualTo(_f.Config.Feedback.Enabled));
            Assert.That(_f.Config.Feedback.Enabled, Is.False, "off until switched on in the TV settings panel");
            var on = Tv(new ChangeTvSettings(feedbackEnabled: true));
            Assert.That(_f.State.FeedbackEnabled, Is.True);
            var changed = on.OfType<SettingsChanged>().Single();
            Assert.That(changed.Setting, Is.EqualTo(SettingsChanged.SettingFeedback));
            Assert.That(changed.Value, Is.EqualTo("true"));
            Assert.That(changed.Source, Is.EqualTo(SettingsChanged.SourceTv));
            Assert.That(Tv(new ChangeTvSettings(feedbackEnabled: true)).OfType<SettingsChanged>(), Is.Empty, "no change, no event");
        }

        // ------------------------------------------------------------ the rating card (D109, D110)

        [Test]
        public void EveryPhoneGetsTheRatingCardInRevealWithItsLanguage()
        {
            ToFirstReveal();
            var mama = Last("c1");
            Assert.That((string)mama["id"], Is.EqualTo("watch"));
            var rate = mama["data"]["rate"];
            Assert.That((string)rate["questionId"], Is.EqualTo(_f.State.Question.Def.Id));
            Assert.That((string)rate["title"], Is.EqualTo("Оцени вопрос"));
            Assert.That((string)rate["close"], Is.EqualTo("Закрыть"));
            Assert.That((int)rate["stars"], Is.EqualTo(5));
            Assert.That(rate["items"].Select(i => (string)i["id"]), Is.EqualTo(new[] { "fun", "guess", "fact" }));
            Assert.That((string)rate["items"][0]["text"], Is.EqualTo("Понравился вопрос?"));
            Assert.That((string)rate["items"][0]["low"], Is.EqualTo("Скучно"));
            Assert.That((string)rate["items"][0]["high"], Is.EqualTo("Отлично"));
            Assert.That(rate["items"].All(i => (int)i["given"] == 0), Is.True);
            Assert.That((string)Last("c2")["data"]["rate"]["items"][1]["text"], Is.EqualTo("Could you work it out without knowing?"));
            Assert.That(Last("c3")["data"]["rate"], Is.Not.Null, "players who did not answer rate too");
        }

        [Test]
        public void APowerUpHolderGetsTheCardOnTheResultScreen()
        {
            Use(new LobbyFixture(content: TestContent.WithConfig(c => c["powerUps"]["drawChanceLeader"] = 1.0)));
            ToFirstReveal();
            var holder = _f.State.Players.First(p => p.HeldPowerUp != null);
            var screen = Last(holder.Conn);
            Assert.That((string)screen["id"], Is.EqualTo("result"));
            Assert.That(screen["data"]["targets"], Is.Not.Empty, "the pick is still there");
            Assert.That((string)screen["data"]["rate"]["questionId"], Is.EqualTo(_f.State.Question.Def.Id));
        }

        [Test]
        public void WithFeedbackOffThereIsNoCardAndNoRateField()
        {
            ToFirstReveal(feedback: false);
            var screen = Last("c1");
            Assert.That((string)screen["id"], Is.EqualTo("watch"));
            Assert.That(((JObject)screen["data"]).Properties(), Is.Empty, "watch stays {} without feedback");
            Assert.That(_f.Game.RatingOpen, Is.False);
        }

        [Test]
        public void SwitchingFeedbackOnInRevealSendsTheCardAtOnce()
        {
            ToFirstReveal(feedback: false);
            Tv(new ChangeTvSettings(feedbackEnabled: true));
            Assert.That(Last("c1")["data"]["rate"], Is.Not.Null);
        }

        [Test]
        public void ATapIsAckedLoggedWithTheQuestionAndShownAsGiven()
        {
            ToFirstReveal();
            var events = Rate("c1", "fun", 4);
            Assert.That((bool)Last("c1", "ack")["ok"], Is.True);
            var rated = events.OfType<QuestionRated>().Single();
            var def = _f.State.Question.Def;
            Assert.That(rated.PlayerId, Is.EqualTo("p1"));
            Assert.That(rated.QuestionId, Is.EqualTo(def.Id));
            Assert.That(rated.Topic, Is.EqualTo(def.Topic));
            Assert.That(rated.Format, Is.EqualTo(def.Format));
            Assert.That(rated.Difficulty, Is.EqualTo(def.Difficulty));
            Assert.That(rated.Item, Is.EqualTo("fun"));
            Assert.That(rated.Stars, Is.EqualTo(4));
            Assert.That(rated.Correct, Is.True, "Mama answered right");
            Assert.That((int)Last("c1")["data"]["rate"]["items"][0]["given"], Is.EqualTo(4));
            Assert.That(Rate("c2", "fact", 1).OfType<QuestionRated>().Single().Correct, Is.False, "Papa answered wrong");
            Assert.That(Rate("c3", "guess", 5).OfType<QuestionRated>().Single().Correct, Is.Null, "Kid sent no answer");
        }

        [Test]
        public void ALaterTapReplacesTheEarlierOneAndScoresKeepsTheCard()
        {
            ToFirstReveal();
            long phaseId = _f.State.PhaseId;
            Rate("c1", "fun", 2);
            Step(_f.RevealMs);
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Scores));
            Assert.That(_f.State.PhaseId, Is.EqualTo(phaseId), "Reveal and Scores share one phaseId");
            Assert.That((int)Last("c1")["data"]["rate"]["items"][0]["given"], Is.EqualTo(2), "the card stays in Scores");
            Assert.That(Rate("c1", "fun", 5, phaseId).OfType<QuestionRated>().Single().Stars, Is.EqualTo(5));
            Assert.That(_f.State.Question.GivenRating("p1", "fun"), Is.EqualTo(5));
            Assert.That((int)Last("c1")["data"]["rate"]["items"][0]["given"], Is.EqualTo(5));
        }

        [TestCase("nope", 3)]
        [TestCase("fun", 0)]
        [TestCase("fun", 6)]
        [TestCase("fun", 2.5)]
        [TestCase("fun", "4")]
        public void BadItemsAndStarsAreInvalid(string item, object stars)
        {
            ToFirstReveal();
            var events = Rate("c1", item, stars);
            Assert.That(events.OfType<QuestionRated>(), Is.Empty);
            var ack = Last("c1", "ack");
            Assert.That((bool)ack["ok"], Is.False);
            Assert.That((string)ack["reason"], Is.EqualTo(AckReasons.Invalid));
            Assert.That(events.OfType<InputRefused>().Single().Kind, Is.EqualTo(InputKinds.Rate));
        }

        [Test]
        public void RatingsWithFeedbackOffOrOutsideRevealAndScoresAreRefused()
        {
            ToFirstReveal(feedback: false);
            Rate("c1", "fun", 4);
            Assert.That((string)Last("c1", "ack")["reason"], Is.EqualTo(AckReasons.Invalid), "feedback off");

            Tv(new ChangeTvSettings(feedbackEnabled: true));
            long scoresPhaseId = _f.State.PhaseId;
            RunUntil(GamePhase.QuestionShow);
            Rate("c1", "fun", 4, scoresPhaseId);
            Assert.That((string)Last("c1", "ack")["reason"], Is.EqualTo(AckReasons.Closed), "the Scores that just ended");
            Rate("c1", "fun", 4, scoresPhaseId - 5);
            Assert.That((string)Last("c1", "ack")["reason"], Is.EqualTo(AckReasons.Stale));
            Rate("c1", "fun", 4);
            Assert.That((string)Last("c1", "ack")["reason"], Is.EqualTo(AckReasons.Invalid), "no card in QuestionShow");
        }

        [Test]
        public void TheCardNeverHoldsAPhaseUp()
        {
            ToFirstReveal();
            Assert.That(_f.Game.AnyoneCanAnswer, Is.False, "nobody can hold Reveal up by not rating");
            long revealStart = _f.State.PhaseStartedAtMs;
            RunUntil(GamePhase.Scores);
            Assert.That(_f.State.PhaseStartedAtMs - revealStart, Is.EqualTo(_f.RevealMs), "Reveal runs on its own timer");
        }

        [Test]
        public void TheRaceGetsNoCard()
        {
            ToFirstReveal();
            RunUntil(GamePhase.RaceQuestion);
            RunUntil(GamePhase.RaceReveal);
            Assert.That(_f.Game.RatingOpen, Is.False);
            Assert.That(Last("c1")["data"]["rate"], Is.Null);
        }

        // ------------------------------------------------------------ the pause (D105, D112)

        [Test]
        public void PausingSendsPausedToEveryPhoneAndResumingSendsTheirScreensBack()
        {
            Join();
            Do(_f.ReadyAll());
            RunUntil(GamePhase.Answer);
            long phaseId = _f.State.PhaseId;
            var paused = Tv(new PauseGame());
            Assert.That(_f.State.Paused, Is.True);
            Assert.That(paused.OfType<GamePaused>().Single().PhaseId, Is.EqualTo(phaseId));
            foreach (var conn in new[] { "c1", "c2", "c3" })
            {
                var screen = Last(conn);
                Assert.That((string)screen["id"], Is.EqualTo("paused"));
                Assert.That((long)screen["phaseId"], Is.EqualTo(phaseId));
            }

            Assert.That((string)Last("c1")["data"]["text"], Is.EqualTo("Пауза. Смотри на телевизор"));
            Assert.That((string)Last("c2")["data"]["text"], Is.EqualTo("Paused. Eyes on the TV"));
            Assert.That(Tv(new PauseGame()).OfType<GamePaused>(), Is.Empty, "already paused");

            var resumed = Tv(new ResumeGame());
            Assert.That(_f.State.Paused, Is.False);
            Assert.That(resumed.OfType<GameResumed>().Single().PhaseId, Is.EqualTo(phaseId));
            Assert.That((string)Last("c1")["id"], Is.EqualTo("answerChoice"));
            Assert.That((long)Last("c1")["phaseId"], Is.EqualTo(phaseId), "the same phaseId: the phone keeps its answer clock");
            Assert.That(Tv(new ResumeGame()).OfType<GameResumed>(), Is.Empty, "nothing to resume");
        }

        [Test]
        public void APhoneThatJoinsWhilePausedSeesPaused()
        {
            Tv(new PauseGame());
            Tv(new ConnectionOpened("c9"));
            Tv(new PlayerJoin("c9", "Gran", "ru"));
            Assert.That((string)Last("c9")["id"], Is.EqualTo("paused"));
        }

        // ------------------------------------------------------------ New game (D107)

        [Test]
        public void NewGameAbandonsTheGameAndStartsTheCountdownWithTheSamePlayers()
        {
            ToFirstReveal();
            int gameNumber = _f.State.GameNumber;
            Tv(new PauseGame());
            var events = Tv(new RestartGame());
            Assert.That(_f.State.Paused, Is.False, "New game resumes");
            Assert.That(events.OfType<GameResumed>().Count(), Is.EqualTo(1));
            var ended = events.OfType<GameEnded>().Single();
            Assert.That(ended.Reason, Is.EqualTo(GameEnded.ReasonRestart));
            Assert.That(ended.Game, Is.EqualTo(gameNumber));
            Assert.That(events.OfType<SettingsChanged>().Single().Setting, Is.EqualTo(SettingsChanged.SettingNewGame));
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.StartCountdown), "everyone with a character is ready at once");
            Assert.That(_f.State.Players.All(p => p.Score == 0), Is.True);
            Assert.That((string)Last("c1")["id"], Is.EqualTo("countdown"));

            Step(_f.CountdownMs);
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.TopicVote), "the next game skips the Intro");
            Assert.That(_f.State.GameNumber, Is.EqualTo(gameNumber + 1));
            Assert.That(_f.State.Players.Count, Is.EqualTo(3), "the same seats");
        }

        [Test]
        public void NewGameWithTooFewPlayersWaitsInTheLobby()
        {
            ToFirstReveal();
            Tv(new ConnectionClosed("c2"));
            Tv(new ConnectionClosed("c3"));
            Tv(new RestartGame());
            Assert.That(_f.State.Phase, Is.EqualTo(GamePhase.Lobby));
        }

        [Test]
        public void NewGameInTheLobbyDoesNothing()
        {
            Join();
            long phaseId = _f.State.PhaseId;
            var events = Tv(new RestartGame());
            Assert.That(events.OfType<GameEnded>(), Is.Empty);
            Assert.That(events.OfType<ReadyChanged>(), Is.Empty, "nobody is marked ready by a New game in the Lobby");
            Assert.That(_f.State.PhaseId, Is.EqualTo(phaseId));
        }
    }
}
