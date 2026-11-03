using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using NUnit.Framework;
using static FamilyQuiz.Tests.Dialogue.DialogueFixture;

namespace FamilyQuiz.Tests.Dialogue
{
    /// <summary>
    /// Which line plays and who says it (docs/15_schemas.md, "Selection"; docs/19_acceptance.md, Day 4 "Host lines"):
    /// one line per question cycle by priority, the gap, the host silent longer, one bubble per host.
    /// </summary>
    public sealed class HostSelectionTests
    {
        private static DialogueFixture WithAllTriggers()
        {
            return new DialogueFixture(OnePerHost(HostTriggers.All.ToArray()));
        }

        [Test]
        public void OneLinePerQuestionCycleByPriority()
        {
            var f = WithAllTriggers();
            // Core's order does not matter: the highest priority of the cycle plays, the others are dropped.
            var lines = f.Batch(new PhaseStarted(GamePhase.Reveal, 5),
                Request(HostTriggers.FastAnswer, "Fast", cycle: 1),
                Request(HostTriggers.Streak3, "Streaky", cycle: 1),
                Request(HostTriggers.NewLeader, "Leader", cycle: 1),
                Request(HostTriggers.NobodyCorrect, cycle: 1));
            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0].Trigger, Is.EqualTo(HostTriggers.NewLeader));
            Assert.That(lines[0].Name, Is.EqualTo("Leader"));
            Assert.That(lines[0].Cycle, Is.EqualTo(1));

