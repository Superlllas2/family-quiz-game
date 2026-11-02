using System;
using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Net.Protocol;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// Listens to Core events and sends the phones what they need: welcome, snapshot, screen, player, refused, ack,
    /// and the close control message for a superseded socket. Only reads the state.
    /// </summary>
    public sealed class PhoneOutbox
    {
        /// <summary>SettingsChanged.Setting for the phone language (docs/15_schemas.md, settingsChanged).</summary>
        public const string LangSetting = "lang";

        private readonly Game _game;
        private readonly IRelaySender _sender;
        private readonly ScreenBuilder _screens;
        private readonly Func<string> _roomCode;
        private readonly Action<string> _log;

        public PhoneOutbox(Game game, IRelaySender sender, Func<string> roomCode, Action<string> log)
        {
            _game = game;
            _sender = sender;
            _screens = new ScreenBuilder(game);
            _roomCode = roomCode;
            _log = log ?? (_ => { });
        }

        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            foreach (var e in events) OnEvent(e);
        }

        public void OnEvent(IGameEvent e)
        {
            switch (e)
            {
                case PlayerJoined joined:
                    Welcome(joined.PlayerId, joined.Conn);
                    break;
                case PlayerRejoined rejoined:
                    Welcome(rejoined.PlayerId, rejoined.Conn);
                    break;
                case JoinRefused refused:
                    Send(refused.Conn, new RefusedMessage { Reason = refused.Reason });
                    break;
                case ConnectionSuperseded superseded:
                    _sender.Send(ProtocolJson.Serialize(new CloseControl
                    {
                        Close = superseded.OldConn,
                        Code = ProtocolConstants.CloseReplacedByNewerSocket
                    }));
                    _log("closed older socket " + superseded.OldConn + " of " + superseded.PlayerId + " (4002)");
                    break;
                case InputAcked ack:
                    Send(ack.Conn, new AckMessage { InputId = ack.InputId, Ok = ack.Ok, Reason = ack.Reason });
                    _log("ack sent conn=" + ack.Conn + " inputId=" + ack.InputId + " ok=" + ack.Ok
                         + (ack.Reason != null ? " reason=" + ack.Reason : ""));
                    if (!ack.Ok && ack.Reason == AckReasons.CharacterTaken) SendScreen(_game.State.FindByConn(ack.Conn));
                    break;
                case PhaseStarted _:
                    SendScreenToEveryone();
                    break;
                case AnswerLocked _:
                    // The locker now waits; every other locked player's pending list just lost a name.
                    SendScreenToLockedPlayers();
                    break;
                case LastCallStarted lastCall:
                    SendScreen(_game.State.FindById(lastCall.PlayerId));
                    break;
                case RosterChanged _:
                case PlayerConnectionChanged _:
                case PlayerTimedOut _: // a player past deadline plus grace drops out of the pending lists
                case ReadyChanged _: // every lobby screen shows everyone's ready mark (D76)
                    RefreshAfterRosterChange();
                    break;
                case PlayerUpdated updated:
                    SendPlayer(_game.State.FindById(updated.PlayerId));
                    break;
                case SettingsChanged settings:
                    if (settings.Setting == LangSetting && settings.PlayerId != null)
                        SendScreen(_game.State.FindById(settings.PlayerId));
                    break;
            }
        }

        private void Welcome(string playerId, string conn)
        {
            var player = _game.State.FindById(playerId);
            if (player == null) return;
            Send(conn, new WelcomeMessage { PlayerId = player.PlayerId, Token = player.Token, Room = _roomCode() });
            Send(conn, new SnapshotMessage { Player = _screens.Player(player), Screen = _screens.CurrentScreen(player) });
        }

        /// <summary>The pending lists changed (Answer), or the player list or ready marks did (Lobby).</summary>
        private void RefreshAfterRosterChange()
        {
            switch (_game.State.Phase)
            {
                case GamePhase.Answer:
                    SendScreenToLockedPlayers();
                    break;
                case GamePhase.Lobby:
                    SendScreenToEveryone();
                    break;
            }
        }

        private void SendScreenToEveryone()
        {
            foreach (var player in _game.State.Players) SendScreen(player);
        }

        private void SendScreenToLockedPlayers()
        {
            if (_game.State.Phase != GamePhase.Answer) return;
            foreach (var player in _game.State.Players)
            {
                if (player.Turn != null && player.Turn.Locked) SendScreen(player);
            }
        }

        private void SendScreen(PlayerState player)
        {
            if (player != null && IsReachable(player)) Send(player.Conn, new ScreenMessage(_screens.CurrentScreen(player)));
        }

        private void SendPlayer(PlayerState player)
        {
            if (player != null && IsReachable(player)) Send(player.Conn, new PlayerMessage { Player = _screens.Player(player) });
        }

        private static bool IsReachable(PlayerState player)
        {
            return player.Connected && player.Conn != null;
        }

        private void Send(string conn, object msg)
        {
            if (conn == null) return;
            _sender.Send(ProtocolJson.Serialize(new OutboundEnvelope { To = conn, Msg = msg }));
        }
    }
}
