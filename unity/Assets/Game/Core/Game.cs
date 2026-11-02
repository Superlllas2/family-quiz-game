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
    /// </summary>
    public sealed class Game
    {
        private readonly IClock _clock;
        private readonly Roster _roster;
        private readonly InputRules _inputs;
        private bool _started;

        public Game(GameConfig config, IReadOnlyList<CharacterDef> characters, IClock clock, IRandom tokenRandom)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            State = new GameState();
            _roster = new Roster(State, config, characters, clock, tokenRandom);
            _inputs = new InputRules(State, clock);
        }

        public GameState State { get; }

        public GameConfig Config { get; }

        public long NowMs => _clock.NowMs;

        /// <summary>Start in the Lobby needs players.min seated players (PROTOCOL.md, "lobby actions").</summary>
        public bool CanStart => State.Players.Count >= Config.Players.Min;

        /// <summary>Opens the Lobby phase.</summary>
        public IReadOnlyList<IGameEvent> Start()
        {
            var output = new List<IGameEvent>();
            if (_started) return output;
            _started = true;
            State.Phase = GamePhase.Lobby;
            State.PhaseId++;
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
            }

            return output;
        }

        /// <summary>Called once per frame: time based rules (the seat hold).</summary>
        public IReadOnlyList<IGameEvent> Tick()
        {
            var output = new List<IGameEvent>();
            _roster.ExpireSeats(output);
            return output;
        }
    }
}
