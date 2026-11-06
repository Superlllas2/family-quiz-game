using System.IO;
using FamilyQuiz.Presentation.Graphics;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The graphics settings model (decisions/0021, D122 to D124): presets, cycling and the file on the laptop.</summary>
    public sealed class GraphicsOptionsTests
    {
        private string _folder;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "fq_gfx_" + System.Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        }

        [Test]
        public void TheDefaultIsTheHighPresetTheStudioWasLitFor()
        {
            var o = new GraphicsOptions();
            Assert.That(o.MatchingPreset(), Is.EqualTo("high"));
            Assert.That(o.AntiAliasing, Is.EqualTo("msaa"));
            Assert.That(o.AmbientOcclusion, Is.True);
            Assert.That(o.RenderScale, Is.EqualTo(100));
            Assert.That(o.DisplayMode, Is.EqualTo("borderless"));
            Assert.That(o.VSync, Is.True);
        }

        [TestCase("low")]
        [TestCase("medium")]
        [TestCase("high")]
        [TestCase("ultra")]
        public void EveryPresetUsesListedValuesAndIsRecognised(string preset)
        {
            var o = new GraphicsOptions();
            o.ApplyPreset(preset);
            Assert.That(o.MatchingPreset(), Is.EqualTo(preset));
            Assert.That(GraphicsOptions.RenderScales, Does.Contain(o.RenderScale));
            Assert.That(GraphicsOptions.AnisoLevels, Does.Contain(o.Anisotropic));
            Assert.That(GraphicsOptions.ShadowLevels, Does.Contain(o.Shadows));
            Assert.That(GraphicsOptions.TextureLevels, Does.Contain(o.Textures));
            Assert.That(GraphicsOptions.AntiAliasingModes, Does.Contain(o.AntiAliasing));
        }

        [Test]
        public void ChangingADetailMakesTheQualityCustom()
        {
            var o = new GraphicsOptions();
            o.Bloom = false;
            Assert.That(o.MatchingPreset(), Is.EqualTo(GraphicsOptions.Custom));
        }

        [Test]
        public void APresetKeepsTheWindowAndTheFrameRate()
        {
            var o = new GraphicsOptions { DisplayMode = "windowed", Width = 1280, Height = 720, VSync = false, FrameLimit = 144 };
            o.ApplyPreset("low");
            Assert.That(o.DisplayMode, Is.EqualTo("windowed"));
            Assert.That(o.Width, Is.EqualTo(1280));
            Assert.That(o.VSync, Is.False);
            Assert.That(o.FrameLimit, Is.EqualTo(144));
        }

        [Test]
        public void CyclingWrapsRoundBothWays()
        {
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.FrameLimits, 0, 1), Is.EqualTo(30));
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.FrameLimits, 30, -1), Is.EqualTo(0));
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.AntiAliasingModes, "smaa", 1), Is.EqualTo("taa"));
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.AntiAliasingModes, "nope", 1), Is.EqualTo("off"));
        }

        [Test]
        public void TheOptionsAreKeptOnTheLaptop()
        {
            var path = Path.Combine(_folder, GraphicsOptions.FileName);
            var o = new GraphicsOptions { DisplayMode = "fullscreen", Width = 2560, Height = 1440, VSync = false, FrameLimit = 120 };
            o.ApplyPreset("low");
            o.Save(path);
            var back = GraphicsOptions.Load(path);
            Assert.That(back.DisplayMode, Is.EqualTo("fullscreen"));
            Assert.That(back.Width, Is.EqualTo(2560));
            Assert.That(back.FrameLimit, Is.EqualTo(120));
            Assert.That(back.Preset, Is.EqualTo("low"));
            Assert.That(back.AntiAliasing, Is.EqualTo("fxaa"));
        }

        [TestCase("not json")]
        [TestCase("{\"antiAliasing\":\"rainbow\",\"renderScale\":33}")]
        public void ABrokenFileOrUnknownValuesReadAsTheDefault(string text)
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, GraphicsOptions.FileName);
            File.WriteAllText(path, text);
            var o = GraphicsOptions.Load(path);
            Assert.That(o.AntiAliasing, Is.EqualTo("msaa"));
            Assert.That(o.RenderScale, Is.EqualTo(100));
        }
    }
}
