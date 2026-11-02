using System.Linq;
using FamilyQuiz.Core.State;
using FamilyQuiz.DevTools;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>Plain C# parts of the TV presentation and the debug overlay.</summary>
    public sealed class StandingsTests
    {
        private static PlayerState Player(GameState state, string id, int score, int points)
        {
            var p = new PlayerState(id, id, "en", 0, "t" + id, 1);
            p.Score = score;
            p.Turn = new PlayerTurn { Points = points, Revealed = true };
            state.MutablePlayers.Add(p);
            return p;
        }

        [Test]
        public void OrdersByScoreAndKeepsJoinOrderOnTies()
        {
            var s = new GameState();
            Player(s, "a", 400, 0);
            Player(s, "b", 900, 900);
            Player(s, "c", 900, 500);
            Player(s, "d", 400, 0);
            CollectionAssert.AreEqual(new[] { "b", "c", "a", "d" }, Standings.ByScore(s).Select(p => p.PlayerId).ToArray());
            // Before: a 400, b 0, c 400, d 400
            CollectionAssert.AreEqual(new[] { "a", "c", "d", "b" }, Standings.ByScoreBefore(s).Select(p => p.PlayerId).ToArray());
        }

        [Test]
        public void PointsCountOnlyOnceRevealed()
        {
            var s = new GameState();
            var p = Player(s, "a", 300, 200);
            p.Turn.Revealed = false;
            Assert.AreEqual(0, Standings.PointsNow(p));
            Assert.AreEqual(300, Standings.ScoreBefore(p));
        }
    }
}
