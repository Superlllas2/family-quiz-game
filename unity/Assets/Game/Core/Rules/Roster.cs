using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Seats: join, rejoin, leave, connections and the seat hold (PROTOCOL.md sections 3 to 5, docs/18_edge_cases.md).
    /// </summary>
    public sealed class Roster
    {
        private readonly GameState _state;
        private readonly GameConfig _config;
        private readonly IReadOnlyList<CharacterDef> _characters;
        private readonly IClock _clock;
        private readonly IRandom _tokenRandom;
        private int _nextPlayerNumber = 1;

        public Roster(GameState state, GameConfig config, IReadOnlyList<CharacterDef> characters, IClock clock, IRandom tokenRandom)
        {
            _state = state;
            _config = config;
            _characters = characters;
            _clock = clock;
            _tokenRandom = tokenRandom;
        }

        private long SeatHoldMs => (long)Math.Round(_config.Players.SeatHoldSeconds * 1000.0);

        public void Join(PlayerJoin cmd, List<IGameEvent> output)
        {
            _state.OpenConns.Add(cmd.Conn);

            // The same socket sending join again (for example a resend): answer with its seat again.
            var existing = _state.FindByConn(cmd.Conn);
            if (existing != null)
            {
                Reconnect(existing, cmd.Conn, output);
                output.Add(new PlayerRejoined(existing.PlayerId, cmd.Conn, 0, true));
                return;
            }

            if (_state.Phase != GamePhase.Lobby)
            {
                output.Add(new JoinRefused(cmd.Conn, RefusalReasons.GameStarted));
                return;
            }

            var name = NameRules.Normalize(cmd.Name);
            if (name == null)
            {
                output.Add(new JoinRefused(cmd.Conn, RefusalReasons.BadName));
                return;
            }

            foreach (var p in _state.Players)
            {
                if (NameRules.SameName(p.Name, name))
                {
                    output.Add(new JoinRefused(cmd.Conn, RefusalReasons.NameTaken));
                    return;
                }
            }

            var characterId = FirstFreeCharacter();
            if (_state.Players.Count >= _config.Players.Max || characterId < 0)
            {
                output.Add(new JoinRefused(cmd.Conn, RefusalReasons.RoomFull));
                return;
            }

            var lang = Languages.IsSupported(cmd.Lang) ? cmd.Lang : Languages.Default;
            var player = new PlayerState("p" + _nextPlayerNumber++, name, lang, characterId, NewToken(),
                _config.Players.TimerMultiplierOptions[0])
            {
                Conn = cmd.Conn,
                Connected = true
            };
            _state.MutablePlayers.Add(player);
            output.Add(new PlayerJoined(player.PlayerId, cmd.Conn));
            output.Add(new RosterChanged());
        }

        public void Rejoin(PlayerRejoin cmd, List<IGameEvent> output)
        {
            _state.OpenConns.Add(cmd.Conn);
            var player = _state.FindByToken(cmd.Token);
            if (player == null)
            {
                output.Add(new JoinRefused(cmd.Conn, RefusalReasons.BadToken));
                return;
            }

            bool isRefresh = player.Conn == cmd.Conn;
            if (!isRefresh && _state.IsOpen(player.Conn))
            {
                // Two sockets for one seat: the newer wins, the older is closed (PROTOCOL.md section 5, step 6).
                output.Add(new ConnectionSuperseded(player.PlayerId, player.Conn, cmd.Conn));
                _state.OpenConns.Remove(player.Conn);
            }

            long awayMs = player.Connected ? 0 : Math.Max(0, _clock.NowMs - player.DisconnectedAtMs);
            Reconnect(player, cmd.Conn, output);
            output.Add(new PlayerRejoined(player.PlayerId, cmd.Conn, awayMs, isRefresh));
        }

        public void Leave(PlayerLeave cmd, List<IGameEvent> output)
        {
            var player = _state.FindByConn(cmd.Conn);

            // Leaving without a seat (for example a resend after the seat was freed) is acked as done.
            output.Add(new InputAcked(cmd.Conn, cmd.InputId, true, null));
            if (player == null) return;
            Remove(player, PlayerLeft.ReasonLeft, output);
        }

        public void ConnectionOpened(ConnectionOpened cmd, List<IGameEvent> output)
        {
            _state.OpenConns.Add(cmd.Conn);
            var player = _state.FindByConn(cmd.Conn);
            if (player != null && !player.Connected)
            {
                player.Connected = true;
                output.Add(new PlayerConnectionChanged(player.PlayerId, true));
            }
        }

        public void ConnectionClosed(ConnectionClosed cmd, List<IGameEvent> output)
        {
            _state.OpenConns.Remove(cmd.Conn);

            // Only the seat's current socket counts; a superseded socket closing changes nothing.
            var player = _state.FindByConn(cmd.Conn);
            if (player != null && player.Connected) MarkDisconnected(player, output);
        }

        public void RelayConnectionLost(List<IGameEvent> output)
        {
            _state.OpenConns.Clear();
            foreach (var player in _state.Players)
            {
                if (player.Connected) MarkDisconnected(player, output);
            }
        }

        /// <summary>Frees every seat that has been disconnected for players.seatHoldSeconds.</summary>
        public void ExpireSeats(List<IGameEvent> output)
        {
            var now = _clock.NowMs;
            var holdMs = SeatHoldMs;
            var expired = _state.MutablePlayers.FindAll(p => !p.Connected && now - p.DisconnectedAtMs >= holdMs);
            foreach (var player in expired) Remove(player, PlayerLeft.ReasonSeatExpired, output);
        }

        private void Reconnect(PlayerState player, string conn, List<IGameEvent> output)
        {
            // A socket belongs to one seat only: if it held another seat before, that seat is now away.
            foreach (var other in _state.Players)
            {
                if (other != player && other.Conn == conn)
                {
                    other.Conn = null;
                    if (other.Connected) MarkDisconnected(other, output);
                }
            }

            bool wasConnected = player.Connected;
            player.Conn = conn;
            player.Connected = true;
            if (!wasConnected) output.Add(new PlayerConnectionChanged(player.PlayerId, true));
        }

        private void MarkDisconnected(PlayerState player, List<IGameEvent> output)
        {
            player.Connected = false;
            player.DisconnectedAtMs = _clock.NowMs;
            output.Add(new PlayerConnectionChanged(player.PlayerId, false));
        }

        private void Remove(PlayerState player, string reason, List<IGameEvent> output)
        {
            _state.MutablePlayers.Remove(player);
            output.Add(new PlayerLeft(player.PlayerId, player.Name, player.Conn, reason));
            output.Add(new RosterChanged());
        }

        private int FirstFreeCharacter()
        {
            foreach (var character in _characters)
            {
                bool taken = false;
                foreach (var p in _state.Players)
                {
                    if (p.CharacterId == character.Id)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken) return character.Id;
            }

            return -1;
        }

        private string NewToken()
        {
            while (true)
            {
                var token = TokenGenerator.Create(_tokenRandom);
                if (_state.FindByToken(token) == null) return token;
            }
        }
    }
}
