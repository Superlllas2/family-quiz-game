using FamilyQuiz.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>
    /// The TV's zones (decisions/0017_game_night_ui.md): the hosts' bubbles, the status corner or title and each
    /// view's pieces never meet, everything stays on screen, and the content stays under the hosts' and status zones.
    /// </summary>
    public sealed class TvLayoutTests
    {
        private static readonly Rect Screen = new Rect(0, 0, TvLayout.Width, TvLayout.Height);

        [Test]
        public void NoTwoPiecesOfAPhaseOverlap()
        {
            var sets = TvLayout.PhaseSets();
            for (int s = 0; s < sets.Length; s++)
            {
                for (int i = 0; i < sets[s].Length; i++)
                for (int j = i + 1; j < sets[s].Length; j++)
                    Assert.That(sets[s][i].Overlaps(sets[s][j]), Is.False, "set " + s + ": " + sets[s][i] + " meets " + sets[s][j]);
            }
        }

        [Test]
        public void EveryZoneIsOnScreen()
        {
            foreach (var set in TvLayout.PhaseSets())
            foreach (var r in set)
                Assert.That(Screen.Contains(r.min) && Screen.Contains(r.max), Is.True, r + " leaves the screen");
        }

        [Test]
        public void ContentStaysUnderTheTopZones()
        {
            foreach (var r in new[] { TvLayout.ShowCard, TvLayout.AnswerCard, TvLayout.Tiles, TvLayout.Ruler, TvLayout.RulerReveal,
                         TvLayout.VoteTitle, TvLayout.VoteCards, TvLayout.ScoreRow, TvLayout.Ticket, TvLayout.Content })
                Assert.That(r.yMax, Is.LessThanOrEqualTo(TvLayout.ContentTop), r + " reaches into the top zones");
            Assert.That(TvLayout.Hosts.yMin, Is.GreaterThan(TvLayout.ContentTop));
            Assert.That(TvLayout.Status.yMin, Is.GreaterThan(TvLayout.ContentTop));
            Assert.That(TvLayout.Title.yMin, Is.GreaterThan(TvLayout.ContentTop));
        }

        [Test]
        public void TheAnswerCardAndTilesStayUnderTheGuests()
        {
            // The guests sit between about 33 % and 57 % of the Wide frame's height.
            Assert.That(TvLayout.AnswerCard.yMax / TvLayout.Height, Is.LessThan(0.38f));
            Assert.That(TvLayout.Tiles.yMax, Is.LessThan(TvLayout.AnswerCard.yMin));
        }
    }
}
