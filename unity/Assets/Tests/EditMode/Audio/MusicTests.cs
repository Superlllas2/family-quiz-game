using System.Collections.Generic;
using System.IO;
using System.Linq;
using FamilyQuiz.Audio;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using NUnit.Framework;

namespace FamilyQuiz.Tests.Audio
{
    /// <summary>The background music (decisions/0019, D113 to D115): playlist order, the remembered volume, the files and credits.</summary>
    public sealed class MusicTests
    {
        private string _folder;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "fq_music_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(42)]
        public void EveryTrackPlaysOncePerRoundAndNeverTwiceInARow(int seed)
        {
            var playlist = new MusicPlaylist(5, new SeededRandom(seed));
            var order = Enumerable.Range(0, 1000).Select(_ => playlist.Next()).ToList();
            for (int round = 0; round < order.Count / 5; round++)
                Assert.That(order.Skip(round * 5).Take(5).OrderBy(i => i), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }), "round " + round);
            for (int i = 1; i < order.Count; i++) Assert.That(order[i], Is.Not.EqualTo(order[i - 1]), "at " + i);
        }

        [Test]
        public void TheOrderChangesFromRoundToRound()
        {
            var playlist = new MusicPlaylist(5, new SeededRandom(7));
            var rounds = new HashSet<string>();
            for (int r = 0; r < 20; r++) rounds.Add(string.Join(",", Enumerable.Range(0, 5).Select(_ => playlist.Next())));
            Assert.That(rounds.Count, Is.GreaterThan(5));
        }

        [Test]
        public void OneTrackSimplyRepeats()
        {
            var playlist = new MusicPlaylist(1, new SeededRandom(1));
            Assert.That(Enumerable.Range(0, 3).Select(_ => playlist.Next()), Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void TheVolumeIsRememberedOnThisLaptop()
        {
            var path = Path.Combine(_folder, MusicVolumeFile.FileName);
            Assert.That(new MusicVolumeFile(path, 0.35f).Load(), Is.EqualTo(0.35f), "no file: the config default");
            new MusicVolumeFile(path, 0.35f).Save(0.8f);
            Assert.That(new MusicVolumeFile(path, 0.35f).Load(), Is.EqualTo(0.8f).Within(0.001f));
            new MusicVolumeFile(path, 0.35f).Save(0f);
            Assert.That(new MusicVolumeFile(path, 0.35f).Load(), Is.EqualTo(0f), "0 mutes and is kept");
        }

        [TestCase("not json")]
        [TestCase("{\"musicVolume\": 3}")]
        [TestCase("{\"musicVolume\": \"loud\"}")]
        [TestCase("{}")]
        public void ABrokenFileReadsAsTheDefault(string text)
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, MusicVolumeFile.FileName);
            File.WriteAllText(path, text);
            Assert.That(new MusicVolumeFile(path, 0.35f).Load(), Is.EqualTo(0.35f));
        }

        [Test]
        public void TheConfigHoldsTheDefaultsAndChecksThem()
        {
            var music = TestContent.Config().Music;
            Assert.That(music.Volume, Is.InRange(0.0, 1.0));
            Assert.That(music.FadeSeconds, Is.GreaterThan(0));
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["music"]["volume"] = 1.5));
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["music"]["fadeSeconds"] = -1));
        }

        [Test]
        public void EveryTrackHasItsFileAndALicenseLogLine()
        {
            var assets = Path.GetFullPath(Path.Combine(TestContent.Folder, "..", "unity", "Assets", "Audio", "Placeholder"));
            var licenses = File.ReadAllText(Path.Combine(assets, "LICENSES.md"));
            var audioSpec = File.ReadAllText(Path.Combine(TestContent.Folder, "..", "docs", "12_audio.md"));
            Assert.That(MusicTrack.All.Count, Is.EqualTo(5));
            foreach (var track in MusicTrack.All)
            {
                Assert.That(File.Exists(Path.Combine(assets, "Resources", "Music", track.Resource + ".mp3")), Is.True, track.Resource);
                Assert.That(licenses, Does.Contain(track.Resource + ".mp3").And.Contain(track.Title), track.Title);
                Assert.That(audioSpec, Does.Contain(track.Resource + ".mp3"), "docs/12_audio.md license log: " + track.Title);
                Assert.That(track.License, Is.EqualTo("CC BY 4.0"), "only CC0, CC BY or the Pixabay license (docs/12_audio.md)");
            }
        }
    }
}
