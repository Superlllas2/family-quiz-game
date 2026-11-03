using System;
using System.Linq;
using FamilyQuiz.Core.Content;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The Day 4 content (docs/15_schemas.md): the powerUps, hosts and poll config sections, host_lines.json and
    /// poll.json load from the real files, and a broken one names the file, the item and the field.
    /// </summary>
    public sealed class Day4ContentTests
    {
        private static ContentException ConfigError(Action<JObject> change)
        {
            var root = JObject.Parse(TestContent.Read(ConfigLoader.FileName));
            change(root);
            return Assert.Throws<ContentException>(() => ConfigLoader.Parse(root.ToString()));
        }

        [Test]
        public void TheRealConfigHasPowerUpsHostsAndPoll()
        {
            var c = TestContent.Config();
            Assert.That(c.PowerUps.Types, Is.EquivalentTo(PowerUpTypes.All));
            Assert.That(c.PowerUps.Targeting, Is.EqualTo(TargetingModes.Free));
            Assert.That(c.PowerUps.DrawChanceLeader, Is.InRange(0, 1));
            Assert.That(c.PowerUps.Freeze.Taps, Is.GreaterThan(0));
            Assert.That(c.Hosts.BubbleSeconds, Is.GreaterThan(0));
            Assert.That(c.Poll.MaxSeconds, Is.GreaterThan(0));
        }

        [Test]
        public void PowerUpConfigIsChecked()
        {
            var ex = ConfigError(c =>
            {
                c["powerUps"]["types"] = new JArray("freeze", "banana");
                c["powerUps"]["targeting"] = "sideways";
                c["powerUps"]["drawChanceLeader"] = 1.5;
                c["powerUps"]["freeze"]["taps"] = 0;
            });
            Assert.That(ex.File, Is.EqualTo(ConfigLoader.FileName));
            Assert.That(ex.Message, Does.Contain("powerUps.types: \"banana\""));
            Assert.That(ex.Message, Does.Contain("powerUps.targeting: must be free or aimUp"));
            Assert.That(ex.Message, Does.Contain("powerUps.drawChanceLeader: must be 0 to 1"));
            Assert.That(ex.Message, Does.Contain("powerUps.freeze.taps: must be 1 or more"));
        }

        [Test]
        public void HostAndPollConfigIsChecked()
        {
            var ex = ConfigError(c =>
            {
                c["hosts"]["bubbleSeconds"] = 0;
                c["poll"]["maxSeconds"] = -1;
            });
            Assert.That(ex.Message, Does.Contain("hosts.bubbleSeconds: must be more than 0"));
            Assert.That(ex.Message, Does.Contain("poll.maxSeconds: must be more than 0"));
        }

        // ---------------------------------------------------------------- host_lines.json

        [Test]
        public void TheRealHostLinesLoad()
        {
            var lines = HostLineLoader.Parse(TestContent.Read(HostLineLoader.FileName));
            Assert.That(lines.Count, Is.GreaterThan(0));
            foreach (var host in HostIds.All)
                Assert.That(lines.Any(l => l.Host == host && l.Trigger == HostTriggers.GameStart), Is.True, host);
            Assert.That(lines.All(l => l.Weight > 0), Is.True);
        }

        private static string Line(string id, string host, string trigger, string ru = "Привет", string en = "Hi", double? weight = null)
        {
            var o = new JObject
            {
                ["id"] = id, ["host"] = host, ["trigger"] = trigger,
                ["text"] = new JObject { ["ru"] = ru, ["en"] = en }
            };
            if (weight.HasValue) o["weight"] = weight.Value;
            return o.ToString();
        }

        private static string Lines(params string[] items)
        {
            return "[" + string.Join(",", items) + "]";
        }

        private static readonly string KStart = Line("k_gs", "konstantin", "gameStart");
        private static readonly string IStart = Line("i_gs", "ilja", "gameStart");

        [Test]
        public void TheWeightDefaultsToOne()
        {
            var lines = HostLineLoader.Parse(Lines(KStart, IStart));
            Assert.That(lines[0].Weight, Is.EqualTo(1));
        }

        [Test]
        public void BrokenHostLinesAreNamedByIdAndField()
        {
            var ex = Assert.Throws<ContentException>(() => HostLineLoader.Parse(Lines(KStart, IStart,
                Line("x1", "bob", "gameStart"),
                Line("x2", "ilja", "dance"),
                Line("x3", "ilja", "roundEnd", new string('я', 91)),
                Line("x4", "ilja", "roundEnd", weight: 0),
                Line("k_gs", "konstantin", "roundEnd"))));
            Assert.That(ex.File, Is.EqualTo(HostLineLoader.FileName));
            Assert.That(ex.Errors, Has.Some.EqualTo("host_lines.json x1: host must be konstantin or ilja"));
            Assert.That(ex.Errors, Has.Some.Contains("host_lines.json x2: trigger \"dance\""));
            Assert.That(ex.Errors, Has.Some.EqualTo("host_lines.json x3: text must be at most 90 characters per language"));
            Assert.That(ex.Errors, Has.Some.EqualTo("host_lines.json x4: weight must be more than 0"));
            Assert.That(ex.Errors, Has.Some.EqualTo("host_lines.json k_gs: id is used twice"));
        }

        [Test]
        public void EachHostNeedsAGameStartLine()
        {
            var ex = Assert.Throws<ContentException>(() => HostLineLoader.Parse(Lines(KStart)));
            Assert.That(ex.Errors, Has.Some.EqualTo("host_lines.json ilja: needs a gameStart line (the Intro plays one per host)"));
        }

        // ---------------------------------------------------------------- poll.json

        [Test]
        public void TheRealPollLoads()
        {
            var poll = PollLoader.Parse(TestContent.Read(PollLoader.FileName));
            Assert.That(poll.Select(q => q.Id), Is.EqualTo(new[] { "fun", "frustrating", "waiting", "time", "tvMissed", "unfair", "again" }));
            Assert.That(poll.Single(q => q.Id == "fun").Kind, Is.EqualTo(PollQuestionDef.KindText));
            Assert.That(poll.Single(q => q.Id == "waiting").Options.Count, Is.EqualTo(4));
        }

        [Test]
        public void BrokenPollQuestionsAreNamed()
        {
            const string json = @"[
                { ""id"": ""a"", ""kind"": ""slider"", ""text"": { ""ru"": ""А?"", ""en"": ""A?"" } },
                { ""id"": ""b"", ""kind"": ""choice"", ""text"": { ""ru"": ""Б?"", ""en"": ""B?"" }, ""options"": [ { ""ru"": ""Да"", ""en"": ""Yes"" } ] },
                { ""id"": ""c"", ""kind"": ""text"", ""text"": { ""ru"": """", ""en"": ""C?"" } },
                { ""id"": ""a"", ""kind"": ""text"", ""text"": { ""ru"": ""А?"", ""en"": ""A?"" } }
            ]";
            var ex = Assert.Throws<ContentException>(() => PollLoader.Parse(json));
            Assert.That(ex.File, Is.EqualTo(PollLoader.FileName));
            Assert.That(ex.Errors, Has.Some.EqualTo("poll.json a: kind must be choice or text"));
            Assert.That(ex.Errors, Has.Some.EqualTo("poll.json b: options must list 2 or more choices"));
            Assert.That(ex.Errors, Has.Some.EqualTo("poll.json c: text must have non empty ru and en"));
            Assert.That(ex.Errors, Has.Some.EqualTo("poll.json a: id is used twice"));
        }

        [Test]
        public void ABrokenHostLinesFileStopsTheWholeLoader()
        {
            var ex = Assert.Throws<ContentException>(() => new ContentLoader(name =>
                name == HostLineLoader.FileName ? "{" : TestContent.Read(name)).Load());
            Assert.That(ex.File, Is.EqualTo(HostLineLoader.FileName));
        }
    }
}