            // The next cycle plays again once a host is free.
            f.Clock.Advance(f.QuietMs);
            lines = f.Batch(Request(HostTriggers.FastAnswer, "Fast", cycle: 2),
                Request(HostTriggers.LastPlaceCorrect, "Last", cycle: 2));
            Assert.That(lines.Select(l => l.Trigger), Is.EqualTo(new[] { HostTriggers.LastPlaceCorrect }));
        }

        [Test]
        public void EveryPairOfCycleTriggersPlaysTheHigherOne()
        {
            var order = HostTriggers.CyclePriority;
            for (int hi = 0; hi < order.Count; hi++)
            {
                for (int lo = hi + 1; lo < order.Count; lo++)
                {
                    var f = WithAllTriggers();
                    var lines = f.Batch(Request(order[lo], cycle: 1), Request(order[hi], cycle: 1));
                    Assert.That(lines.Select(l => l.Trigger), Is.EqualTo(new[] { order[hi] }), order[hi] + " over " + order[lo]);
                }
            }
        }

        [Test]
        public void PowerUpUsedNeverPlaysAfterALineOfItsCycle()
        {
            var f = WithAllTriggers();
            Assert.That(f.Batch(Request(HostTriggers.EveryoneCorrect, cycle: 3)).Count, Is.EqualTo(1));
            f.Clock.Advance(f.QuietMs);
            // Scores of cycle 3, both hosts free: still dropped.
            Assert.That(f.Batch(Request(HostTriggers.PowerUpUsed, "Ann", "Bob", 3)), Is.Empty);
            Assert.That(f.Batch(Request(HostTriggers.NobodyCorrect, cycle: 3)), Is.Empty, "a closed cycle stays closed");
            // The next cycle is open.
            var lines = f.Batch(Request(HostTriggers.PowerUpUsed, "Ann", "Bob", 4));
            Assert.That(lines.Select(l => l.Trigger), Is.EqualTo(new[] { HostTriggers.PowerUpUsed }));
        }

        [Test]
        public void ACycleLineDroppedByTheGapLetsAPowerUpPlayLater()
        {
            var f = WithAllTriggers();
            // Both hosts speak just before the Reveal: both are inside the gap.
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)).Count, Is.EqualTo(1));
            Assert.That(f.Batch(Request(HostTriggers.LastCall)).Count, Is.EqualTo(1));
            Assert.That(f.Dialogue.Showing.Count, Is.EqualTo(2));

            f.Clock.Advance(f.BubbleMs);
            Assert.That(f.Batch(Request(HostTriggers.NewLeader, "Leader", cycle: 1)), Is.Empty, "dropped, never delayed");
            Assert.That(f.Advance(f.GapMs - 1), Is.Empty, "a dropped line never comes back");

            // In Scores, outside the gap: the power up of the same cycle plays.
            f.Clock.Advance(1);
            var lines = f.Batch(Request(HostTriggers.PowerUpUsed, "Ann", "Bob", 1));
            Assert.That(lines.Count, Is.EqualTo(1));
            Assert.That(lines[0].Trigger, Is.EqualTo(HostTriggers.PowerUpUsed));
            Assert.That(lines[0].Text(Languages.English), Is.EqualTo("i_powerUpUsed en").Or.EqualTo("k_powerUpUsed en"));
            f.Clock.Advance(f.QuietMs);
            Assert.That(f.Batch(Request(HostTriggers.PowerUpUsed, "Bob", "Ann", 1)), Is.Empty, "one line per cycle");
        }

        [Test]
        public void ACycleWithoutLinesForItsTopTriggerPlaysNothing()
        {
            // Only the highest priority request of the batch is tried; a trigger with no lines says nothing.
            var f = new DialogueFixture(OnePerHost(HostTriggers.FastAnswer, HostTriggers.PowerUpUsed));
            Assert.That(f.Batch(Request(HostTriggers.NewLeader, "A", cycle: 1), Request(HostTriggers.FastAnswer, "A", cycle: 1)), Is.Empty);
            // Nothing played, so the cycle is still open for a power up.
            Assert.That(f.Batch(Request(HostTriggers.PowerUpUsed, "A", "B", 1)).Count, Is.EqualTo(1));
        }

        [Test]
        public void PlayerJoinedGreetsWithTheGap()
        {
            var f = WithAllTriggers();
            Assert.That(f.Batch(Request(HostTriggers.PlayerJoined, "Ann")).Single().Name, Is.EqualTo("Ann"));
            // The other host never spoke, so they greet the second player.
            f.Clock.Advance(100);
            Assert.That(f.Batch(Request(HostTriggers.PlayerJoined, "Bob")).Single().Name, Is.EqualTo("Bob"));
            // Both hosts are inside the gap: the third join is not greeted.
            f.Clock.Advance(100);
            Assert.That(f.Batch(Request(HostTriggers.PlayerJoined, "Cid")), Is.Empty);
            // The first host is outside the gap again: the next join is greeted, by them.
            f.Clock.Advance(f.QuietMs - 200);
            var line = f.Batch(Request(HostTriggers.PlayerJoined, "Dan")).Single();
            Assert.That(line.Name, Is.EqualTo("Dan"));
            Assert.That(line.Host, Is.EqualTo(f.Spoken[0].Host));
            Assert.That(f.Spoken.Select(l => l.Name), Is.EqualTo(new[] { "Ann", "Bob", "Dan" }));
        }

        [Test]
        public void TheGapCountsFromTheEndOfTheBubble()
        {
            var f = new DialogueFixture(new List<HostLineDef>
            {
                Line("k1", K, HostTriggers.RoundStart), Line("k2", K, HostTriggers.RoundStart), Line("k3", K, HostTriggers.RoundStart)
            });
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)).Count, Is.EqualTo(1));
            f.Clock.Advance(f.QuietMs - 1);
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)), Is.Empty, "1 ms inside the gap");
            f.Clock.Advance(1);
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)).Count, Is.EqualTo(1), "the gap is over");
        }

        [Test]
        public void TheHostSilentLongerSpeaks()
        {
            var f = WithAllTriggers();
            var first = f.Batch(Request(HostTriggers.RoundStart)).Single();
            f.Clock.Advance(500);
            var second = f.Batch(Request(HostTriggers.RoundEnd)).Single();
            Assert.That(second.Host, Is.EqualTo(HostIds.Other(first.Host)), "never spoken is silent longest");

            // Both free: the one whose bubble ended earlier speaks, then the other, and so on.
            f.Clock.Advance(f.QuietMs);
            Assert.That(f.Batch(Request(HostTriggers.FinalStart)).Single().Host, Is.EqualTo(first.Host));
            f.Clock.Advance(f.QuietMs);
            Assert.That(f.Batch(Request(HostTriggers.LastCall)).Single().Host, Is.EqualTo(second.Host));
            f.Clock.Advance(f.QuietMs);
            Assert.That(f.Batch(Request(HostTriggers.RaceWinner, "W")).Single().Host, Is.EqualTo(first.Host));
        }

        [Test]
        public void TheOtherHostSpeaksForAHostWithoutLinesOnlyOutsideTheGap()
        {
            // Ilja alone has roundEnd lines, Konstantin alone finalStart and roundStart lines.
            var f = new DialogueFixture(new List<HostLineDef>
            {
                Line("i_end", I, HostTriggers.RoundEnd), Line("k_final", K, HostTriggers.FinalStart),
                Line("k_start", K, HostTriggers.RoundStart), Line("k_start2", K, HostTriggers.RoundStart)
            });
            Assert.That(f.Batch(Request(HostTriggers.RoundEnd)).Single().Host, Is.EqualTo(I));
            f.Clock.Advance(100);
            Assert.That(f.Batch(Request(HostTriggers.FinalStart)).Single().Host, Is.EqualTo(K));

            // Ilja is silent longer and outside the gap but has no roundStart line; Konstantin is still inside it: dropped.
            f.Clock.Advance(f.QuietMs - 100);
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)), Is.Empty, "the other host is inside the gap");
            f.Clock.Advance(100);
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)).Single().Host, Is.EqualTo(K), "now he is outside it");
        }

        [Test]
        public void NeverTwoBubblesPerHostAtOnce()
        {
            var random = new SeededRandom(99);
            var lines = new List<HostLineDef>();
            foreach (var t in HostTriggers.All)
            {
                for (int n = 0; n < 3; n++)
                {
                    lines.Add(Line("k_" + t + n, K, t));
                    lines.Add(Line("i_" + t + n, I, t));
                }
            }

            var f = new DialogueFixture(lines);
            int cycle = 0;
            for (int step = 0; step < 3000; step++)
            {
                f.Advance(random.Next((int)f.QuietMs));
                string trigger = HostTriggers.All[random.Next(HostTriggers.All.Count)];
                if (random.Next(10) == 0) f.Batch(new PhaseStarted(GamePhase.TopicVote, step));
                if (trigger == HostTriggers.GameStart) f.Batch(new PhaseStarted(GamePhase.Intro, step), Request(trigger));
                else if (HostTriggers.IsCycleTrigger(trigger)) f.Batch(Request(trigger, "N", "T", random.Next(3) == 0 ? ++cycle : cycle));
                else f.Batch(Request(trigger, "N"));
                Assert.That(f.Dialogue.Showing.Count, Is.LessThanOrEqualTo(HostIds.All.Count));
                Assert.That(f.Dialogue.Showing.Select(l => l.Host).Distinct().Count(), Is.EqualTo(f.Dialogue.Showing.Count));
            }

            Assert.That(f.Spoken.Count, Is.GreaterThan(500));
            foreach (var host in HostIds.All)
            {
                var spoken = f.SpokenBy(host);
                for (int i = 1; i < spoken.Count; i++)
                {
                    Assert.That(spoken[i].StartMs, Is.GreaterThanOrEqualTo(spoken[i - 1].EndMs), host + " line " + i + " overlaps");
                    if (spoken[i].Trigger != HostTriggers.GameStart)
                        Assert.That(spoken[i].StartMs, Is.GreaterThanOrEqualTo(spoken[i - 1].EndMs + f.GapMs), host + " line " + i + " inside the gap");
                }
            }

            var perCycle = f.Spoken.Where(l => HostTriggers.IsCycleTrigger(l.Trigger)).GroupBy(l => l.Cycle);
            Assert.That(perCycle.All(g => g.Count() == 1), Is.True, "one line per question cycle");
        }

        [Test]
        public void NameAndTargetAreFilledInEitherLanguage()
        {
            var f = new DialogueFixture(new List<HostLineDef>
            {
                Line("k_pu", K, HostTriggers.PowerUpUsed, ru: "{name} бьёт {target}!", en: "{name} hits {target}!"),
                Line("i_pu", I, HostTriggers.PowerUpUsed, ru: "{name} бьёт {target}!", en: "{name} hits {target}!"),
                Line("k_rs", K, HostTriggers.RoundStart, ru: "Раунд, {name}", en: "Round, {name}"),
                Line("i_rs", I, HostTriggers.RoundStart, ru: "Раунд, {name}", en: "Round, {name}")
            });
            var line = f.Batch(Request(HostTriggers.PowerUpUsed, "Аня", "Bob", 1)).Single();
            Assert.That(line.Text(Languages.Russian), Is.EqualTo("Аня бьёт Bob!"));
            Assert.That(line.Text(Languages.English), Is.EqualTo("Аня hits Bob!"));
            Assert.That(line.EndMs - line.StartMs, Is.EqualTo(f.BubbleMs));
            Assert.That(line.Def.Id, Is.EqualTo(line.LineId));

            var plain = f.Batch(Request(HostTriggers.RoundStart)).Single();
            Assert.That(plain.Text(Languages.English), Is.EqualTo("Round, "), "a missing name becomes empty");
            Assert.That(plain.Text(Languages.Russian), Is.EqualTo("Раунд, "));
        }

        [Test]
        public void ATriggerWithNoLinesSaysNothing()
        {
            var f = new DialogueFixture(OnePerHost(HostTriggers.RoundStart));
            Assert.That(f.Batch(Request(HostTriggers.RoundEnd)), Is.Empty);
            Assert.That(f.Batch(Request(HostTriggers.RoundStart)).Count, Is.EqualTo(1), "the dropped request took no host");
        }

        [Test]
        public void TheSameSeedReplaysTheSameLines()
        {
            var lines = new List<HostLineDef>();
            for (int n = 0; n < 5; n++)
            {
                lines.Add(Line("k" + n, K, HostTriggers.RoundStart));
                lines.Add(Line("i" + n, I, HostTriggers.RoundStart));
            }

            List<string> Play(int constructorSeed)
            {
                var f = new DialogueFixture(lines, seed: constructorSeed);
                f.Batch(new GameStarted(1, 4242, GameStarted.ModeStandard, new List<string>(), Languages.Russian));
                for (int n = 0; n < 10; n++)
                {
                    f.Batch(Request(HostTriggers.RoundStart));
                    f.Clock.Advance(f.QuietMs);
                }

                return f.Spoken.Select(l => l.LineId).ToList();
            }

            Assert.That(Play(1), Is.EqualTo(Play(2)), "GameStarted.Seed decides, not the constructor seed");
        }
    }
}
