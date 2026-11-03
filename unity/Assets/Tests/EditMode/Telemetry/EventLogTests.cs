using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using FamilyQuiz.Telemetry;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests.EventLog
{
    /// <summary>The event log (docs/15_schemas.md, "Event log"): every line parsed back with Newtonsoft.</summary>
    public sealed class EventLogTests
    {
        private const string Session = "2026-11-07T19-30-00";
        private const string Build = "0.1.0";

        private string _folder;
        private string _path;
        private ManualClock _clock;
        private Game _game;
        private EventLogWriter _writer;
        private TelemetryListener _log;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "fq-telemetry-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_folder, "playtests", "2026-11-07", "events_" + Session + ".jsonl");
            _clock = new ManualClock();
            _clock.Advance(50000);
            var config = TestContent.Config();
            var real = TestContent.Content();
            _game = new Game(new GameContent(config, TestContent.Characters(config), new List<QuestionDef>(), null, real.Topics,
                    real.Race, real.Scripts), _clock,
                new SeededRandom(1), () => 42);
            _writer = new EventLogWriter(_path, Session, Build, _clock);
            _log = new TelemetryListener(_game, _writer);
        }

        [TearDown]
        public void TearDown()
        {
            _log.Dispose();
            try
            {
                if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
            }
            catch (Exception)
            {
                // A leftover temp folder is harmless.
            }
        }

        /// <summary>The file as text, read while the writer still has it open (as a log tool would).</summary>
        private string ReadAll()
        {
            using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private List<JObject> Lines()
        {
            return ReadAll().Split('\n').Where(l => l.Length > 0).Select(JObject.Parse).ToList();
        }

        private JObject LogOne(params IGameEvent[] events)
        {
            _log.OnEvents(events);
            return Lines().Single();
        }

        /// <summary>The line without t, session and build, which every line has.</summary>
        private static JObject Body(JObject line)
        {
            var copy = (JObject)line.DeepClone();
            copy.Remove("t");
            copy.Remove("session");
            copy.Remove("build");
            return copy;
        }

        private static void AssertBody(string expected, JObject line)
        {
            var want = JObject.Parse(expected);
            var got = Body(line);
            Assert.That(JToken.DeepEquals(want, got), Is.True, "expected " + want.ToString(Newtonsoft.Json.Formatting.None)
                                                               + " but was " + got.ToString(Newtonsoft.Json.Formatting.None));
            Assert.That(got.Properties().Select(p => p.Name), Is.EqualTo(want.Properties().Select(p => p.Name)), "field order");
        }

        private PlayerState Seat(string id, string name, string lang, int characterId)
        {
            var player = new PlayerState(id, name, lang, characterId, "token_" + id, 1) { Conn = "c_" + id, Connected = true };
            _game.State.MutablePlayers.Add(player);
            return player;
        }

        [Test]
        public void EveryLineStartsWithTSessionAndBuildAndTCountsFromTheSessionStart()
        {
            _clock.Advance(5210);
            _log.SessionStarted("KXQT");
            var line = Lines().Single();
            Assert.That(line.Properties().Select(p => p.Name).Take(4), Is.EqualTo(new[] { "t", "session", "build", "event" }));
            Assert.That((long)line["t"], Is.EqualTo(5210));
            Assert.That((string)line["session"], Is.EqualTo(Session));
            Assert.That((string)line["build"], Is.EqualTo(Build));
            AssertBody("{'event':'sessionStarted','room':'KXQT','configVersion':1}", line);
        }

        [Test]
        public void TheFolderIsCreatedAndEachLineIsOnDiskAtOnce()
        {
            _log.SessionStarted("KXQT");
            Assert.That(File.Exists(_path), Is.True);
            Assert.That(Lines().Count, Is.EqualTo(1), "flushed without closing the file");
            _clock.Advance(10);
            _log.Error("content", "questions.json geo_009: bad difficulty");
            Assert.That(Lines().Count, Is.EqualTo(2));
            Assert.That(ReadAll().Split('\n').Length, Is.EqualTo(3), "one object per line, each ending in a newline");
        }

        [Test]
        public void PlayerJoinedReadsTheSeat()
        {
            Seat("p1", "Мама", "ru", 0);
            AssertBody("{'event':'playerJoined','playerId':'p1','name':'Мама','lang':'ru','characterId':0}",
                LogOne(new PlayerJoined("p1", "c_p1")));
        }

        [Test]
        public void PlayerLeftCarriesTheReason()
        {
            _log.OnEvents(new IGameEvent[]
            {
                new PlayerLeft("p3", "Kid", "c3", PlayerLeft.ReasonLeft),
                new PlayerLeft("p4", "Gran", null, PlayerLeft.ReasonSeatExpired),
                new PlayerConnectionChanged("p2", false)
            });
            var lines = Lines();
            AssertBody("{'event':'playerLeft','playerId':'p3','reason':'left'}", lines[0]);
            AssertBody("{'event':'playerLeft','playerId':'p4','reason':'seatExpired'}", lines[1]);
            AssertBody("{'event':'playerLeft','playerId':'p2','reason':'disconnected'}", lines[2]);
            Assert.That(lines.Count, Is.EqualTo(3));
        }

        [Test]
        public void PlayerRejoinedHasAwayMsAndSkipsRefreshes()
        {
            _log.OnEvents(new IGameEvent[] { new PlayerConnectionChanged("p3", true), new PlayerRejoined("p3", "c9", 2900, false) });
            _log.OnEvents(new IGameEvent[] { new PlayerRejoined("p3", "c9", 0, true) });
            AssertBody("{'event':'playerRejoined','playerId':'p3','awayMs':2900}", Lines().Single());
        }

        [Test]
        public void ASocketBackWithoutARejoinCountsAsARejoin()
        {
            var player = Seat("p2", "Papa", "en", 1);
            player.DisconnectedAtMs = _clock.NowMs;
            _clock.Advance(4000);
            AssertBody("{'event':'playerRejoined','playerId':'p2','awayMs':4000}", LogOne(new PlayerConnectionChanged("p2", true)));
        }

        [Test]
        public void LobbyActionLine()
        {
            AssertBody("{'event':'lobbyAction','playerId':'p2','action':'start','ok':true}",
                LogOne(new LobbyActionHandled("p2", "start", true)));
        }

        [Test]
        public void SettingsChangedFromAPhone()
        {
            AssertBody("{'event':'settingsChanged','source':'phone','playerId':'p1','setting':'timerMultiplier','value':'1.5'}",
                LogOne(new SettingsChanged(SettingsChanged.SourcePhone, "p1", "timerMultiplier", "1.5")));
        }

        [Test]
        public void SettingsChangedFromTheTvHasNoPlayerId()
        {
            AssertBody("{'event':'settingsChanged','source':'tv','setting':'gameLang','value':'en'}",
                LogOne(new SettingsChanged(SettingsChanged.SourceTv, null, "gameLang", "en")));
        }

        [Test]
        public void GameStartedHasTheSeedBeforeTheEvent()
        {
            var line = LogOne(new GameStarted(1, 918273, GameStarted.ModeStandard, new[] { "p1", "p2", "p3" }, "ru"));
            Assert.That(line.Properties().Select(p => p.Name).Take(5), Is.EqualTo(new[] { "t", "session", "build", "seed", "event" }));
            AssertBody("{'seed':918273,'event':'gameStarted','game':1,'mode':'standard','players':['p1','p2','p3'],'settings':{'gameLang':'ru'}}",
                line);
        }

        [Test]
        public void PhaseStartedOutsideAQuestionHasNoQuestionFields()
        {
            AssertBody("{'event':'phaseStarted','phase':'StartCountdown','phaseId':2,'budgetMs':3000,'stagingExtraMs':0}",
                LogOne(new PhaseStarted(GamePhase.StartCountdown, 2, 3000, 0, 53000)));
        }

        [Test]
        public void PhaseStartedInAQuestionHasRoundSlotAndQuestionId()
        {
            var def = new QuestionDef
            {
                Id = "geo_003",
                Options = new List<LocalizedText> { new LocalizedText(), new LocalizedText(), new LocalizedText(), new LocalizedText() },
                Correct = 0
            };
            _game.State.Question = new QuestionInPlay(def, 2, 1, 0, 1, new[] { 0, 1, 2, 3 });
            AssertBody("{'event':'phaseStarted','phase':'Answer','phaseId':16,'round':1,'slot':2,'questionId':'geo_003','budgetMs':15000,'stagingExtraMs':0}",
                LogOne(new PhaseStarted(GamePhase.Answer, 16, 15000, 0, 65500)));
        }

        [Test]
        public void PhaseEndedLine()
        {
            AssertBody("{'event':'phaseEnded','phase':'Answer','phaseId':16,'reason':'allDone','durationMs':9800}",
                LogOne(new PhaseEnded(GamePhase.Answer, 16, PhaseEndReasons.AllDone, 9800)));
        }

        [Test]
        public void InputFromAnAnswerLocked()
        {
            AssertBody("{'event':'input','playerId':'p2','kind':'choice','phaseId':16,'ms':3410,'correct':true,'points':890}",
                LogOne(new AnswerLocked("p2", 16, InputKinds.Choice, 1, 3410, true, 890)));
        }

        [Test]
        public void InputRefusedLine()
        {
            AssertBody("{'event':'inputRefused','playerId':'p4','kind':'choice','phaseId':16,'reason':'closed'}",
                LogOne(new InputRefused("p4", InputKinds.Choice, 16, AckReasons.Closed)));
        }

        [Test]
        public void ScoreChangedLine()
        {
            AssertBody("{'event':'scoreChanged','playerId':'p2','delta':890,'score':2350,'rank':1}",
                LogOne(new ScoreChanged("p2", 890, 2350, 1)));
        }

        [Test]
        public void GameEndedLine()
        {
            AssertBody("{'event':'gameEnded','game':1,'reason':'finished','winner':'p2','durationMs':617000}",
                LogOne(new GameEnded(1, GameEnded.ReasonFinished, "p2", 617000)));
        }

        [Test]
        public void ErrorLine()
        {
            _log.Error("content", "questions.json geo_009: bad difficulty");
            AssertBody("{'event':'error','where':'content','message':'questions.json geo_009: bad difficulty'}", Lines().Single());
        }

        [Test]
        public void EventsWithoutALogLineWriteNothing()
        {
            _log.OnEvents(new IGameEvent[] { new RosterChanged(), new PlayerUpdated("p1"), new InputAcked("c1", "a1", true, null) });
            Assert.That(File.Exists(_path), Is.False);
        }

        [Test]
        public void AnUnwritablePathNeverThrowsAndKeepsTheFirstError()
        {
            // A file where a folder should be: creating the folder fails on every platform.
            Directory.CreateDirectory(_folder);
            var blocker = Path.Combine(_folder, "blocker");
            File.WriteAllText(blocker, "x");
            var reported = new List<string>();
            var writer = new EventLogWriter(Path.Combine(blocker, "sub", "events.jsonl"), Session, Build, _clock, reported.Add);
            using (var log = new TelemetryListener(_game, writer))
            {
                Assert.DoesNotThrow(() =>
                {
                    log.SessionStarted("KXQT");
                    log.OnEvents(new IGameEvent[] { new PhaseEnded(GamePhase.Lobby, 1, PhaseEndReasons.AllDone, 100) });
                    log.Error("relay", "down");
                });
                Assert.That(log.FirstError, Is.Not.Null);
                Assert.That(writer.LinesWritten, Is.EqualTo(0));
                Assert.That(reported.Count, Is.EqualTo(1), "the first error is reported once");
            }
        }

        [Test]
        public void WritingAfterDisposeDoesNothing()
        {
            _log.Dispose();
            Assert.DoesNotThrow(() => _log.SessionStarted("KXQT"));
            Assert.That(File.Exists(_path), Is.False);
        }
    }
}
