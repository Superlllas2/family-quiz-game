using System.IO;
using System.Linq;
using FamilyQuiz.Audio;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests.Audio
{
    /// <summary>The sound effects and host voices (decisions/0020, D120, D121).</summary>
    public sealed class SfxTests
    {
        private static string Assets => Path.GetFullPath(Path.Combine(TestContent.Folder, "..", "unity", "Assets", "Audio", "Sfx"));

        [Test]
        public void EveryCueAndVoiceHasItsFileAndALicenseLine()
        {
            var licenses = File.ReadAllText(Path.Combine(Assets, "LICENSES.md"));
            foreach (var cue in SfxCues.All)
            {
                Assert.That(File.Exists(Path.Combine(Assets, "Resources", "Sfx", "sfx_" + cue + ".ogg")), Is.True, cue);
                Assert.That(licenses, Does.Contain("sfx_" + cue + ".ogg"), cue);
            }

            foreach (var style in SfxConfig.Voices.Where(v => v != SfxPlayer.NoVoice))
            foreach (var host in new[] { HostIds.Konstantin, HostIds.Ilja })
                Assert.That(File.Exists(Path.Combine(Assets, "Resources", "Voices", "voice_" + style + "_" + host + ".wav")), Is.True,
                    style + " " + host);
        }

        [Test]
        public void TheConfigChecksTheVoiceAndTheVolume()
        {
            Assert.That(TestContent.Config().Sfx.HostVoice, Is.EqualTo("e_bubbles"), "Ilia picked E, soft bubbles");
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["sfx"]["hostVoice"] = "opera"));
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["sfx"]["volume"] = 2));
        }

        [Test]
        public void ARealGamePlaysItsCues()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            Assert.That(SfxCues.For(f.Join("c9", "Gran"), f.State), Does.Contain(SfxCues.Join));
            var start = f.ReadyAll();
            Assert.That(SfxCues.For(start, f.State), Does.Contain(SfxCues.Countdown));
            var toVote = f.RunUntilPhase(GamePhase.TopicVote);
            Assert.That(SfxCues.For(toVote, f.State), Does.Contain(SfxCues.Start));
            var toAnswer = f.RunUntilPhase(GamePhase.Answer);
            Assert.That(SfxCues.For(toAnswer, f.State), Does.Contain(SfxCues.Topic).And.Contain(SfxCues.Card));
            var locked = f.Choice(seats[0].Conn, f.Right, 1000);
            Assert.That(SfxCues.For(locked, f.State), Is.EqualTo(new[] { SfxCues.Lock }));
            var reveal = f.RunUntilPhase(GamePhase.Reveal);
            Assert.That(SfxCues.For(reveal, f.State), Does.Contain(SfxCues.Right), "Mama was right");
            var scores = f.RunUntilPhase(GamePhase.Scores);
            Assert.That(SfxCues.For(scores, f.State), Does.Contain(SfxCues.Scores));
        }

        [Test]
        public void NobodyRightPlaysTheBong()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            f.Choice(seats[0].Conn, f.Wrong, 1000);
            var reveal = f.Choice(seats[1].Conn, f.Wrong, 1000);
            Assert.That(SfxCues.For(reveal, f.State), Does.Contain(SfxCues.Nobody).And.Not.Contain(SfxCues.Right));
        }

        [Test]
        public void PauseAndPowerUpsHaveTheirCues()
        {
            var state = new GameState();
            Assert.That(SfxCues.For(new IGameEvent[] { new GamePaused(GamePhase.Answer, 3) }, state), Is.EqualTo(new[] { SfxCues.Pause }));
            Assert.That(SfxCues.For(new IGameEvent[] { new GameResumed(GamePhase.Answer, 3) }, state), Is.EqualTo(new[] { SfxCues.Resume }));
            Assert.That(SfxCues.For(new IGameEvent[] { new PowerUpApplied("p1", "p2", "Papa", PowerUpTypes.Slime, 5) }, state),
                Is.EqualTo(new[] { PowerUpTypes.Slime }));
        }

        [Test]
        public void TheClockTicksOverTheLastFiveSecondsOfTheLatestOpenDeadline()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstAnswer(2);
            long now = f.Game.NowMs;
            long deadline = seats.Max(p => p.Turn.DeadlineMs);
            Assert.That(SfxCues.SecondsLeft(f.State, deadline - 4200), Is.EqualTo(5));
            Assert.That(SfxCues.SecondsLeft(f.State, deadline - 900), Is.EqualTo(1));
            Assert.That(SfxCues.SecondsLeft(f.State, deadline), Is.EqualTo(0));
            Assert.That(SfxCues.SecondsLeft(f.State, now), Is.GreaterThan(SfxCues.TickFromSeconds), "no ticks at the start");
            f.State.Paused = true;
            Assert.That(SfxCues.SecondsLeft(f.State, deadline - 2000), Is.EqualTo(0), "no ticks while paused");
        }
    }
}
