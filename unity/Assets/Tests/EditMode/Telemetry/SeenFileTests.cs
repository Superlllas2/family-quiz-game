using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.State;
using FamilyQuiz.Telemetry;
using NUnit.Framework;

namespace FamilyQuiz.Tests.Telemetry
{
    /// <summary>The seen lists on disk survive an app restart and a reset clears them (docs/02_show_flow.md, "Seen questions").</summary>
    public sealed class SeenFileTests
    {
        private string _folder;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "fq-seen-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_folder, SeenFile.FileName);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
            }
            catch (Exception)
            {
                // A leftover temp folder is harmless.
            }
        }

        [Test]
        public void AMissingFileMeansNothingSeen()
        {
            var (questions, race, _) = new SeenFile(_path).Load();
            Assert.That(questions, Is.Empty);
            Assert.That(race, Is.Empty);
        }

        [Test]
        public void AFileFromBeforeFactsLoadsWithNoFacts()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(_path, "{\"questions\": [\"geo_001\"], \"race\": [\"race_001\"]}");
            var (questions, race, facts) = new SeenFile(_path).Load();
            Assert.That(questions, Is.EqualTo(new[] { "geo_001" }));
            Assert.That(race, Is.EqualTo(new[] { "race_001" }));
            Assert.That(facts, Is.Empty);
        }

        [Test]
        public void ABrokenFileIsReportedAndMeansNothingSeen()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(_path, "{ not json");
            var errors = new List<string>();
            var (questions, _, _) = new SeenFile(_path, errors.Add).Load();
            Assert.That(questions, Is.Empty);
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheListsSurviveAnAppRestartAndAResetClearsThem()
        {
            // First session: one game, the listener saves on every SeenChanged.
            var file = new SeenFile(_path);
            var first = new LobbyFixture(3);
            file.Attach(first.Game);
            first.SeatPlayers(2);
            file.OnEvents(first.ReadyAll());
            while (first.State.Phase != GamePhase.Lobby || first.State.GameNumber == 0) file.OnEvents(first.Advance(100));
            var seenQuestions = first.State.SeenQuestionIds.ToList();
            var seenRace = first.State.SeenRaceIds.ToList();
            var seenFacts = first.State.SeenFactIds.ToList();
            Assert.That(seenFacts.Count, Is.EqualTo(1), "the winner's fact (D87)");
            Assert.That(File.Exists(_path), Is.True);

            // The app restarts: the next Game starts from the file.
            var restarted = new SeenFile(_path);
            var (questions, race, facts) = restarted.Load();
            Assert.That(facts, Is.EquivalentTo(seenFacts));
            Assert.That(questions, Is.EquivalentTo(seenQuestions));
            Assert.That(race, Is.EquivalentTo(seenRace));
            var second = new LobbyFixture(4, seenQuestions: questions, seenRace: race);
            restarted.Attach(second.Game);
            Assert.That(second.State.SeenQuestionIds, Is.EquivalentTo(seenQuestions));

            // Reset seen questions: the file is cleared at once.
            restarted.OnEvents(second.Send(new ResetSeen()));
            var (afterQ, afterR, afterF) = new SeenFile(_path).Load();
            Assert.That(afterF, Is.Empty);
            Assert.That(afterQ, Is.Empty);
            Assert.That(afterR, Is.Empty);
            Assert.That(File.Exists(_path + ".tmp"), Is.False);
        }
    }
}
