using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
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
        private bool _started;

        /// <param name="content">Loaded and validated content.</param>
        /// <param name="clock">The core clock.</param>
        /// <param name="tokenRandom">Random for seat tokens (not seeded in the app).</param>
        /// <param name="newSeed">Gives the seed for each new game; the game's IRandom is a SeededRandom of it, so a seed replays a game.</param>
        public Game(GameContent content, IClock clock, IRandom tokenRandom, Func<int> newSeed)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Config = content.Config ?? throw new ArgumentNullException(nameof(content.Config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (newSeed == null) throw new ArgumentNullException(nameof(newSeed));
            State = new GameState();
            _roster = new Roster(State, Config, content.Characters, clock, tokenRandom);
            _inputs = new InputRules(State, clock);
        }

        public GameState State { get; }

        public GameConfig Config { get; }

        public GameContent Content { get; }

        public long NowMs => _clock.NowMs;

        /// <summary>Start in the Lobby needs players.min seated players and no countdown running (PROTOCOL.md, "lobby actions").</summary>
        public bool CanStart => State.Phase == GamePhase.Lobby && State.Players.Count >= Config.Players.Min;

        /// <summary>Play again in PlayAgain needs players.min seated players (pollDone.canPlayAgain).</summary>
        public bool CanPlayAgain => State.Phase == GamePhase.PlayAgain && State.Players.Count >= Config.Players.Min;

        /// <summary>
        /// Milliseconds this player has left, for screen.endsInMs: until their personal deadline in Answer, until the
        /// phase end otherwise; null when the phase has no timer. Never negative.
        /// </summary>
        public long? EndsInMs(PlayerState player)
        {
            long? end = State.Phase == GamePhase.Answer && player?.Turn != null ? player.Turn.DeadlineMs : State.PhaseEndsAtMs;
            if (!end.HasValue) return null;
            return Math.Max(0, end.Value - _clock.NowMs);
        }

        /// <summary>
        /// True when at least one connected player can still send a game input right now (docs/15_schemas.md, "Idle"):
        /// in Answer, a connected player who has not locked in and is before their deadline plus net.inputGraceMs.
        /// </summary>
        public bool AnyoneCanAnswer
        {
            get
            {
                if (State.Phase != GamePhase.Answer) return false;
                var now = _clock.NowMs;
                foreach (var p in State.Players)
                {
                    if (p.Connected && p.Turn != null && !p.Turn.Locked && now < p.Turn.DeadlineMs + Config.Net.InputGraceMs) return true;
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
            State.Phase = GamePhase.Lobby;
            State.PhaseId++;
            State.PhaseStartedAtMs = _clock.NowMs;
            output.Add(new PhaseStarted(State.Phase, State.PhaseId));
            return output;
        }

        public IReadOnlyList<IGameEvent> Handle(IGameCommand command)
        {
            var output = new List<IGameEvent>();
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
                case ChangeSettings _: break; // Day 2: ShowDirector
            }

            return output;
        }

        /// <summary>Called once per frame: time based rules (phase ends, deadlines, the seat hold).</summary>
        public IReadOnlyList<IGameEvent> Tick()
        {
            var output = new List<IGameEvent>();
            _roster.ExpireSeats(output);
            return output;
        }
    }
}
