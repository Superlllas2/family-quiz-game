using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Show;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Inputs that get an ack: input, lobby and character (PROTOCOL.md sections 3 and 4).
    /// A resent inputId gets the same ack again and is never applied twice; a first ok ack comes back
    /// as ok with reason "duplicate". A refused input from a seated player also raises InputRefused.
    /// What is valid when is decided by the ShowDirector (answers, lobby actions) and here (character picks).
    /// </summary>
    public sealed class InputRules
    {
        private readonly GameState _state;
        private readonly ShowDirector _director;
        private readonly IReadOnlyList<CharacterDef> _characters;

        public InputRules(GameState state, ShowDirector director, IReadOnlyList<CharacterDef> characters)
        {
            _state = state;
            _director = director;
            _characters = characters;
        }

        public void Submit(SubmitAnswer cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;

            var reason = _director.CheckChoice(player, cmd);
            if (reason != null)
            {
                Refuse(player, cmd.Conn, cmd.InputId, cmd.Kind, cmd.PhaseId, reason, output);
                return;
            }

            Answer(player, cmd.Conn, cmd.InputId, true, null, output);
            _director.LockChoice(player, cmd, output);
        }

        public void Lobby(LobbyAction cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;

            bool ok = _director.CanLobbyAction(cmd.Action);
            Answer(player, cmd.Conn, cmd.InputId, ok, ok ? null : AckReasons.Invalid, output);
            output.Add(new LobbyActionHandled(player.PlayerId, cmd.Action, ok));
            if (ok) _director.ApplyLobbyAction(cmd.Action, output);
        }

        /// <summary>
        /// A character pick: only in the Lobby and PlayAgain, for an id in characters.json (else invalid), and not one
        /// another seated player has (characterTaken, docs/18_edge_cases.md).
        /// </summary>
        public void Character(PickCharacter cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;

            string reason = null;
            if (_state.Phase != GamePhase.Lobby && _state.Phase != GamePhase.PlayAgain) reason = AckReasons.Invalid;
            else if (!CharacterExists(cmd.CharacterId)) reason = AckReasons.Invalid;
            else if (TakenByOther(player, cmd.CharacterId)) reason = AckReasons.CharacterTaken;
            if (reason != null)
            {
                Refuse(player, cmd.Conn, cmd.InputId, InputRefused.KindCharacter, _state.PhaseId, reason, output);
                return;
            }

            Answer(player, cmd.Conn, cmd.InputId, true, null, output);
            player.CharacterId = cmd.CharacterId;
            player.CharacterPicked = true;
            output.Add(new PlayerUpdated(player.PlayerId));
            output.Add(new RosterChanged());
        }

        private bool CharacterExists(int id)
        {
            if (_characters == null) return false;
            foreach (var c in _characters)
            {
                if (c.Id == id) return true;
            }

            return false;
        }

        private bool TakenByOther(PlayerState player, int id)
        {
            foreach (var p in _state.Players)
            {
                if (p != player && p.CharacterId == id) return true;
            }

            return false;
        }

        private PlayerState SeatOf(string conn, string inputId, List<IGameEvent> output)
        {
            var player = _state.FindByConn(conn);
            if (player == null || string.IsNullOrEmpty(inputId))
            {
                output.Add(new InputAcked(conn, inputId, false, AckReasons.Invalid));
                return null;
            }

            return player;
        }

        private static bool Resent(PlayerState player, string conn, string inputId, List<IGameEvent> output)
        {
            if (!player.TryGetAck(inputId, out var first)) return false;
            output.Add(new InputAcked(conn, inputId, first.Ok, first.Ok ? AckReasons.Duplicate : first.Reason));
            return true;
        }

        private static void Refuse(PlayerState player, string conn, string inputId, string kind, long phaseId, string reason,
            List<IGameEvent> output)
        {
            Answer(player, conn, inputId, false, reason, output);
            output.Add(new InputRefused(player.PlayerId, kind, phaseId, reason));
        }

        private static void Answer(PlayerState player, string conn, string inputId, bool ok, string reason, List<IGameEvent> output)
        {
            player.RememberAck(inputId, new AckRecord(ok, reason));
            output.Add(new InputAcked(conn, inputId, ok, reason));
        }
    }
}
