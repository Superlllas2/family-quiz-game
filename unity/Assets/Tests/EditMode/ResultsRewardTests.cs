using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Results rewards: one winner with a fact (D87), a funny tag for every other player (D86), and the stats the tags
    /// come from.
    /// </summary>
    public sealed class ResultsRewardTests
    {
        private static List<PlayerState> Players(int count)
        {
            var list = new List<PlayerState>();
            for (int i = 1; i <= count; i++) list.Add(new PlayerState("p" + i, "P" + i, "en", i, "t" + i, 1));
            return list;
        }

        // ---------------------------------------------------------------- tags

        [Test]
        public void EveryPlayerButTheWinnerGetsOneTagAndNoTagTwice()
        {
            for (int seed = 1; seed <= 50; seed++)
            {
                var players = Players(8);
                var tags = Tags.Assign(players, "p1", new SeededRandom(seed));
                Assert.That(tags.ContainsKey("p1"), Is.False, "the winner gets the fact, not a tag");
                Assert.That(tags.Keys, Is.EquivalentTo(players.Skip(1).Select(p => p.PlayerId)));
                Assert.That(tags.Values.Distinct().Count(), Is.EqualTo(7), "seven non-winners, seven different tags");
                Assert.That(tags.Values.All(t => Tags.All.Contains(t)), Is.True);
            }
        }

        [Test]
        public void WithNoStatsEveryoneGetsAFallbackTag()
        {
            var players = Players(3);
            var tags = Tags.Assign(players, "p1", new SeededRandom(5));
            Assert.That(tags.Values.All(t => Tags.FallbackTags.Contains(t)), Is.True);
        }

        [Test]
        public void AStatTagGoesToThePlayerWhoReallyIsTheMost()
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                var players = Players(3);
                players[1].Stats.Wrong = 5;
                players[2].Stats.Wrong = 2;
                var tags = Tags.Assign(players, "p1", new SeededRandom(seed));
                Assert.That(tags["p2"], Is.EqualTo(Tags.MostMisses));
                Assert.That(tags["p3"], Is.Not.EqualTo(Tags.MostMisses));
            }
        }

        [Test]
        public void AStatTagIsNeverGivenWhenTheWinnerIsTheMost()
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                var players = Players(3);
                players[0].Stats.Wrong = 9; // the winner
                players[1].Stats.Wrong = 5;
                var tags = Tags.Assign(players, "p1", new SeededRandom(seed));
                Assert.That(tags.Values, Has.No.Member(Tags.MostMisses), "P2 is not the master of misses: the winner is");
            }
        }

        [Test]
        public void TiedPlayersShareNothingTheSeedPicksOne()
        {
            var picked = new HashSet<string>();
            for (int seed = 1; seed <= 40; seed++)
            {
                var players = Players(3);
                players[1].Stats.Unanswered = 2;
                players[2].Stats.Unanswered = 2;
                var tags = Tags.Assign(players, "p1", new SeededRandom(seed));
                var dreamer = tags.Where(t => t.Value == Tags.Daydreamer).Select(t => t.Key).ToList();
                Assert.That(dreamer.Count, Is.EqualTo(1));
                picked.Add(dreamer[0]);
            }

            Assert.That(picked, Is.EquivalentTo(new[] { "p2", "p3" }));
        }

        [Test]
        public void EachStatTagReadsItsOwnStat()
        {
            var players = Players(2);
            var p = players[1];
            Assert.That(Tags.Fit(Tags.Slowest, p, players), Is.EqualTo(0), "no answers, no average");
            p.Stats.Answered = 2;
            p.Stats.WindowShareSum = 1.5;
            Assert.That(Tags.Fit(Tags.Slowest, p, players), Is.EqualTo(0.75));
            p.Stats.WorstMiss = 0.9;
            Assert.That(Tags.Fit(Tags.WildGuess, p, players), Is.EqualTo(0), "inside the tolerance is no wild guess");
            p.Stats.WorstMiss = 3;
            Assert.That(Tags.Fit(Tags.WildGuess, p, players), Is.EqualTo(3));
            players[0].Stats.RaceSteps = 7;
            p.Stats.RaceSteps = 2;
            Assert.That(Tags.Fit(Tags.RaceTortoise, p, players), Is.EqualTo(5));
            Assert.That(Tags.Fit(Tags.RaceTortoise, players[0], players), Is.EqualTo(0), "the furthest is no tortoise");
        }

        // ---------------------------------------------------------------- facts

        private static List<FactDef> Facts(params string[] ids)
        {
            return ids.Select(id => new FactDef { Id = id, Text = new LocalizedText { Ru = id, En = id }, Source = "s" }).ToList();
        }

        [Test]
        public void TheFactIsOneNotShownBefore()
        {
            var facts = Facts("a", "b", "c");
            for (int seed = 1; seed <= 20; seed++)
                Assert.That(FactPicker.Pick(facts, new[] { "a", "c" }, new SeededRandom(seed)).Id, Is.EqualTo("b"));
        }

        [Test]
        public void OnceEveryFactWasShownAnyFactWillDo()
        {
            var facts = Facts("a", "b");
            Assert.That(FactPicker.Pick(facts, new[] { "a", "b" }, new SeededRandom(3)), Is.Not.Null);
            Assert.That(FactPicker.Pick(new List<FactDef>(), new string[0], new SeededRandom(3)), Is.Null);
        }

        [Test]
        public void TheFactsFileLoadsAndChecksItsEntries()
        {
            var facts = FactLoader.Parse(TestContent.Read(FactLoader.FileName));
            Assert.That(facts.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(TestContent.Content().Facts.Count, Is.EqualTo(facts.Count));

            var twice = "[{\"id\":\"x\",\"text\":{\"ru\":\"a\",\"en\":\"a\"},\"source\":\"s\"},"
                        + "{\"id\":\"x\",\"text\":{\"ru\":\"b\",\"en\":\"b\"},\"source\":\"s\"}]";
            var ex = Assert.Throws<ContentException>(() => FactLoader.Parse(twice));
            Assert.That(ex.Message, Does.Contain("used twice"));

            var longText = new string('a', ContentLimits.FactTextMax + 1);
            var tooLong = "[{\"id\":\"x\",\"text\":{\"ru\":\"" + longText + "\",\"en\":\"a\"},\"source\":\"s\"}]";
            Assert.Throws<ContentException>(() => FactLoader.Parse(tooLong));
            Assert.Throws<ContentException>(() =>
                FactLoader.Parse("[{\"id\":\"x\",\"text\":{\"ru\":\"a\",\"en\":\"a\"},\"source\":\"\"}]"));
        }

        // ---------------------------------------------------------------- in a game

        [Test]
        public void ResultsGiveTheWinnerAFactAndEveryoneElseATag()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(4);
            f.ReadyAll();
            f.SkipTo(GamePhase.Results);
            var state = f.State;
            Assert.That(state.WinnerId, Is.Not.Null);
            Assert.That(state.WinnerFact, Is.Not.Null);
            Assert.That(state.SeenFactIds, Is.EquivalentTo(new[] { state.WinnerFact.Id }));
            Assert.That(state.TagsByPlayer.Keys,
                Is.EquivalentTo(seats.Select(s => s.PlayerId).Where(id => id != state.WinnerId)));
            Assert.That(state.TagsByPlayer.Values.Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void TheNextWinnerGetsADifferentFactAndAResetClearsThem()
        {
            var f = new LobbyFixture(seenFacts: new[] { "f001" });
            f.SeatPlayers(2);
            f.ReadyAll();
            f.SkipTo(GamePhase.Results);
            Assert.That(f.State.WinnerFact.Id, Is.Not.EqualTo("f001"));
            Assert.That(f.State.SeenFactIds.Count, Is.EqualTo(2));
            f.Send(new ResetSeen());
            Assert.That(f.State.SeenFactIds, Is.Empty);
        }

        [Test]
        public void StatsCountWrongHastyUnansweredAndLostVotes()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(3);
            f.Vote("c1", 0);
            f.Vote("c2", 0);
            f.Vote("c3", 1);
            Assert.That(seats[2].Stats.LostVotes, Is.EqualTo(1));
            Assert.That(seats[0].Stats.LostVotes, Is.EqualTo(0));

            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 3000);
            f.Choice("c2", f.Wrong, f.Config.Scoring.FastAnswerMs - 1);
            f.SkipTo(GamePhase.Reveal);
            Assert.That(seats[0].Stats.Answered, Is.EqualTo(1));
            Assert.That(seats[0].Stats.Wrong, Is.EqualTo(0));
            Assert.That(seats[0].Stats.WindowShareSum, Is.EqualTo(3000.0 / f.WindowMs).Within(1e-9));
            Assert.That(seats[1].Stats.Wrong, Is.EqualTo(1));
            Assert.That(seats[1].Stats.HastyWrong, Is.EqualTo(1));
            Assert.That(seats[2].Stats.Unanswered, Is.EqualTo(1));
            Assert.That(seats[2].Stats.LastCalls, Is.EqualTo(1), "P3 was the last one left");
        }

        [Test]
        public void ANewGameStartsWithFreshStats()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.ReadyAll();
            f.RunToLobby();
            Assert.That(seats[0].Stats.Answered + seats[0].Stats.Unanswered, Is.GreaterThan(0));
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(seats[0].Stats.Answered + seats[0].Stats.Unanswered, Is.EqualTo(0));
        }
    }
}
