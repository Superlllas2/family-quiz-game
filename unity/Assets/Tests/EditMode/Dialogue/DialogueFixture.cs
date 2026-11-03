using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Dialogue;

namespace FamilyQuiz.Tests.Dialogue
{
    /// <summary>
    /// A HostDialogue on a fake clock with hand-made lines and the real config.hosts numbers, with helpers that send
    /// requests like Core would and collect every line that played.
    /// </summary>
    public sealed class DialogueFixture
    {
        public const string K = HostIds.Konstantin;
        public const string I = HostIds.Ilja;

        public DialogueFixture(IReadOnlyList<HostLineDef> lines, MemoryHostBagStore store = null, int seed = 1, ManualClock clock = null)
        {
            Lines = lines;
            Hosts = TestContent.Config().Hosts;
            Clock = clock ?? new ManualClock();
            Store = store ?? new MemoryHostBagStore();
            Dialogue = new HostDialogue(lines, Hosts, Clock, s => new SeededRandom(s), Store, seed);
        }

        public IReadOnlyList<HostLineDef> Lines { get; }
        public HostsConfig Hosts { get; }
        public ManualClock Clock { get; }
        public MemoryHostBagStore Store { get; }
        public HostDialogue Dialogue { get; }

        /// <summary>Every line that played, in order.</summary>
        public List<SpokenLine> Spoken { get; } = new List<SpokenLine>();

        public long BubbleMs => LobbyFixture.Ms(Hosts.BubbleSeconds);
        public long GapMs => LobbyFixture.Ms(Hosts.MinGapSeconds);

        /// <summary>From a bubble start to the moment its host may speak again.</summary>
        public long QuietMs => BubbleMs + GapMs;

        public static HostLineDef Line(string id, string host, string trigger, double weight = 1, string ru = null, string en = null)
        {
            return new HostLineDef
            {
                Id = id, Host = host, Trigger = trigger, Weight = weight,
                Text = new LocalizedText { Ru = ru ?? id + " ru", En = en ?? id + " en" }
            };
        }

        /// <summary>One line per host for every trigger, ids like k_trigger and i_trigger.</summary>
        public static List<HostLineDef> OnePerHost(params string[] triggers)
        {
            var lines = new List<HostLineDef>();
            foreach (var t in triggers)
            {
                lines.Add(Line("k_" + t, K, t));
                lines.Add(Line("i_" + t, I, t));
            }

            return lines;
        }

        public static HostLineRequested Request(string trigger, string name = null, string target = null, int cycle = 0)
        {
            return new HostLineRequested(trigger, name, target, cycle);
        }

        /// <summary>Sends one batch; returns the lines that started.</summary>
        public IReadOnlyList<SpokenLine> Batch(params IGameEvent[] events)
        {
            var lines = Dialogue.OnEvents(events);
            Spoken.AddRange(lines);
            return lines;
        }

        /// <summary>Moves the clock and runs one frame; returns the lines that started.</summary>
        public IReadOnlyList<SpokenLine> Advance(long ms)
        {
            Clock.Advance(ms);
            var lines = Dialogue.Tick();
            Spoken.AddRange(lines);
            return lines;
        }

        /// <summary>The Intro batch of Core: the game starts, the Intro starts, gameStart is requested.</summary>
        public IReadOnlyList<SpokenLine> Intro(int seed = 7)
        {
            return Batch(new GameStarted(1, seed, GameStarted.ModeStandard, new List<string>(), Languages.Russian),
                new PhaseStarted(GamePhase.Intro, 1), Request(HostTriggers.GameStart));
        }

        public List<SpokenLine> SpokenBy(string host)
        {
            return Spoken.Where(l => l.Host == host).ToList();
        }
    }
}
