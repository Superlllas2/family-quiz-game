using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Rules;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>The topic vote doors and the vote result (docs/02_show_flow.md, "Rules inside a round", 1; docs/18_edge_cases.md).</summary>
    public sealed class TopicPickerTests
    {
        private static readonly TopicsConfig Config = new TopicsConfig { Doors = 4, MinUnseenToOffer = 4, MixedTopicId = "mixed" };

        private static TopicDef Topic(string id, bool mixed = false)
        {
            return new TopicDef { Id = id, Title = new LocalizedText { Ru = id, En = id }, Mixed = mixed };
        }

        /// <summary>Topics a..h plus mixed; topic t has counts[t] questions (all unseen unless listed in seen).</summary>
        private static (List<TopicDef> Topics, List<QuestionDef> Bank) World(params (string Id, int Count)[] counts)
        {
            var topics = counts.Select(c => Topic(c.Id)).ToList();
            topics.Add(Topic("mixed", true));
            var bank = new List<QuestionDef>();
            foreach (var (id, count) in counts)
            {
                for (int i = 0; i < count; i++)
                    bank.Add(new QuestionDef { Id = id + "_" + i, Topic = id, Format = QuestionDef.MultipleChoice, Difficulty = "easy" });
            }

            return (topics, bank);
        }

        private static List<string> Ids(List<TopicDef> doors)
        {
            return doors.Select(d => d.Id).ToList();
        }

        [Test]
        public void TheMixedDoorIsAlwaysDoorFour()
        {
            var (topics, bank) = World(("a", 6), ("b", 6), ("c", 6), ("d", 6), ("e", 6));
            for (int seed = 0; seed < 30; seed++)
            {
                var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), new HashSet<string>(), new SeededRandom(seed));
                Assert.That(doors.Count, Is.EqualTo(4));
                Assert.That(doors[3].Id, Is.EqualTo("mixed"));
                Assert.That(doors.Take(3).Any(d => d.Mixed), Is.False);
                Assert.That(Ids(doors).Distinct().Count(), Is.EqualTo(4));
            }
        }

        [Test]
        public void ThreeOfTheQualifyingCandidatesAreChosenBySeed()
        {
            var (topics, bank) = World(("a", 6), ("b", 6), ("c", 6), ("d", 6), ("e", 6), ("f", 2));
            var offered = new HashSet<string>();
            for (int seed = 0; seed < 40; seed++)
            {
                var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), new HashSet<string>(), new SeededRandom(seed));
                foreach (var d in doors.Take(3)) offered.Add(d.Id);
                Assert.That(Ids(doors), Does.Not.Contain("f"), "f has fewer than topics.minUnseenToOffer unseen questions");
                var again = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), new HashSet<string>(), new SeededRandom(seed));
                Assert.That(Ids(again), Is.EqualTo(Ids(doors)), "the same seed gives the same doors");
            }

            Assert.That(offered, Is.EquivalentTo(new[] { "a", "b", "c", "d", "e" }), "every qualifying topic comes up across seeds");
        }

        [Test]
        public void TopicsChosenEarlierInTheGameAreNotCandidates()
        {
            var (topics, bank) = World(("a", 6), ("b", 6), ("c", 6), ("d", 6), ("e", 6));
            var earlier = new HashSet<string> { "a", "b" };
            for (int seed = 0; seed < 20; seed++)
            {
                var doors = TopicPicker.Doors(topics, bank, Config, earlier, new HashSet<string>(), new SeededRandom(seed));
                Assert.That(Ids(doors).Take(3), Is.EquivalentTo(new[] { "c", "d", "e" }));
            }
        }

        [Test]
        public void UnseenMeansNotInTheSeenListOfAnyFormatOrDifficulty()
        {
            var (topics, bank) = World(("a", 6), ("b", 6), ("c", 6), ("d", 6));
            bank.Add(new QuestionDef { Id = "a_cv", Topic = "a", Format = QuestionDef.ClosestValue, Difficulty = "hard" });
            // a: 7 questions, 4 seen -> 3 unseen (not offered first); b, c, d qualify.
            var seen = new HashSet<string> { "a_0", "a_1", "a_2", "a_3" };
            Assert.That(TopicPicker.UnseenCount("a", bank, seen), Is.EqualTo(3));
            var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), seen, new SeededRandom(3));
            Assert.That(Ids(doors).Take(3), Is.EquivalentTo(new[] { "b", "c", "d" }));
        }

        [Test]
        public void FewerThanThreeQualifyFillsWithTheOtherCandidatesMostUnseenFirst()
        {
            // Edge case: fewer than 3 unused topics have 4 unseen questions.
            var (topics, bank) = World(("a", 6), ("b", 3), ("c", 1), ("d", 2), ("e", 0));
            for (int seed = 0; seed < 20; seed++)
            {
                var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), new HashSet<string>(), new SeededRandom(seed));
                Assert.That(Ids(doors), Is.EqualTo(new[] { "a", "b", "d", "mixed" }));
            }
        }

        [Test]
        public void EqualUnseenCountsAreOrderedBySeed()
        {
            var (topics, bank) = World(("a", 6), ("b", 2), ("c", 2), ("d", 2));
            var seconds = new HashSet<string>();
            for (int seed = 0; seed < 30; seed++)
            {
                var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string>(), new HashSet<string>(), new SeededRandom(seed));
                Assert.That(doors[0].Id, Is.EqualTo("a"));
                Assert.That(new[] { doors[1].Id, doors[2].Id }, Is.SubsetOf(new[] { "b", "c", "d" }));
                seconds.Add(doors[1].Id);
            }

            Assert.That(seconds.Count, Is.GreaterThan(1), "ties are not always resolved the same way");
        }

        [Test]
        public void ThenTopicsChosenEarlierInThisGameMostUnseenFirst()
        {
            var (topics, bank) = World(("a", 6), ("b", 5), ("c", 3), ("d", 1), ("e", 6));
            // a, b, c were chosen earlier; only d and e are candidates (e qualifies, d does not).
            var earlier = new HashSet<string> { "a", "b", "c" };
            var doors = TopicPicker.Doors(topics, bank, Config, earlier, new HashSet<string>(), new SeededRandom(1));
            Assert.That(Ids(doors), Is.EqualTo(new[] { "e", "d", "a", "mixed" }));
        }

        [Test]
        public void TheMixedTopicChosenEarlierStaysDoorFour()
        {
            var (topics, bank) = World(("a", 6), ("b", 6), ("c", 6), ("d", 6));
            var doors = TopicPicker.Doors(topics, bank, Config, new HashSet<string> { "mixed" }, new HashSet<string>(), new SeededRandom(1));
            Assert.That(doors[3].Id, Is.EqualTo("mixed"));
        }

        // ---------------------------------------------------------------- the vote

        private static int Resolve(Dictionary<string, int> votes, Dictionary<string, int> ranks, int seed)
        {
            return TopicPicker.ResolveVote(votes, 4, id => ranks[id], new SeededRandom(seed));
        }

        [Test]
        public void TheMajorityWins()
        {
            var votes = new Dictionary<string, int> { { "p1", 2 }, { "p2", 2 }, { "p3", 0 } };
            var ranks = new Dictionary<string, int> { { "p1", 3 }, { "p2", 2 }, { "p3", 1 } };
            Assert.That(Resolve(votes, ranks, 1), Is.EqualTo(2));
        }

        [Test]
        public void ATieGoesToTheLowestRankedVoterOfTheTiedDoors()
        {
            // p1 leads (rank 1) on door 0, p3 is last (rank 3) on door 1: door 1 wins.
            var votes = new Dictionary<string, int> { { "p1", 0 }, { "p3", 1 } };
            var ranks = new Dictionary<string, int> { { "p1", 1 }, { "p3", 3 } };
            for (int seed = 0; seed < 10; seed++) Assert.That(Resolve(votes, ranks, seed), Is.EqualTo(1));
        }

        [Test]
        public void VotersForOtherDoorsDoNotDecideATie()
        {
            // Doors 0 and 1 tie with 2 votes each; p5 (last place) voted for door 3, which is not tied.
            var votes = new Dictionary<string, int> { { "p1", 0 }, { "p2", 0 }, { "p3", 1 }, { "p4", 1 }, { "p5", 3 } };
            var ranks = new Dictionary<string, int> { { "p1", 1 }, { "p2", 2 }, { "p3", 1 }, { "p4", 3 }, { "p5", 5 } };
            for (int seed = 0; seed < 10; seed++) Assert.That(Resolve(votes, ranks, seed), Is.EqualTo(1), "p4 is the lowest ranked of the tied voters");
        }

        [Test]
        public void SeveralLowestRankedTiedVotersArePickedBySeed()
        {
            var votes = new Dictionary<string, int> { { "p1", 0 }, { "p2", 1 }, { "p3", 2 } };
            var ranks = new Dictionary<string, int> { { "p1", 1 }, { "p2", 1 }, { "p3", 1 } }; // the start of a game: everyone rank 1
            var doors = new HashSet<int>();
            for (int seed = 0; seed < 40; seed++)
            {
                int door = Resolve(votes, ranks, seed);
                Assert.That(door, Is.InRange(0, 2));
                Assert.That(Resolve(votes, ranks, seed), Is.EqualTo(door), "the same seed decides the same way");
                doors.Add(door);
            }

            Assert.That(doors.Count, Is.EqualTo(3));
        }

        [Test]
        public void NobodyVotedPicksAnyOfTheFourDoorsBySeed()
        {
            var doors = new HashSet<int>();
            for (int seed = 0; seed < 60; seed++) doors.Add(Resolve(new Dictionary<string, int>(), new Dictionary<string, int>(), seed));
            Assert.That(doors, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
        }
    }
}
