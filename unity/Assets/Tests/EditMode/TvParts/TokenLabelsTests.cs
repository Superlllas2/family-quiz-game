using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The letters on the players' round tokens (decisions/0017_game_night_ui.md): never two tokens alike.</summary>
    public sealed class TokenLabelsTests
    {
        [Test]
        public void FirstLettersWhenTheyDiffer()
        {
            Assert.That(TokenLabels.For(new[] { "Мама", "папа", "Kid992" }), Is.EqualTo(new[] { "М", "П", "K" }));
        }

        [Test]
        public void TwoLettersWhereTheFirstIsShared()
        {
            Assert.That(TokenLabels.For(new[] { "Мама", "Миша", "Папа" }), Is.EqualTo(new[] { "Ма", "Ми", "П" }));
        }

        [Test]
        public void NumbersWhenTwoLettersAreSharedToo()
        {
            var labels = TokenLabels.For(new[] { "Маша", "Мария", "Папа" });
            Assert.That(labels, Is.EqualTo(new[] { "М1", "М2", "П" }));
        }

        [Test]
        public void NoLettersGivesAQuestionMark()
        {
            Assert.That(TokenLabels.For(new[] { "123", "" }), Is.EqualTo(new[] { "?1", "?2" }));
        }
    }
}
