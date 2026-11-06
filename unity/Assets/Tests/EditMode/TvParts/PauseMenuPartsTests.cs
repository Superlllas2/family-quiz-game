using FamilyQuiz.Core.Content;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The Escape key and the feedback config (decisions/0018, D105, D106, D110).</summary>
    public sealed class PauseMenuPartsTests
    {
        [Test]
        public void EscapePausesFirst()
        {
            Assert.That(EscapeKey.For(paused: false, settingsOpen: false), Is.EqualTo(EscapeKey.Action.Pause));
            Assert.That(EscapeKey.For(paused: false, settingsOpen: true), Is.EqualTo(EscapeKey.Action.Pause),
                "the Tab panel never pauses, so Escape over it opens the pause menu");
        }

        [Test]
        public void EscapeInThePauseMenuClosesTheSettingsThenResumes()
        {
            Assert.That(EscapeKey.For(paused: true, settingsOpen: true), Is.EqualTo(EscapeKey.Action.CloseSettings));
            Assert.That(EscapeKey.For(paused: true, settingsOpen: false), Is.EqualTo(EscapeKey.Action.Resume));
        }

        [TestCase(1)]
        [TestCase(11)]
        public void FeedbackStarsMustBeTwoToTen(int stars)
        {
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["feedback"]["stars"] = stars));
        }

        [Test]
        public void FeedbackItemsMustBeListedOnceEach()
        {
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["feedback"]["items"] = new Newtonsoft.Json.Linq.JArray("fun", "fun")));
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => c["feedback"]["items"] = new Newtonsoft.Json.Linq.JArray()));
            Assert.Throws<ContentException>(() => TestContent.WithConfig(c => ((Newtonsoft.Json.Linq.JObject)c).Remove("feedback")));
        }
    }
}
