using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    public sealed class ContentLoadTests
    {
        [Test]
        public void TheRealContentFilesLoad()
        {
            var config = TestContent.Config();
            Assert.That(config.Players.Max, Is.GreaterThanOrEqualTo(config.Players.Min));
            Assert.That(config.Net.TvReconnectSeconds, Is.GreaterThan(0));
            Assert.That(TestContent.Characters(config).Count, Is.GreaterThanOrEqualTo(config.Players.Max));

            var ui = UiStrings.Parse(TestContent.Read(UiStrings.FileName));
            Assert.That(ui.Has("tv.lobby.roomCode"), Is.True);
            Assert.That(ui.Has("tv.error.relayDown"), Is.True);
        }

        [Test]
        public void AMissingConfigKeyNamesTheFile()
        {
            var ex = Assert.Throws<ContentException>(() =>
                ConfigLoader.Parse("{\"configVersion\":1,\"players\":{\"min\":2,\"timerMultiplierOptions\":[1],\"seatHoldSeconds\":300},\"net\":{\"inputGraceMs\":500,\"tvReconnectSeconds\":2}}"));
            Assert.That(ex.Message, Does.Contain("config.json"));
            Assert.That(ex.Message, Does.Contain("max"));
        }

        [Test]
        public void BrokenJsonIsAContentError()
        {
            Assert.Throws<ContentException>(() => ConfigLoader.Parse("{ not json"));
            Assert.Throws<ContentException>(() => UiStrings.Parse("{\"tv.a\": {\"ru\": \"\", \"en\": \"x\"}}"));
        }

        [Test]
        public void UiStringsFillPlaceholders()
        {
            var ui = UiStrings.Parse("{\"tv.results.winner\": {\"ru\": \"Победитель: {name}\", \"en\": \"Winner: {name}\"}}");
            var values = new System.Collections.Generic.Dictionary<string, string> { { "name", "Mama" } };
            Assert.That(ui.Format("tv.results.winner", Languages.English, values), Is.EqualTo("Winner: Mama"));
            Assert.That(ui.Get("tv.results.winner", Languages.Russian), Is.EqualTo("Победитель: {name}"));
            Assert.That(ui.Get("tv.missing", Languages.Russian), Is.EqualTo("tv.missing"));
        }

        [Test]
        public void PausableClockStandsStillWhilePaused()
        {
            long source = 1000;
            var clock = new PausableClock(() => source);
            source += 500;
            Assert.That(clock.NowMs, Is.EqualTo(500));
            clock.Pause();
            source += 10000;
            Assert.That(clock.NowMs, Is.EqualTo(500));
            clock.Resume();
            source += 250;
            Assert.That(clock.NowMs, Is.EqualTo(750));
        }
    }
}
