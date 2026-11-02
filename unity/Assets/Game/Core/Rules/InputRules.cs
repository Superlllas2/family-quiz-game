using System.Collections.Generic;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Inputs that get an ack: input, lobby and character (PROTOCOL.md sections 3 and 4).
    /// A resent inputId gets the same ack again and is never applied twice; a first ok ack comes back
    /// as ok with reason "duplicate". Day 1 has no phases with answer windows yet, so there are no
    /// stale or closed checks; lobby and character are answered "invalid" until Day 2.
    /// </summary>
    public sealed class InputRules
    {
        private readonly GameState _state;
        private readonly IClock _clock;

        public InputRules(GameState state, IClock clock)
        {
            _state = state;
            _clock = clock;
        }

        public void Submit(SubmitAnswer cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;

            bool valid = cmd.Kind == InputKinds.Choice && cmd.Choice.HasValue
                         && cmd.Choice.Value >= 0 && cmd.Choice.Value < ProtocolLimits.ChoiceCount;
            if (!valid)
            {
                Answer(player, cmd.Conn, cmd.InputId, false, AckReasons.Invalid, output);
                return;
            }

            var record = new AnswerRecord(cmd.InputId, cmd.PhaseId, cmd.Kind, cmd.Choice, cmd.Ms, _clock.NowMs);
            player.LastAnswer = record;
            Answer(player, cmd.Conn, cmd.InputId, true, null, output);
            output.Add(new AnswerReceived(player.PlayerId, record));
        }

        public void Lobby(LobbyAction cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;
            Answer(player, cmd.Conn, cmd.InputId, false, AckReasons.Invalid, output);
        }

        public void Character(PickCharacter cmd, List<IGameEvent> output)
        {
            var player = SeatOf(cmd.Conn, cmd.InputId, output);
            if (player == null || Resent(player, cmd.Conn, cmd.InputId, output)) return;
            Answer(player, cmd.Conn, cmd.InputId, false, AckReasons.Invalid, output);
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

        private static void Answer(PlayerState player, string conn, string inputId, bool ok, string reason, List<IGameEvent> output)
        {
            player.RememberAck(inputId, new AckRecord(ok, reason));
            output.Add(new InputAcked(conn, inputId, ok, reason));
        }
    }
}
