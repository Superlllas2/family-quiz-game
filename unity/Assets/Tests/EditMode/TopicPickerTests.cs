using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The topic vote doors and the vote result (docs/02_show_flow.md, "Rules inside a round", 1; D153, D154;
    /// docs/18_edge_cases.md).
    /// </summary>
    public sealed class TopicPickerTests
    {
        private static readonly TopicsConfig Config = new TopicsConfig { Doors = 4, MinUnseenToOffer = 4, MixedTopicId = "mixed" };

        private sealed class T
        {
            public string Id;
            public string Shelf;
            public string Category;
            public string Lean = TopicShelves.LeanAll;
            public bool Russian;
            public int Questions = 6;
        }

        private static T S(string id, string category = null) => new T { Id = id, Shelf = TopicShelves.Screen, Category = category ?? id };
        private static T K(string id, string category = null) => new T { Id = id, Shelf = TopicShelves.Knowledge, Category = category ?? id };
        private static T L(string id, string category = null) => new T { Id = id, Shelf = TopicShelves.Life, Category = category ?? id };

        /// <summary>The topics plus mixed, and topic t with t.Questions questions (all unseen unless listed in seen).</summary>
        private static (List<TopicDef> Topics, List<QuestionDef> Bank) World(params T[] list)
        {
            var topics = list.Select(t => new TopicDef
            {
                Id = t.Id, Title = new LocalizedText { Ru = t.Id, En = t.Id }, Shelf = t.Shelf, Category = t.Category, Lean = t.Lean,
                Russian = t.Russian
            }).ToList();
            topics.Add(new TopicDef { Id = "mixed", Title = new LocalizedText { Ru = "m", En = "m" }, Mixed = true });
            var bank = new List<QuestionDef>();
            foreach (var t in list)
            {
                for (int i = 0; i < t.Questions; i++)
                    bank.Add(new QuestionDef { Id = t.Id + "_" + i, Topic = t.Id, Format = QuestionDef.MultipleChoice, Difficulty = "easy" });
            }

            return (topics, bank);
        }

        private static (List<TopicDef> Topics, List<QuestionDef> Bank) Standard()
        {
            return World(S("s1"), S("s2"), S("s3"), K("k1"), K("k2"), K("k3"), L("l1"), L("l2"), L("l3"));
        }

        private static List<string> Doors((List<TopicDef> Topics, List<QuestionDef> Bank) world, TopicPicker.History history = null,
            int seed = 1, IReadOnlyCollection<string> seen = null)
        {
            return TopicPicker.Doors(world.Topics, world.Bank, Config, history ?? new TopicPicker.History(), seen ?? new HashSet<string>(),
                new SeededRandom(seed)).Select(d => d.Id).ToList();
        }

        [Test]
        public void OneDoorPerShelfInOrderAndTheMixedDoorLast()
        {
            var world = Standard();
            for (int seed = 0; seed < 30; seed++)
            {
                var doors = Doors(world, seed: seed);
                Assert.That(doors.Count, Is.EqualTo(4));
                Assert.That(doors[0], Does.StartWith("s"), "door 1 is the screen shelf");
                Assert.That(doors[1], Does.StartWith("k"), "door 2 is the knowledge shelf");
                Assert.That(doors[2], Does.StartWith("l"), "door 3 is the life shelf");
                Assert.That(doors[3], Is.EqualTo("mixed"));
                Assert.That(Doors(world, seed: seed), Is.EqualTo(doors), "the same seed gives the same doors");
            }
        }

        [Test]
        public void NeverOfferedTopicsAreChosenBySeed()
        {
            var world = Standard();
            var firsts = new HashSet<string>();
            for (int seed = 0; seed < 40; seed++) firsts.Add(Doors(world, seed: seed)[0]);
            Assert.That(firsts, Is.EquivalentTo(new[] { "s1", "s2", "s3" }));
        }

        [Test]
        public void TheTopicOfferedLongestAgoComesFirstAcrossGames()
        {
            // s1 and s2 were offered in earlier games (s2 most recently); s3 never was: s3 first, then s1, then s2.
            var world = Standard();
            var ever = new SeenList();
            ever.Add("s1");
            ever.Add("s2");
            var history = new TopicPicker.History { OfferedEver = ever };
            for (int seed = 0; seed < 10; seed++) Assert.That(Doors(world, history, seed)[0], Is.EqualTo("s3"));
            ever.Add("s3");
            for (int seed = 0; seed < 10; seed++) Assert.That(Doors(world, history, seed)[0], Is.EqualTo("s1"));
            ever.Add("s1");
            for (int seed = 0; seed < 10; seed++) Assert.That(Doors(world, history, seed)[0], Is.EqualTo("s2"));
        }

        [Test]
        public void ATopicOfferedEarlierInThisGameIsNotOfferedAgain()
        {
            // Nov 8 game 1: ru_places on the doors of rounds 2 and 4.
            var world = Standard();
            var history = new TopicPicker.History { OfferedInGame = new HashSet<string> { "s1", "s2", "k1" } };
            for (int seed = 0; seed < 20; seed++)
            {
                var doors = Doors(world, history, seed);
                Assert.That(doors[0], Is.EqualTo("s3"));
                Assert.That(doors[1], Is.Not.EqualTo("k1"));
            }
        }

        [Test]
        public void ACategoryChosenEarlierInThisGameIsNotOfferedAgain()
        {
            var world = World(S("films_old", "film"), S("films_new", "film"), S("music", "music"), K("k1"), L("l1"));
            var history = new TopicPicker.History { ChosenCategories = new HashSet<string> { "film" } };
            for (int seed = 0; seed < 20; seed++) Assert.That(Doors(world, history, seed)[0], Is.EqualTo("music"));
        }

        [Test]
        public void AtMostOneRussianTopicPerVote()
        {
            var ru = new T { Id = "k_ru", Shelf = TopicShelves.Knowledge, Category = "k_ru", Russian = true };
            var world = World(new T { Id = "s_ru", Shelf = TopicShelves.Screen, Category = "s_ru", Russian = true }, ru, K("k1"),
                new T { Id = "l_ru", Shelf = TopicShelves.Life, Category = "l_ru", Russian = true }, L("l1"));
            for (int seed = 0; seed < 30; seed++)
            {
                var doors = Doors(world, seed: seed);
                Assert.That(doors.Count(d => d.EndsWith("_ru")), Is.EqualTo(1), string.Join(",", doors));
            }
        }

        [Test]
        public void TheThreeDoorsAreNeverAllYoungOrAllOld()
        {
            T Young(string id, string shelf) => new T { Id = id, Shelf = shelf, Category = id, Lean = TopicShelves.LeanYoung };
            var world = World(Young("s_y", TopicShelves.Screen), Young("k_y", TopicShelves.Knowledge), Young("l_y", TopicShelves.Life),
                L("l_all"));
            for (int seed = 0; seed < 20; seed++) Assert.That(Doors(world, seed: seed).Take(3), Is.EqualTo(new[] { "s_y", "k_y", "l_all" }));
        }

        [Test]
        public void ATopicNeedsFourUnseenQuestionsUnlessNothingElseIsLeft()
        {
            var world = World(S("s1"), new T { Id = "s2", Shelf = TopicShelves.Screen, Category = "s2", Questions = 3 }, K("k1"), L("l1"));
            for (int seed = 0; seed < 20; seed++) Assert.That(Doors(world, seed: seed)[0], Is.EqualTo("s1"));
            var seen = new HashSet<string> { "s1_0", "s1_1", "s1_2", "s1_3", "s1_4" };
            Assert.That(TopicPicker.UnseenCount("s1", world.Bank, seen), Is.EqualTo(1));
            // Now neither has 4 unseen: the rule is dropped and the shelf still gets one of them.
            for (int seed = 0; seed < 20; seed++) Assert.That(Doors(world, seed: seed, seen: seen)[0], Is.EqualTo("s1").Or.EqualTo("s2"));
        }

        [Test]
        public void RulesAreDroppedWhenAShelfRunsOut()
        {
            // Every screen topic was offered in this game: the shelf still gets a door rather than staying empty.
            var world = World(S("s1"), K("k1"), L("l1"));
            var history = new TopicPicker.History { OfferedInGame = new HashSet<string> { "s1", "k1", "l1" } };
            Assert.That(Doors(world, history), Is.EqualTo(new[] { "s1", "k1", "l1", "mixed" }));
        }

        [Test]
        public void AnEmptyShelfBorrowsFromAnotherShelf()
        {
            var world = World(K("k1"), K("k2"), L("l1"));
            var doors = Doors(world);
            Assert.That(doors.Count, Is.EqualTo(4));
            Assert.That(doors.Take(3), Is.EquivalentTo(new[] { "k1", "k2", "l1" }));
            Assert.That(doors[3], Is.EqualTo("mixed"));
        }

        // ---------------------------------------------------------------- the vote (D153)

        private static int Resolve(Dictionary<string, int> votes, int seed)
        {
            return TopicPicker.ResolveVote(votes, 4, new SeededRandom(seed));
        }

        [Test]
        public void TheMajorityWins()
        {
            var votes = new Dictionary<string, int> { { "p1", 2 }, { "p2", 2 }, { "p3", 0 } };
            for (int seed = 0; seed < 10; seed++) Assert.That(Resolve(votes, seed), Is.EqualTo(2));
            Assert.That(TopicPicker.TiedDoors(votes, 4), Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void ATieIsPickedAtRandomAmongTheTiedDoorsOnly()
        {
            // Nov 8 game 1 round 4: 2 votes for door 0, 2 for door 2, 1 for door 1.
            var votes = new Dictionary<string, int> { { "p1", 0 }, { "p2", 0 }, { "p3", 2 }, { "p4", 2 }, { "p5", 1 } };
            Assert.That(TopicPicker.TiedDoors(votes, 4), Is.EqualTo(new[] { 0, 2 }));
            var winners = new HashSet<int>();
            for (int seed = 0; seed < 40; seed++)
            {
                int door = Resolve(votes, seed);
                Assert.That(Resolve(votes, seed), Is.EqualTo(door), "the same seed decides the same way");
                winners.Add(door);
            }

            Assert.That(winners, Is.EquivalentTo(new[] { 0, 2 }));
        }

        [Test]
        public void NobodyVotedPicksAnyOfTheFourDoorsBySeed()
        {
            Assert.That(TopicPicker.TiedDoors(new Dictionary<string, int>(), 4), Is.EqualTo(new[] { 0, 1, 2, 3 }));
            var doors = new HashSet<int>();
            for (int seed = 0; seed < 60; seed++) doors.Add(Resolve(new Dictionary<string, int>(), seed));
            Assert.That(doors, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
        }
    }
}
