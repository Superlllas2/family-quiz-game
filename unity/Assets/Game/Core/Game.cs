using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.Show;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core
{
    /// <summary>
    /// The core: takes commands, changes the state, returns events. Runs on the injected clock.
    /// This is the only entry point the composition root and the listeners use; the show itself runs in ShowDirector.
    /// </summary>
    public sealed class Game
    {
        private readonly IClock _clock;
        private readonly Roster _roster;
        private readonly InputRules _inputs;
        private readonly SettingsRules _settings;
        private readonly ShowDirector _director;
        private bool _started;

        /// <param name="content">Loaded and validated content.</param>
        /// <param name="clock">The core clock.</param>
        /// <param name="tokenRandom">Random for seat tokens (not seeded in the app).</param>
        /// <param name="newSeed">Gives the seed for each new game; the game's IRandom is a SeededRandom of it, so a seed replays a game.</param>
        /// <param name="seenQuestions">Question ids seen in earlier sessions (docs/02_show_flow.md, "Seen questions"), or null.</param>
        /// <param name="seenRace">Race statement ids seen in earlier sessions, or null.</param>
        /// <param name="seenFacts">Winner fact ids given in earlier sessions, or null.</param>
        public Game(GameContent content, IClock clock, IRandom tokenRandom, Func<int> newSeed,
            IEnumerable<string> seenQuestions = null, IEnumerable<string> seenRace = null,
            IEnumerable<string> seenFacts = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Config = content.Config ?? throw new ArgumentNullException(nameof(content.Config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (newSeed == null) throw new ArgumentNullException(nameof(newSeed));
            State = new GameState();
            _roster = new Roster(State, Config, content.Characters, clock, tokenRandom);
            _director = new ShowDirector(State, content, clock, newSeed);
            _director.LoadSeen(seenQuestions, seenRace, seenFacts);
            _inputs = new InputRules(State, _director, content.Characters);
            _settings = new SettingsRules(State, Config);
        }

        public GameState State { get; }

        public GameConfig Config { get; }

        public GameContent Content { get; }

        public long NowMs => _clock.NowMs;

        /// <summary>Connected seated players: the count the ready start uses (D76).</summary>
        public int ConnectedPlayers => State.ConnectedPlayerCount();

        /// <summary>players.min or more players are connected (lobby.enoughPlayers, D76).</summary>
        public bool EnoughPlayers => ConnectedPlayers >= Config.Players.Min;

        /// <summary>
        /// Milliseconds this player has left, for screen.endsInMs: until their personal deadline in Answer, until the
        /// phase end otherwise; null when the phase has no timer. Never negative.
        /// </summary>
        public long? EndsInMs(PlayerState player)
        {
            bool turnPhase = State.Phase == GamePhase.Answer || State.Phase == GamePhase.RaceQuestion;
            long? end = turnPhase && player?.Turn != null ? player.Turn.DeadlineMs
                : State.Phase == GamePhase.TopicVote && State.Round != null ? State.Round.VoteDeadlineMs
                : State.PhaseEndsAtMs;
            if (!end.HasValue) return null;
            return Math.Max(0, end.Value - _clock.NowMs);
        }

        /// <summary>
        /// True when at least one connected player can still send a game input right now (docs/15_schemas.md, "Idle"):
        /// a vote in TopicVote from a connected player who has not sent one; in Answer and
        /// RaceQuestion, a connected player who has not locked in and is before their deadline plus net.inputGraceMs.
        /// </summary>
        public bool AnyoneCanAnswer
        {
            get
            {
                var now = _clock.NowMs;
                foreach (var p in State.Players)
                {
                    if (!p.Connected) continue;
                    switch (State.Phase)
                    {
                        case GamePhase.TopicVote:
                            if (State.Round != null && !State.Round.Votes.ContainsKey(p.PlayerId)) return true;
                            break;
                        case GamePhase.Answer:
                        case GamePhase.RaceQuestion:
                            if (p.Turn != null && !p.Turn.Locked && now < p.Turn.DeadlineMs + Config.Net.InputGraceMs) return true;
                            break;
                    }
                }

                return false;
            }
        }

        /// <summary>Opens the Lobby phase.</summary>
        public IReadOnlyList<IGameEvent> Start()
        {
            var output = new List<IGameEvent>();
            if (_started) return output;
            _started = true;
            _director.OpenLobby(output);
            return output;
        }

        /// <summary>
        /// Applies one command. Phases already due by the clock end first; after the command the end checks run at once
        /// (a countdown left with too few players, the Answer end and last call), so a phase ends on the same call as
        /// the last lock in, leave or disconnect.
        /// </summary>
        public IReadOnlyList<IGameEvent> Handle(IGameCommand command)
        {
            var output = new List<IGameEvent>();
            if (_started) _director.Advance(output);
            switch (command)
            {
                case PlayerJoin c: _roster.Join(c, output); break;
                case PlayerRejoin c: _roster.Rejoin(c, output); break;
                case PlayerLeave c: _roster.Leave(c, output); break;
                case ConnectionOpened c: _roster.ConnectionOpened(c, output); break;
                case ConnectionClosed c: _roster.ConnectionClosed(c, output); break;
                case RelayConnectionLost _: _roster.RelayConnectionLost(output); break;
                case SubmitAnswer c: _inputs.Submit(c, output); break;
                case LobbyAction c: _inputs.Lobby(c, output); break;
                case PickCharacter c: _inputs.Character(c, output); break;
                case ChangeSettings c: _settings.Change(c, output); break;
                case ResetSeen _: _director.ResetSeen(output); break;
            }

            if (_started) _director.Reevaluate(output);
            return output;
        }

        /// <summary>
        /// Called once per frame: time based rules. Every phase that is due ends at its scheduled time (looping, so a
        /// late frame catches up exactly), then the seat hold, then the end checks after a freed seat.
        /// </summary>
        public IReadOnlyList<IGameEvent> Tick()
        {
            var output = new List<IGameEvent>();
            if (_started) _director.Advance(output);
            _roster.ExpireSeats(output);
            if (_started) _director.Reevaluate(output);
            return output;
        }
    }
}
