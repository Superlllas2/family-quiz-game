using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Dialogue;
using NUnit.Framework;
using static FamilyQuiz.Tests.Dialogue.DialogueFixture;

namespace FamilyQuiz.Tests.Dialogue
{
    /// <summary>
    /// The shuffle bags per trigger and host (docs/15_schemas.md, "Selection" rule 3): no line repeats, also across
    /// evenings, while others of its trigger remain; the other host first, then both bags refill; draws by weight; the
    /// bag file on disk.
    /// </summary>
    public sealed class HostBagTests
    {
        private string _folder;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "fq-bags-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_folder, HostBagFile.FileName);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
            }
            catch (Exception)
            {
                // A leftover temp folder is harmless.
            }
        }

        /// <summary>Requests the trigger count times, each time after both hosts are free again.</summary>
        private static List<string> Say(DialogueFixture f, string trigger, int count)
        {
            var ids = new List<string>();
            for (int n = 0; n < count; n++)
            {
                ids.Add(f.Batch(Request(trigger)).Single().LineId);
                f.Clock.Advance(f.QuietMs);
            }

            return ids;
        }

        private static List<HostLineDef> RoundStartLines(int perHost)
        {
            var lines = new List<HostLineDef>();
            for (int n = 0; n < perHost; n++)
            {
                lines.Add(Line("k" + n, K, HostTriggers.RoundStart));
                lines.Add(Line("i" + n, I, HostTriggers.RoundStart));
            }

            return lines;
        }

        [Test]
        public void NoLineRepeatsAcrossEveningsWhileOthersRemain()
        {
            var lines = RoundStartLines(3);
            var store = new MemoryHostBagStore();
            // Evening 1 shows 4 of the 6 lines; evening 2 (a new app start on the same store) the other 2 first.
            var evening1 = Say(new DialogueFixture(lines, store, seed: 1), HostTriggers.RoundStart, 4);
            Assert.That(evening1.Distinct().Count(), Is.EqualTo(4));
            Assert.That(store.Saves, Is.EqualTo(4), "saved after every line");

            var evening2 = Say(new DialogueFixture(lines, store, seed: 2), HostTriggers.RoundStart, 2);
            Assert.That(evening2.Intersect(evening1), Is.Empty);
            Assert.That(evening1.Concat(evening2).Distinct().Count(), Is.EqualTo(6), "every line once before any repeat");
        }

        [Test]
        public void TheOtherHostSpeaksBeforeTheBagsRefill()
        {
            // Konstantin has one line, Ilja three: once Konstantin's is shown, Ilja says the rest before any refill.
            var lines = new List<HostLineDef>
            {
                Line("k0", K, HostTriggers.RoundStart), Line("i0", I, HostTriggers.RoundStart),
                Line("i1", I, HostTriggers.RoundStart), Line("i2", I, HostTriggers.RoundStart)
            };
            var ids = Say(new DialogueFixture(lines), HostTriggers.RoundStart, 4);
            Assert.That(ids, Is.EquivalentTo(new[] { "k0", "i0", "i1", "i2" }));
        }

        [Test]
        public void BothBagsRefillWhenUsedUp()
        {
            var lines = RoundStartLines(1);
            lines.Add(Line("k_end", K, HostTriggers.RoundEnd));
            var store = new MemoryHostBagStore();
            var f = new DialogueFixture(lines, store);
            Assert.That(Say(f, HostTriggers.RoundStart, 2), Is.EquivalentTo(new[] { "k0", "i0" }));
            Say(f, HostTriggers.RoundEnd, 1);
            Assert.That(store.Shown, Is.EquivalentTo(new[] { "k0", "i0", "k_end" }));

            // Both used up: both refill, the chosen host draws; the other trigger's bag is untouched.
            var third = f.Batch(Request(HostTriggers.RoundStart)).Single();
            Assert.That(store.Shown, Is.EquivalentTo(new[] { third.LineId, "k_end" }));
            f.Clock.Advance(f.QuietMs);
            var fourth = f.Batch(Request(HostTriggers.RoundStart)).Single();
            Assert.That(fourth.LineId, Is.Not.EqualTo(third.LineId), "the other host's refilled line comes next");
        }

        [Test]
        public void ARefillForAHostWithoutLinesGoesToTheOtherHost()
        {
            var lines = new List<HostLineDef> { Line("k0", K, HostTriggers.RoundStart), Line("k1", K, HostTriggers.RoundStart) };
            var ids = Say(new DialogueFixture(lines), HostTriggers.RoundStart, 6);
            Assert.That(ids.Count, Is.EqualTo(6), "every request plays, whoever is chosen");
            for (int n = 0; n < ids.Count; n += 2)
                Assert.That(ids.Skip(n).Take(2), Is.EquivalentTo(new[] { "k0", "k1" }), "both lines before a repeat");
        }

        [Test]
        public void LinesAreDrawnByWeight()
        {
            // Konstantin alone: one line of weight 1, one of weight 3. The first draw after each refill picks the
            // heavy one about 3 times in 4.
            var lines = new List<HostLineDef>
            {
                Line("light", K, HostTriggers.RoundStart, weight: 1), Line("heavy", K, HostTriggers.RoundStart, weight: 3)
            };
            var f = new DialogueFixture(lines, seed: 5);
            const int Refills = 2000;
            var ids = Say(f, HostTriggers.RoundStart, 2 * Refills);
            int heavyFirst = 0;
            for (int n = 0; n < ids.Count; n += 2)
            {
                Assert.That(ids[n + 1], Is.Not.EqualTo(ids[n]), "the second draw takes the line left in the bag");
                if (ids[n] == "heavy") heavyFirst++;
            }

            Assert.That(heavyFirst / (double)Refills, Is.InRange(0.71, 0.79));
        }

        [Test]
        public void UnknownIdsInTheStoreAreIgnored()
        {
            var store = new MemoryHostBagStore();
            store.Save(new[] { "gone_01", "k0" });
            var f = new DialogueFixture(RoundStartLines(1), store);
            Say(f, HostTriggers.RoundStart, 1);
            Assert.That(store.Shown, Does.Not.Contain("gone_01"));
            Assert.That(store.Shown, Does.Contain("k0"));
        }

        [Test]
        public void TheBagFileRoundTrips()
        {
            var file = new HostBagFile(_path);
            Assert.That(file.Load(), Is.Empty, "a missing file means full bags");
            file.Save(new[] { "k_b", "i_a" });
            Assert.That(File.Exists(_path + ".tmp"), Is.False);
            Assert.That(new HostBagFile(_path).Load(), Is.EquivalentTo(new[] { "k_b", "i_a" }));
            file.Save(new[] { "k_b" });
            Assert.That(new HostBagFile(_path).Load(), Is.EquivalentTo(new[] { "k_b" }));
        }

        [Test]
        public void TwoEveningsOnTheBagFileDoNotRepeat()
        {
            var lines = RoundStartLines(2);
            var first = new DialogueFixture(lines);
            var dialogue1 = new HostDialogue(lines, first.Hosts, first.Clock, s => new FamilyQuiz.Core.SeededRandom(s),
                new HostBagFile(_path), 1);
            var shown = dialogue1.OnEvents(new[] { Request(HostTriggers.RoundStart) }).Select(l => l.LineId).ToList();
            first.Clock.Advance(first.QuietMs);
            shown.AddRange(dialogue1.OnEvents(new[] { Request(HostTriggers.RoundStart) }).Select(l => l.LineId));

            var second = new DialogueFixture(lines);
            var dialogue2 = new HostDialogue(lines, second.Hosts, second.Clock, s => new FamilyQuiz.Core.SeededRandom(s),
                new HostBagFile(_path), 2);
            var next = dialogue2.OnEvents(new[] { Request(HostTriggers.RoundStart) }).Single().LineId;
            Assert.That(shown.Count, Is.EqualTo(2));
            Assert.That(shown, Does.Not.Contain(next));
        }

        [Test]
        public void ABrokenBagFileIsReportedAndStartsFull()
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(_path, "{ not json");
            var errors = new List<string>();
            var file = new HostBagFile(_path, errors.Add);
            Assert.That(file.Load(), Is.Empty);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0], Does.Contain(_path));

            // The game goes on and the next save repairs the file.
            var f = new DialogueFixture(RoundStartLines(1));
            var dialogue = new HostDialogue(f.Lines, f.Hosts, f.Clock, s => new FamilyQuiz.Core.SeededRandom(s), file, 1);
            var line = dialogue.OnEvents(new[] { Request(HostTriggers.RoundStart) }).Single();
            Assert.That(new HostBagFile(_path).Load(), Is.EquivalentTo(new[] { line.LineId }));
        }
    }
}
