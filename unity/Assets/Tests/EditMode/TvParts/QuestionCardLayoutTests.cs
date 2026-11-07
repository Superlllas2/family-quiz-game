using FamilyQuiz.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>
    /// The question card's topic ribbon never covers the question (Nov 6, 2026 playtest, notes.md 2: a long topic
    /// label covered a few letters of the question).
    /// </summary>
    public sealed class QuestionCardLayoutTests
    {
        [Test]
        public void TheQuestionStartsBelowEveryCornerOfTheTiltedRibbon()
        {
            float rad = QuestionCardLayout.RibbonTilt * Mathf.Deg2Rad;
            float bottom = -QuestionCardLayout.RibbonHeight / 2f - QuestionCardLayout.RibbonEdge;
            float top = -QuestionCardLayout.QuestionTopInset();
            // Every point along the ribbon's bottom edge, its ink edge included.
            for (float x = 0f; x <= QuestionCardLayout.RibbonWidth; x += 10f)
            {
                float y = QuestionCardLayout.RibbonY + x * Mathf.Sin(rad) + bottom * Mathf.Cos(rad);
                Assert.That(y, Is.GreaterThanOrEqualTo(top + QuestionCardLayout.Gap - 0.01f), "x " + x);
            }
        }

        [Test]
        public void TheRibbonsRightEndDipsBelowItsLeftEnd()
        {
            // A tilt to the right (clockwise) makes the right end the lowest point: the case the inset covers.
            Assert.That(QuestionCardLayout.RibbonTilt, Is.LessThan(0f));
            float leftBottom = QuestionCardLayout.RibbonY - QuestionCardLayout.RibbonHeight / 2f - QuestionCardLayout.RibbonEdge;
            Assert.That(QuestionCardLayout.RibbonLowestY(), Is.LessThan(leftBottom));
        }

        [Test]
        public void TheInsetLeavesTheSmallAnswerCardRoomForTwoLines()
        {
            // The Answer card is 146 units tall with an 18 unit bottom margin; two lines of 30 unit type need about 72.
            float room = TvLayout.AnswerCard.height - 18f - QuestionCardLayout.QuestionTopInset();
            Assert.That(room, Is.GreaterThanOrEqualTo(72f));
        }
    }
}
