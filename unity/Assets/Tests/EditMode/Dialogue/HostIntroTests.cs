using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using FamilyQuiz.Dialogue;
using NUnit.Framework;
using static FamilyQuiz.Tests.Dialogue.DialogueFixture;

namespace FamilyQuiz.Tests.Dialogue
{
    /// <summary>
    /// The Intro exception (docs/15_schemas.md, "Selection" rule 5 and the startCountdown note): one gameStart line from
    /// each host, the second when the first bubble ends, both exempt from the gap.
    /// </summary>
    public sealed class HostIntroTests
    {
        private static DialogueFixture Make()
        {
            return new DialogueFixture(OnePerHost(HostTriggers.StartCountdown, HostTriggers.GameStart, HostTriggers.PlayerJoined,
                HostTriggers.RoundStart));
        }

        private static long CountdownMs => LobbyFixture.Ms(TestContent.Config().Lobby.StartCountdownSeconds);

        [Test]
        public void AfterTheCountdownLineTheOtherHostOpensTheIntro()
        {
            var f = Make();
            var countdown = f.Batch(Request(HostTriggers.StartCountdown)).Single();
            f.Clock.Advance(CountdownMs);
            Assert.That(f.Dialogue.Showing.Single(), Is.SameAs(countdown), "the countdown bubble still shows");

            var first = f.Intro().Single();
            Assert.That(first.Trigger, Is.EqualTo(HostTriggers.GameStart));
            Assert.That(first.Host, Is.EqualTo(HostIds.Other(countdown.Host)), "the countdown host is inside the gap");
            Assert.That(first.StartMs, Is.EqualTo(f.Clock.NowMs), "at once");

            Assert.That(f.Advance(f.BubbleMs - 1), Is.Empty);
            var second = f.Advance(1).Single();
            Assert.That(second.Trigger, Is.EqualTo(HostTriggers.GameStart));
            Assert.That(second.Host, Is.EqualTo(countdown.Host), "the countdown host says the second line, inside the gap");
            Assert.That(second.StartMs, Is.EqualTo(first.EndMs));
            Assert.That(f.Dialogue.Showing.Single(), Is.SameAs(second));
            Assert.That(f.Advance(f.QuietMs), Is.Empty, "one line per host");
        }

        [Test]
        public void ALateFrameStillStartsTheSecondLineAtTheFirstBubbleEnd()
        {
            var f = Make();
            var first = f.Intro().Single();
            var second = f.Advance(f.BubbleMs + 1234).Single();
            Assert.That(second.Host, Is.EqualTo(HostIds.Other(first.Host)));
            Assert.That(second.StartMs, Is.EqualTo(first.EndMs));
            Assert.That(second.EndMs, Is.EqualTo(first.EndMs + f.BubbleMs));
        }

        [Test]
        public void TheSecondLineComesBeforeTheNextBatchOfALateFrame()
        {
            // The frame loop jumps past the first bubble end and Core's next batch starts TopicVote: the second line was due
            // first, so it still plays (at its exact time); the TopicVote batch does not cancel it.
            var f = Make();
            var first = f.Intro().Single();
            f.Clock.Advance(f.BubbleMs + 10);
            var lines = f.Batch(new FamilyQuiz.Core.Events.PhaseStarted(GamePhase.TopicVote, 2));
            Assert.That(lines.Single().StartMs, Is.EqualTo(first.EndMs));
        }

        [Test]
        public void AnotherPhaseCancelsTheSecondLine()
        {
            var f = Make();
            f.Intro();
            f.Clock.Advance(f.BubbleMs / 2);
            f.Batch(new FamilyQuiz.Core.Events.PhaseStarted(GamePhase.TopicVote, 2));
            Assert.That(f.Advance(f.QuietMs), Is.Empty);
            Assert.That(f.Spoken.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheIntroPhaseItselfDoesNotCancel()
        {
            var f = Make();
            f.Intro();
            f.Batch(new FamilyQuiz.Core.Events.PhaseStarted(GamePhase.Intro, 1));
            Assert.That(f.Advance(f.BubbleMs).Count, Is.EqualTo(1));
        }

        [Test]
        public void WhenBothHostsShowABubbleTheIntroWaitsForTheEarlierOne()
        {
            // A join greeting, then everyone is Ready at once: both hosts have a bubble when the Intro starts.
            var f = Make();
            var greeting = f.Batch(Request(HostTriggers.PlayerJoined, "Ann")).Single();
            f.Clock.Advance(100);
            var countdown = f.Batch(Request(HostTriggers.StartCountdown)).Single();
            Assert.That(countdown.Host, Is.EqualTo(HostIds.Other(greeting.Host)));
            f.Clock.Advance(CountdownMs);
            Assert.That(f.Intro(), Is.Empty, "never over a showing bubble");

            var first = f.Advance(greeting.EndMs - f.Clock.NowMs).Single();
            Assert.That(first.Host, Is.EqualTo(greeting.Host));
            Assert.That(first.StartMs, Is.EqualTo(greeting.EndMs));
            var second = f.Advance(f.BubbleMs).Single();
            Assert.That(second.Host, Is.EqualTo(countdown.Host));
            Assert.That(second.StartMs, Is.EqualTo(first.EndMs));
        }

        [Test]
        public void AHostWithNoGameStartLineIsPassedOver()
        {
            var f = new DialogueFixture(new List<HostLineDef> { Line("i_go", I, HostTriggers.GameStart) });
            var lines = f.Intro();
            Assert.That(lines.Single().Host, Is.EqualTo(I));
            Assert.That(f.Advance(f.QuietMs), Is.Empty);
        }

        [Test]
        public void EachIntroGivesBothHostsAFreshLineWhileTheyLast()
        {
            // Konstantin has two gameStart lines, Ilja one: three evenings with one Intro each.
            var store = new MemoryHostBagStore();
            var lines = new List<HostLineDef>
            {
                Line("k_go1", K, HostTriggers.GameStart), Line("k_go2", K, HostTriggers.GameStart), Line("i_go", I, HostTriggers.GameStart)
            };
            var konstantin = new List<string>();
            for (int evening = 0; evening < 3; evening++)
            {
                var f = new DialogueFixture(lines, store, seed: evening);
                f.Intro(seed: evening);
                f.Advance(f.BubbleMs);
                Assert.That(f.Spoken.Select(l => l.Host).OrderBy(h => h), Is.EqualTo(new[] { I, K }), "one line per host");
                konstantin.Add(f.SpokenBy(K).Single().LineId);
            }

            Assert.That(konstantin.Take(2).Distinct().Count(), Is.EqualTo(2), "no repeat while another remains");
        }
    }
}
