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
            var (questions, race) = new SeenFile(_path).Load();
            Assert.That(questions, Is.Empty);
            Assert.That(race, Is.Empty);
        }

        [Test]
        public void ABrokenFileIsReportedAndMeansNothingSeen()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(_path, "{ not json");
            var errors = new List<string>();
            var (questions, _) = new SeenFile(_path, errors.Add).Load();
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
            Assert.That(File.Exists(_path), Is.True);

            // The app restarts: the next Game starts from the file.
            var restarted = new SeenFile(_path);
            var (questions, race) = restarted.Load();
            Assert.That(questions, Is.EquivalentTo(seenQuestions));
            Assert.That(race, Is.EquivalentTo(seenRace));
            var second = new LobbyFixture(4, seenQuestions: questions, seenRace: race);
            restarted.Attach(second.Game);
            Assert.That(second.State.SeenQuestionIds, Is.EquivalentTo(seenQuestions));

            // Reset seen questions: the file is cleared at once.
            restarted.OnEvents(second.Send(new ResetSeen()));
            var (afterQ, afterR) = new SeenFile(_path).Load();
            Assert.That(afterQ, Is.Empty);
            Assert.That(afterR, Is.Empty);
            Assert.That(File.Exists(_path + ".tmp"), Is.False);
        }
    }
}
