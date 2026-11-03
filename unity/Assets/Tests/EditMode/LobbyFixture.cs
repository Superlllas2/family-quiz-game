using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.Show;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// A game in the Lobby on a fake clock with the real content/, with helpers that send commands like the phones
    /// would and step the clock like the frame loop would.
    /// </summary>
    public sealed class LobbyFixture
    {
        private int _inputNumber;

        public LobbyFixture(int seed = 42, GameContent content = null, IEnumerable<string> seenQuestions = null,
            IEnumerable<string> seenRace = null, IEnumerable<string> seenFacts = null)
        {
            Content = content ?? TestContent.Content();
            Config = Content.Config;
            Characters = Content.Characters;
            Clock = new ManualClock();
            Seeds = new List<int>();
            int nextSeed = seed;
            Game = new Game(Content, Clock, new SeededRandom(1234), () =>
            {
                Seeds.Add(nextSeed);
                return nextSeed++;
            }, seenQuestions, seenRace, seenFacts);
            Game.Start();
        }

        public GameContent Content { get; }
        public GameConfig Config { get; }
        public IReadOnlyList<CharacterDef> Characters { get; }
        public ManualClock Clock { get; }
        public Game Game { get; }
        public GameState State => Game.State;

        /// <summary>Every seed the game asked for, in order.</summary>
        public List<int> Seeds { get; }

        public static long Ms(double seconds)
        {
            return ShowDirector.ToMs(seconds);
        }

        public long CountdownMs => Ms(Config.Lobby.StartCountdownSeconds);
        public long QuestionShowMs => Ms(Config.Timers.QuestionShow) + Ms(Config.Staging.ExtraSeconds.QuestionShow);
        public long RevealMs => Ms(Config.Timers.Reveal) + Ms(Config.Staging.ExtraSeconds.Reveal);
        public long ScoresMs => Ms(Config.Timers.Scores) + Ms(Config.Staging.ExtraSeconds.Scores);
        public long ResultsMs => Ms(Config.Timers.Results);
        public long IntroMs => Ms(Config.Timers.Intro);
        public long TopicVoteMs => Ms(Config.Timers.TopicVote);

        /// <summary>A vote nobody finishes: the vote timer plus net.inputGraceMs (D84).</summary>
        public long VoteEndMs => TopicVoteMs + Config.Net.InputGraceMs;
        public long RoundEndMs => Ms(Config.Timers.RoundEnd) + Ms(Config.Staging.ExtraSeconds.RoundEnd);
        public long FinalIntroMs => Ms(Config.Timers.FinalIntro);
        public long RaceWindowMs => Ms(Config.Race.Answer);
        public long RaceRevealMs => Ms(Config.Race.Reveal);
        public long ClosestWindowMs => Ms(Config.Timers.Answer.ClosestValue);
        public long WindowMs => Ms(Config.Timers.Answer.MultipleChoice);
        public long GraceMs => Config.Net.InputGraceMs;
        public long LastCallMs => Ms(Config.Timers.LastCall);

        public IReadOnlyList<IGameEvent> Send(IGameCommand command)
        {
            return Game.Handle(command);
        }

        public IReadOnlyList<IGameEvent> Join(string conn, string name, string lang = "ru")
        {
            Game.Handle(new ConnectionOpened(conn));
            return Game.Handle(new PlayerJoin(conn, name, lang));
        }

        /// <summary>Joins and asserts the join worked; returns the new seat.</summary>
        public PlayerState Seat(string conn, string name)
        {
            var events = Join(conn, name);
            var joined = events.OfType<PlayerJoined>().SingleOrDefault();
            Assert.That(joined, Is.Not.Null, "join of " + name + " failed: " + Describe(events));
            return State.FindById(joined.PlayerId);
        }

        /// <summary>Seats players c1, c2, ... named P1, P2, ...</summary>
        public PlayerState[] SeatPlayers(int count)
        {
            var seats = new PlayerState[count];
            for (int i = 0; i < count; i++) seats[i] = Seat("c" + (i + 1), "P" + (i + 1));
            return seats;
        }

        public string NextInputId()
        {
            return "in" + ++_inputNumber;
        }

        public IReadOnlyList<IGameEvent> Lobby(string conn, string action)
        {
            return Game.Handle(new LobbyAction(conn, NextInputId(), action));
        }

        /// <summary>Picks the TV assigned character for a seat that has not picked one yet (the phone's pickCharacter tap).</summary>
        public IReadOnlyList<IGameEvent> Pick(PlayerState player)
        {
            return Game.Handle(new PickCharacter(player.Conn, NextInputId(), player.CharacterId));
        }

        /// <summary>
        /// Every connected seat picks its character (if it has not) and taps Ready, in seat order (D76). With players.min
        /// or more connected, the last Ready starts the countdown. Returns every event.
        /// </summary>
        public List<IGameEvent> ReadyAll()
        {
            var all = new List<IGameEvent>();
            foreach (var p in State.Players.ToList())
            {
                if (!p.Connected) continue;
                if (!p.CharacterPicked) all.AddRange(Pick(p));
                all.AddRange(Lobby(p.Conn, LobbyActions.Ready));
            }

            return all;
        }

        /// <summary>A choice for the current phase.</summary>
        public IReadOnlyList<IGameEvent> Choice(string conn, int choice, long? ms, string inputId = null)
        {
            return Game.Handle(new SubmitAnswer(conn, inputId ?? NextInputId(), State.PhaseId, InputKinds.Choice, choice, ms));
        }

        /// <summary>A topic vote for the current phase.</summary>
        public IReadOnlyList<IGameEvent> Vote(string conn, int door, string inputId = null)
        {
            return Game.Handle(new SubmitAnswer(conn, inputId ?? NextInputId(), State.PhaseId, InputKinds.Vote, null, null, door: door));
        }

        /// <summary>A closest value guess for the current phase.</summary>
        public IReadOnlyList<IGameEvent> Value(string conn, double value, long? ms, string inputId = null)
        {
            return Game.Handle(new SubmitAnswer(conn, inputId ?? NextInputId(), State.PhaseId, InputKinds.Value, null, ms, value: value));
        }

        /// <summary>A race answer for the current phase.</summary>
        public IReadOnlyList<IGameEvent> TrueFalse(string conn, bool answer, long? ms, string inputId = null)
        {
            return Game.Handle(new SubmitAnswer(conn, inputId ?? NextInputId(), State.PhaseId, InputKinds.TrueFalse, null, ms,
                answer: answer));
        }

        /// <summary>The right display index of the current question.</summary>
        public int Right => State.Question.CorrectDisplayIndex;

        public int Wrong => (State.Question.CorrectDisplayIndex + 1) % ProtocolLimits.ChoiceCount;

        /// <summary>Moves the clock by ms and runs one frame.</summary>
        public IReadOnlyList<IGameEvent> Advance(long ms)
        {
            Clock.Advance(ms);
            return Game.Tick();
        }

        /// <summary>Steps the clock in small frames until the phase changes or maxMs passed; returns every event.</summary>
        public List<IGameEvent> RunUntilPhase(GamePhase phase, long maxMs = 600000, long stepMs = 10)
        {
            var all = new List<IGameEvent>();
            long waited = 0;
            while (State.Phase != phase)
            {
                Assert.That(waited, Is.LessThanOrEqualTo(maxMs), "phase " + phase + " never came; now in " + State.Phase);
                all.AddRange(Advance(stepMs));
                waited += stepMs;
            }

            return all;
        }

        /// <summary>
        /// Seats count players, everyone taps Ready, and runs the countdown and the Intro (first game only): the first
        /// TopicVote is open.
        /// </summary>
        public PlayerState[] ToFirstVote(int count)
        {
            var seats = SeatPlayers(count);
            StartNextGame();
            return seats;
        }

        /// <summary>From the Lobby: everyone taps Ready, then the countdown and the Intro (first game only) run: TopicVote is open.</summary>
        public void StartNextGame()
        {
            ReadyAll();
            Assert.That(State.Phase, Is.EqualTo(GamePhase.StartCountdown));
            Advance(CountdownMs);
            if (State.Phase == GamePhase.Intro) Advance(IntroMs);
            Assert.That(State.Phase, Is.EqualTo(GamePhase.TopicVote));
        }

        /// <summary>
        /// Seats count players, everyone taps Ready, and runs the countdown, the Intro, the topic vote (nobody votes) and
        /// QuestionShow: the first Answer phase (multiple choice) is open.
        /// </summary>
        public PlayerState[] ToFirstAnswer(int count)
        {
            var seats = ToFirstVote(count);
            Advance(VoteEndMs);
            Assert.That(State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            Advance(QuestionShowMs);
            Assert.That(State.Phase, Is.EqualTo(GamePhase.Answer));
            return seats;
        }

        /// <summary>Steps the clock in 50 ms frames until the given phase starts again (at most maxMs of game clock).</summary>
        public List<IGameEvent> SkipTo(GamePhase phase, long maxMs = 3600000)
        {
            // Waits for the phase to start anew, so it never returns at once when the game is already in it.
            var all = new List<IGameEvent>();
            long waited = 0;
            while (true)
            {
                Assert.That(waited, Is.LessThanOrEqualTo(maxMs), "phase " + phase + " never started; now in " + State.Phase);
                var events = Advance(50);
                all.AddRange(events);
                waited += 50;
                if (events.OfType<PhaseStarted>().Any(p => p.Phase == phase)) return all;
            }
        }

        /// <summary>Runs a running game to its end and back to the Lobby (D76) with nobody answering.</summary>
        public List<IGameEvent> RunToLobby()
        {
            Assert.That(State.Phase, Is.Not.EqualTo(GamePhase.Lobby), "no game is running");
            return RunUntilPhase(GamePhase.Lobby, 3600000, 50);
        }

        public static string RefusalOf(IReadOnlyList<IGameEvent> events)
        {
            return events.OfType<JoinRefused>().Select(r => r.Reason).SingleOrDefault();
        }

        public static InputAcked AckOf(IReadOnlyList<IGameEvent> events)
        {
            return events.OfType<InputAcked>().Single();
        }

        public static string Describe(IEnumerable<IGameEvent> events)
        {
            return string.Join(", ", events.Select(Describe));
        }

        public static string Describe(IGameEvent e)
        {
            switch (e)
            {
                case JoinRefused r: return "refused " + r.Reason;
                case PhaseStarted s: return "PhaseStarted(" + s.Phase + ")";
                case PhaseEnded p: return "PhaseEnded(" + p.Phase + ", " + p.Reason + ")";
                case InputAcked a: return "InputAcked(" + a.Ok + (a.Reason != null ? ", " + a.Reason : "") + ")";
                default: return e.GetType().Name;
            }
        }
    }
}
