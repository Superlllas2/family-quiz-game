using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Dialogue;
using NUnit.Framework;

namespace FamilyQuiz.Tests.Dialogue
{
    /// <summary>
    /// The host lines over real games: the six simulated players drive the real Game exactly like SimulatedGameTests, every
    /// event batch goes to a HostDialogue with the real content/host_lines.json, and Tick runs every step. Checks
    /// docs/19_acceptance.md, Day 4 "Host lines": at most one line per question cycle, never two bubbles per host, only
    /// requested triggers.
    /// </summary>
    public sealed class HostDialogueGameTests
    {
        private const int Sessions = 3;

        private sealed class Run
        {
            public LobbyFixture F;
            public HostDialogue Dialogue;
            public int Game;
            public readonly List<(int Game, SpokenLine Line)> Spoken = new List<(int, SpokenLine)>();
            public readonly List<(int Game, HostLineRequested Request)> Requested = new List<(int, HostLineRequested)>();
        }

        private static void Feed(Run run, IReadOnlyList<IGameEvent> events)
        {
            foreach (var e in events)
            {
                if (e is GameStarted started) run.Game = started.Game;
                if (e is HostLineRequested request) run.Requested.Add((run.Game, request));
            }

            foreach (var line in run.Dialogue.OnEvents(events)) run.Spoken.Add((run.Game, line));
            Step(run);
        }

        private static void Step(Run run)
        {
            foreach (var line in run.Dialogue.Tick()) run.Spoken.Add((run.Game, line));
            var showing = run.Dialogue.Showing;
            Assert.That(showing.Select(l => l.Host).Distinct().Count(), Is.EqualTo(showing.Count), "one bubble per host");
        }

        /// <summary>One game from the Lobby (everyone Ready) back to the Lobby; the loop of SimulatedGameTests.PlayGame.</summary>
        private static void PlayGame(Run run, SimulatedPlayer[] sims)
        {
            var f = run.F;
            Feed(run, f.ReadyAll());
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            long limit = f.Clock.NowMs + 3600000;
            while (f.State.Phase != GamePhase.Lobby)
            {
                long now = f.Clock.NowMs;
                long next = f.State.PhaseEndsAtMs ?? long.MaxValue;
                foreach (var s in sims)
                {
                    var at = s.PlannedAtMs(f.Game);
                    if (at.HasValue) next = Math.Min(next, at.Value);
                }

                Assert.That(next, Is.LessThan(limit), "the game never ended");
                if (next > now) f.Clock.Advance(next - now);
                Feed(run, f.Game.Tick());
                foreach (var s in sims)
                {
                    var input = s.Act(f.Game);
                    if (input == null) continue;
                    var events = f.Send(input);
                    Assert.That(LobbyFixture.AckOf(events).Ok, Is.True, s.Profile + " " + input.GetType().Name + " refused");
                    Feed(run, events);
                }
            }
        }

        [Test]
        public void RealGamesPlayHostLinesByTheRules()
        {
            var store = new MemoryHostBagStore();
            var all = new List<Run>();
            for (int session = 1; session <= Sessions; session++)
            {
                var f = new LobbyFixture(session * 7919);
                var run = new Run
                {
                    F = f, Dialogue = new HostDialogue(f.Content, f.Clock, s => new SeededRandom(s), store, session)
                };
                var sims = SimulatedPlayer.SixProfiles(session);
                foreach (var s in sims)
                {
                    Feed(run, f.Join(s.Conn, s.Name));
                    f.Clock.Advance(LobbyFixture.Ms(f.Config.Hosts.BubbleSeconds));
                    Step(run);
                }

                PlayGame(run, sims);
                PlayGame(run, sims);
                Check(run);
                all.Add(run);
            }

            var spoken = all.SelectMany(r => r.Spoken.Select(s => s.Line)).ToList();
            Assert.That(spoken.Count, Is.GreaterThan(0));
            Assert.That(spoken.Select(l => l.Trigger).Distinct().Count(), Is.GreaterThan(8), "many triggers play");
            Console.WriteLine("Host lines over " + 2 * Sessions + " games: " + spoken.Count + " spoken of "
                + all.Sum(r => r.Requested.Count) + " requested; by trigger: "
                + string.Join(", ", spoken.GroupBy(l => l.Trigger).OrderBy(g => g.Key).Select(g => g.Key + " " + g.Count())));
        }

        private static void Check(Run run)
        {
            var hosts = run.F.Config.Hosts;
            long gapMs = LobbyFixture.Ms(hosts.MinGapSeconds);
            foreach (var (game, line) in run.Spoken)
            {
                Assert.That(run.Requested.Any(r => r.Game == game && r.Request.Trigger == line.Trigger && r.Request.Name == line.Name
                    && r.Request.Target == line.Target && r.Request.Cycle == line.Cycle), Is.True,
                    line.Trigger + " was never requested in game " + game);
            }

            foreach (var host in HostIds.All)
            {
                var lines = run.Spoken.Select(s => s.Line).Where(l => l.Host == host).OrderBy(l => l.StartMs).ToList();
                for (int i = 1; i < lines.Count; i++)
                {
                    Assert.That(lines[i].StartMs, Is.GreaterThanOrEqualTo(lines[i - 1].EndMs), host + " has two bubbles at once");
                    if (lines[i].Trigger != HostTriggers.GameStart)
                        Assert.That(lines[i].StartMs, Is.GreaterThanOrEqualTo(lines[i - 1].EndMs + gapMs), host + " spoke inside the gap");
                }
            }

            var cycleLines = run.Spoken.Where(s => HostTriggers.IsCycleTrigger(s.Line.Trigger)).ToList();
            foreach (var group in cycleLines.GroupBy(s => (s.Game, s.Line.Cycle)))
                Assert.That(group.Count(), Is.EqualTo(1), "game " + group.Key.Game + " cycle " + group.Key.Cycle + " has more than one line");
            foreach (int game in new[] { 1, 2 })
                Assert.That(cycleLines.Count(s => s.Game == game), Is.GreaterThan(0), "game " + game + " has question cycle lines");

            // The Intro (first game only) plays one gameStart line from each host.
            var intro = run.Spoken.Where(s => s.Line.Trigger == HostTriggers.GameStart).ToList();
            Assert.That(intro.Select(s => s.Line.Host).OrderBy(h => h), Is.EqualTo(HostIds.All.OrderBy(h => h)));
            Assert.That(intro.All(s => s.Game == 1), Is.True);
            Assert.That(run.Spoken.Count(s => s.Line.Trigger == HostTriggers.PlayerJoined), Is.EqualTo(6),
                "joins spaced by a bubble are all greeted (the two hosts take turns)");
        }
    }
}
