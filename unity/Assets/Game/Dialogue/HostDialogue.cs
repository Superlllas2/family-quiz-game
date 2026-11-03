using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Show;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Dialogue
{
    /// <summary>
    /// Picks the host lines (docs/15_schemas.md, "Selection"). A listener: it reads the HostLineRequested events Core
    /// raises and says which line starts when; it never changes game state and never delays anything (a line that cannot
    /// play now is dropped). The rules:
    /// <list type="bullet">
    /// <item>Silence and the gap are measured from the end of a host's last bubble: a host may speak when
    /// now &gt;= last bubble end + hosts.minGapSeconds; a host who never spoke may always speak.</item>
    /// <item>The host silent longer (the earlier last bubble end; never spoken is silent longest; a tie goes to the seeded
    /// random) is chosen. Inside the gap: the line is dropped. Else the line is drawn by weight from that host's lines for
    /// the trigger not shown yet; with none left the other host speaks if they have some left and are outside the gap;
    /// when both bags are used up, both refill and the chosen host draws.</item>
    /// <item>Question cycle triggers are keyed by HostLineRequested.Cycle: per batch only the highest priority request of a
    /// cycle is tried, and only while no line has played for that cycle; once one did, the cycle is closed.</item>
    /// <item>The Intro plays one gameStart line from each host, the second when the first bubble ends, both exempt from
    /// the gap.</item>
    /// <item>At most one bubble per host at a time, ever.</item>
    /// </list>
    /// The bags are saved through the store after every line, so a crash keeps them. The random is reseeded from every
    /// GameStarted, so a seed replays a game's lines.
    /// </summary>
    public sealed class HostDialogue
    {
        private readonly IReadOnlyList<HostLineDef> _lines;
        private readonly IClock _clock;
        private readonly Func<int, IRandom> _randomFor;
        private readonly IHostBagStore _store;
        private readonly long _bubbleMs;
        private readonly long _gapMs;
        private readonly HashSet<string> _shown;
        private readonly Dictionary<string, SpokenLine> _last = new Dictionary<string, SpokenLine>();
        private readonly HashSet<int> _playedCycles = new HashSet<int>();

        /// <summary>The Intro gameStart lines still to start: host and start time, in start order.</summary>
        private readonly List<(string Host, long StartMs)> _pendingIntro = new List<(string, long)>();

        private IRandom _random;

        /// <param name="content">The host lines and config.hosts.</param>
        /// <param name="clock">The core clock.</param>
        /// <param name="randomFor">Makes a random from a seed (reseeded from every GameStarted).</param>
        /// <param name="store">The shuffle bags between evenings.</param>
        /// <param name="seed">The seed of the random used before the first game (the Lobby).</param>
        public HostDialogue(GameContent content, IClock clock, Func<int, IRandom> randomFor, IHostBagStore store, int seed)
            : this(content.HostLines, content.Config.Hosts, clock, randomFor, store, seed)
        {
        }

        public HostDialogue(IReadOnlyList<HostLineDef> lines, HostsConfig hosts, IClock clock, Func<int, IRandom> randomFor,
            IHostBagStore store, int seed)
        {
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            if (hosts == null) throw new ArgumentNullException(nameof(hosts));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _randomFor = randomFor ?? throw new ArgumentNullException(nameof(randomFor));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _bubbleMs = ShowDirector.ToMs(hosts.BubbleSeconds);
            _gapMs = ShowDirector.ToMs(hosts.MinGapSeconds);
            _random = _randomFor(seed);
            // Ids no longer in host_lines.json are ignored.
            var known = new HashSet<string>(_lines.Select(l => l.Id));
            _shown = new HashSet<string>((_store.Load() ?? new List<string>()).Where(known.Contains));
        }

        /// <summary>The bubbles visible now: at most one per host.</summary>
        public IReadOnlyList<SpokenLine> Showing
        {
            get
            {
                long now = _clock.NowMs;
                var showing = new List<SpokenLine>();
                foreach (var host in HostIds.All)
                {
                    if (_last.TryGetValue(host, out var line) && line.StartMs <= now && now < line.EndMs) showing.Add(line);
                }

                return showing;
            }
        }

        /// <summary>
        /// Reads one batch of events Core returned (call it with every batch, in order). Returns the lines that start now,
        /// including an Intro line that came due before this batch.
        /// </summary>
        public IReadOnlyList<SpokenLine> OnEvents(IReadOnlyList<IGameEvent> events)
        {
            var output = new List<SpokenLine>();
            StartDueIntroLines(output);
            if (events == null) return output;

            long now = _clock.NowMs;
            var bestOfCycle = BestCycleRequests(events);
            var triedCycles = new HashSet<int>();
            foreach (var e in events)
            {
                switch (e)
                {
                    case GameStarted started:
                        _random = _randomFor(started.Seed);
                        _playedCycles.Clear(); // question numbers start at 1 again in every game
                        break;
                    case PhaseStarted phase when phase.Phase != GamePhase.Intro:
                        _pendingIntro.Clear();
                        break;
                    case HostLineRequested request when HostTriggers.IsCycleTrigger(request.Trigger):
                    {
                        if (!triedCycles.Add(request.Cycle) || _playedCycles.Contains(request.Cycle)) break;
                        var line = Speak(bestOfCycle[request.Cycle], now);
                        if (line == null) break; // dropped: the cycle stays open
                        _playedCycles.Add(request.Cycle);
                        output.Add(line);
                        break;
                    }
                    case HostLineRequested request when request.Trigger == HostTriggers.GameStart:
                        BeginIntro(now, output);
                        break;
                    case HostLineRequested request:
                    {
                        var line = Speak(request, now);
                        if (line != null) output.Add(line);
                        break;
                    }
                }
            }

            return output;
        }

        /// <summary>Call once per frame. Returns the lines that start now (the second gameStart line of the Intro).</summary>
        public IReadOnlyList<SpokenLine> Tick()
        {
            var output = new List<SpokenLine>();
            StartDueIntroLines(output);
            return output;
        }

        // ---------------------------------------------------------------- the rules

        /// <summary>Per question cycle of the batch, its highest priority request (HostTriggers.CyclePriority).</summary>
        private static Dictionary<int, HostLineRequested> BestCycleRequests(IReadOnlyList<IGameEvent> events)
        {
            var best = new Dictionary<int, HostLineRequested>();
            foreach (var request in events.OfType<HostLineRequested>())
            {
                if (!HostTriggers.IsCycleTrigger(request.Trigger)) continue;
                if (!best.TryGetValue(request.Cycle, out var current) || Priority(request.Trigger) < Priority(current.Trigger))
                    best[request.Cycle] = request;
            }

            return best;
        }

        private static int Priority(string trigger)
        {
            for (int i = 0; i < HostTriggers.CyclePriority.Count; i++)
            {
                if (HostTriggers.CyclePriority[i] == trigger) return i;
            }

            return HostTriggers.CyclePriority.Count;
        }

        /// <summary>One line for a request by the normal rules (host, then gap, then bag), or null when it is dropped.</summary>
        private SpokenLine Speak(HostLineRequested request, long now)
        {
            string trigger = request.Trigger;
            if (!_lines.Any(l => l.Trigger == trigger)) return null;

            string chosen = SilentLonger();
            if (!Free(chosen, now)) return null;
            string other = HostIds.Other(chosen);
            string host = chosen;
            if (Left(trigger, chosen).Count == 0)
            {
                if (Left(trigger, other).Count == 0)
                {
                    Refill(trigger, null);
                    // A host with no lines at all for the trigger leaves it to the other host.
                    if (Left(trigger, chosen).Count == 0) host = other;
                }
                else
                {
                    host = other;
                }

                if (host == other && !Free(other, now)) return null;
            }

            return Start(Draw(Left(trigger, host)), request.Name, request.Target, request.Cycle, now);
        }

        /// <summary>
        /// A gameStart request: the host silent longer says one line at once, even inside the gap, unless that host has
        /// a bubble showing (then the other host goes first; when both have one, the first line waits for the earlier
        /// bubble end). The other host's line starts when the first bubble ends. A host with no gameStart line is passed
        /// over.
        /// </summary>
        private void BeginIntro(long now, List<SpokenLine> output)
        {
            _pendingIntro.Clear();
            var order = new List<string>();
            string first = SilentLonger();
            foreach (var host in new[] { first, HostIds.Other(first) })
            {
                if (_lines.Any(l => l.Trigger == HostTriggers.GameStart && l.Host == host)) order.Add(host);
            }

            if (order.Count == 0) return;
            if (order.Count > 1 && BubbleEnd(order[0]) > now && BubbleEnd(order[1]) <= now) order.Reverse();

            long start = Math.Max(now, BubbleEnd(order[0]));
            foreach (var host in order)
            {
                _pendingIntro.Add((host, Math.Max(start, BubbleEnd(host))));
                start = Math.Max(start, BubbleEnd(host)) + _bubbleMs;
            }

            StartDueIntroLines(output);
        }

        /// <summary>Starts the Intro lines whose time has come, each at its exact start time.</summary>
        private void StartDueIntroLines(List<SpokenLine> output)
        {
            long now = _clock.NowMs;
            while (_pendingIntro.Count > 0 && _pendingIntro[0].StartMs <= now)
            {
                var (host, startMs) = _pendingIntro[0];
                _pendingIntro.RemoveAt(0);
                long busyUntil = BubbleEnd(host);
                if (busyUntil > startMs)
                {
                    // Another line of this host got in first: wait for its bubble to end (one bubble per host).
                    _pendingIntro.Insert(0, (host, busyUntil));
                    continue;
                }

                output.Add(Start(DrawIntro(host), null, null, 0, startMs));
                // The next line follows the bubble just started.
                if (_pendingIntro.Count > 0 && _pendingIntro[0].StartMs < startMs + _bubbleMs)
                    _pendingIntro[0] = (_pendingIntro[0].Host, startMs + _bubbleMs);
            }
        }

        /// <summary>
        /// A gameStart line from this host's bag. Each host says one in the Intro, so a used up bag refills: both bags
        /// when the other host's is used up too, else only this host's (the other still has unshown lines).
        /// </summary>
        private HostLineDef DrawIntro(string host)
        {
            string trigger = HostTriggers.GameStart;
            if (Left(trigger, host).Count == 0)
                Refill(trigger, Left(trigger, HostIds.Other(host)).Count == 0 ? null : host);
            return Draw(Left(trigger, host));
        }

        private SpokenLine Start(HostLineDef def, string name, string target, int cycle, long startMs)
        {
            var line = new SpokenLine(def, name, target, cycle, startMs, startMs + _bubbleMs);
            _last[def.Host] = line;
            _shown.Add(def.Id);
            _store.Save(_shown.ToList());
            return line;
        }

        // ---------------------------------------------------------------- hosts and bags

        /// <summary>When the host's last bubble ends (or ended); long.MinValue for a host who never spoke.</summary>
        private long BubbleEnd(string host)
        {
            return _last.TryGetValue(host, out var line) ? line.EndMs : long.MinValue;
        }

        /// <summary>True when the host is outside the gap (so has no bubble showing either).</summary>
        private bool Free(string host, long now)
        {
            return !_last.TryGetValue(host, out var line) || now >= line.EndMs + _gapMs;
        }

        /// <summary>The host silent longer: the earlier last bubble end, never spoken first, a tie by the random.</summary>
        private string SilentLonger()
        {
            string a = HostIds.All[0];
            string b = HostIds.All[1];
            long endA = BubbleEnd(a);
            long endB = BubbleEnd(b);
            if (endA < endB) return a;
            if (endB < endA) return b;
            return HostIds.All[_random.Next(HostIds.All.Count)];
        }

        /// <summary>The host's lines for the trigger not shown yet, in host_lines.json order.</summary>
        private List<HostLineDef> Left(string trigger, string host)
        {
            return _lines.Where(l => l.Trigger == trigger && l.Host == host && !_shown.Contains(l.Id)).ToList();
        }

        /// <summary>Refills the trigger's bag of one host, or of both hosts when host is null.</summary>
        private void Refill(string trigger, string host)
        {
            foreach (var l in _lines)
            {
                if (l.Trigger == trigger && (host == null || l.Host == host)) _shown.Remove(l.Id);
            }
        }

        /// <summary>One line drawn by weight with the seeded random.</summary>
        private HostLineDef Draw(List<HostLineDef> candidates)
        {
            double total = candidates.Sum(l => l.Weight);
            double pick = _random.Next(int.MaxValue) / (double)int.MaxValue * total;
            foreach (var l in candidates)
            {
                pick -= l.Weight;
                if (pick < 0) return l;
            }

            return candidates[candidates.Count - 1];
        }
    }
}
