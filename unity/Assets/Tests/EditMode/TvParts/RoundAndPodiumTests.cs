using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>The round winners on RoundEnd and the top places on Results.</summary>
    public sealed class RoundAndPodiumTests
    {
        private static PlayerState Player(GameState state, string id)
        {
            var p = new PlayerState(id, id, "en", 0, "t" + id, 1);
            state.MutablePlayers.Add(p);
            return p;
        }

        private static GameState WithRound(params (string id, int points)[] players)
        {
            var s = new GameState();
            s.Round = new RoundState(1, 1, new List<TopicDef>());
            foreach (var (id, points) in players)
            {
                Player(s, id);
                if (points >= 0) s.Round.MutablePoints[id] = points;
            }

            return s;
        }

        [Test]
        public void TheRoundWinnerHasTheMostPointsThisRound()
        {
            var s = WithRound(("a", 500), ("b", 1450), ("c", 0));
            s.FindById("a").Score = 5000; // the total does not count, only this round
            CollectionAssert.AreEqual(new[] { "b" }, Standings.RoundWinners(s).Select(p => p.PlayerId).ToArray());
            Assert.AreEqual(1450, Standings.RoundPoints(s, s.FindById("b")));
        }

        [Test]
        public void TiedRoundWinnersAreAllShownInJoinOrder()
        {
            var s = WithRound(("a", 900), ("b", 300), ("c", 900));
            CollectionAssert.AreEqual(new[] { "a", "c" }, Standings.RoundWinners(s).Select(p => p.PlayerId).ToArray());
        }

        [Test]
        public void NobodyWinsARoundWithoutPoints()
        {
            var s = WithRound(("a", 0), ("b", -1));
            Assert.IsEmpty(Standings.RoundWinners(s));
            Assert.AreEqual(0, Standings.RoundPoints(s, s.FindById("b")));
        }

        [Test]
        public void NoRoundNoPoints()
        {
            var s = new GameState();
            var a = Player(s, "a");
            Assert.AreEqual(0, Standings.RoundPoints(s, a));
            Assert.IsEmpty(Standings.RoundWinners(s));
        }

        [Test]
        public void TheTopComesFromTheFinalOrder()
        {
            var s = new GameState();
            foreach (var id in new[] { "a", "b", "c", "d", "e" }) Player(s, id);
            s.MutablePlacement.AddRange(new[] { "d", "a", "e", "b", "c" });
            CollectionAssert.AreEqual(new[] { "d", "a", "e" }, Standings.Top(s, 3).Select(p => p.PlayerId).ToArray());
        }

        [Test]
        public void TheTopIsShorterWithFewerPlayersAndSkipsUnknownIds()
        {
            var s = new GameState();
            Player(s, "a");
            Player(s, "b");
            s.MutablePlacement.AddRange(new[] { "b", "gone", "a" });
            CollectionAssert.AreEqual(new[] { "b", "a" }, Standings.Top(s, 3).Select(p => p.PlayerId).ToArray());
            Assert.IsEmpty(Standings.Top(new GameState(), 3));
        }
    }
}
