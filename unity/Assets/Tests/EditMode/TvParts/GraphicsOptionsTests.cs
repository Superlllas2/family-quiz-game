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

        // ------------------------------------------------------------ brightness (D161)

        [Test]
        public void BrightnessIsZeroByDefault()
        {
            Assert.That(new GraphicsOptions().Brightness, Is.EqualTo(0));
        }

        [Test]
        public void BrightnessStepsAQuarterStopFromMinusOneToPlusTwo()
        {
            var steps = GraphicsOptions.BrightnessSteps;
            Assert.That(steps.Length, Is.EqualTo(13));
            Assert.That(steps[0], Is.EqualTo(-1));
            Assert.That(steps[steps.Length - 1], Is.EqualTo(2));
            for (int i = 1; i < steps.Length; i++) Assert.That(steps[i] - steps[i - 1], Is.EqualTo(0.25).Within(1e-12));
            Assert.That(steps, Does.Contain(0.0));
        }

        [Test]
        public void BrightnessStepsLikeTheOtherRows()
        {
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.BrightnessSteps, 0.0, 1), Is.EqualTo(0.25));
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.BrightnessSteps, 0.0, -1), Is.EqualTo(-0.25));
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.BrightnessSteps, 2.0, 1), Is.EqualTo(-1), "wraps round like every row");
            Assert.That(GraphicsOptions.Cycle(GraphicsOptions.BrightnessSteps, -1.0, -1), Is.EqualTo(2));
        }

        [Test]
        public void BrightnessIsNotPartOfTheQualityPreset()
        {
            var o = new GraphicsOptions { Brightness = 1.5 };
            Assert.That(o.MatchingPreset(), Is.EqualTo("high"));
            o.ApplyPreset("low");
            Assert.That(o.Brightness, Is.EqualTo(1.5));
        }

        [Test]
        public void BrightnessIsKeptOnTheLaptop()
        {
            var path = Path.Combine(_folder, GraphicsOptions.FileName);
            new GraphicsOptions { Brightness = -0.75 }.Save(path);
            Assert.That(GraphicsOptions.Load(path).Brightness, Is.EqualTo(-0.75));
        }

        [TestCase("{\"antiAliasing\":\"fxaa\"}")] // a file from before D161
        [TestCase("{\"brightness\":7}")]
        [TestCase("{\"brightness\":0.3}")]
        [TestCase("{\"brightness\":\"bright\"}")]
        public void AnOldFileOrAnUnknownBrightnessReadsAsZero(string text)
        {
            Directory.CreateDirectory(_folder);
            var path = Path.Combine(_folder, GraphicsOptions.FileName);
            File.WriteAllText(path, text);
            Assert.That(GraphicsOptions.Load(path).Brightness, Is.EqualTo(0));
        }
    }
}
