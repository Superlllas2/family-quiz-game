using System;
using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
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
                case AnswerLocked locked:
                    // Only the sender's screen changes (waiting); no other phone lists who is still answering (D88).
                    SendScreen(_game.State.FindById(locked.PlayerId));
                    break;
                case VoteCast vote:
                    SendScreen(_game.State.FindById(vote.PlayerId));
                    break;
                case LastCallStarted lastCall:
                    SendScreen(_game.State.FindById(lastCall.PlayerId));
                    break;
                case PowerUpUsed _:
                    // The attacker's screen turns to waiting, and every other holder's target list may grey a target.
                    SendScreenToEveryone();
                    break;
                case PowerUpApplied applied:
                    SendEffect(_game.State.FindById(applied.TargetId), applied.PowerUp, applied.AttackerName);
                    break;
                case PollSubmitted poll:
                    SendScreen(_game.State.FindById(poll.PlayerId));
                    break;
                case RosterChanged _:
                case PlayerConnectionChanged _:
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
            // A reload under a sabotage that was not cleared yet gets the overlay back.
            var turn = player.Turn;
            if (_game.State.Phase == GamePhase.Answer && turn != null && turn.Effect != null && !turn.EffectCleared && !turn.Locked)
                Send(conn, new EffectMessage { Effect = turn.Effect, From = turn.EffectFrom, Params = EffectParams(turn.Effect) });
        }

        /// <summary>The player list or ready marks changed: every lobby screen shows them (D76). Game screens list nobody (D88).</summary>
        private void RefreshAfterRosterChange()
        {
            // Game screens list nobody (D88), except the power up targets of the pick window.
            var phase = _game.State.Phase;
            if (phase == GamePhase.Lobby || phase == GamePhase.Reveal || phase == GamePhase.Scores) SendScreenToEveryone();
        }

        /// <summary>The "effect" message with its params from config (PROTOCOL.md section 4).</summary>
        private void SendEffect(PlayerState target, string effect, string from)
        {
            if (target == null || !IsReachable(target)) return;
            Send(target.Conn, new EffectMessage { Effect = effect, From = from, Params = EffectParams(effect) });
        }

        private object EffectParams(string effect)
        {
            var cfg = _game.Config.PowerUps;
            switch (effect)
            {
                case PowerUpTypes.Freeze: return new Dictionary<string, object> { { "taps", cfg.Freeze.Taps } };
                case PowerUpTypes.Slime: return new Dictionary<string, object> { { "blobs", cfg.Slime.Blobs } };
                case PowerUpTypes.Fog: return new Dictionary<string, object> { { "seconds", cfg.Fog.Seconds } };
                default: return new Dictionary<string, object>();
            }
        }

        private void SendScreenToEveryone()
        {
            foreach (var player in _game.State.Players) SendScreen(player);
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
