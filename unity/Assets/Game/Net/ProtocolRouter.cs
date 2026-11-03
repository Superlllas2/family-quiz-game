using System;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Net.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// Turns relay envelopes (PROTOCOL.md section 2) into Core commands. Answers ping and a wrong version
    /// itself, ignores unknown types, and never throws on bad input. Input kinds are passed on as sent: Core
    /// answers a kind it does not take with "invalid".
    /// </summary>
    public sealed class ProtocolRouter
    {
        private readonly IRelaySender _sender;
        private readonly IClock _clock;
        private readonly Action<string> _log;

        public ProtocolRouter(IRelaySender sender, IClock clock, Action<string> log)
        {
            _sender = sender;
            _clock = clock;
            _log = log ?? (_ => { });
        }

        /// <summary>The command for this envelope, or null when there is nothing for Core to do.</summary>
        public IGameCommand ToCommand(RelayInboxItem item)
        {
            try
            {
                var envelope = JObject.Parse(item.Text);
                var conn = Str(envelope["conn"]);
                if (conn == null) return null;
                switch (Str(envelope["relay"]))
                {
                    case "connected": return new ConnectionOpened(conn);
                    case "disconnected": return new ConnectionClosed(conn);
                    case "message": return FromMessage(conn, envelope["msg"] as JObject, item);
                    default: return null;
                }
            }
            catch (Exception e) when (e is JsonException || e is InvalidCastException || e is FormatException || e is OverflowException || e is ArgumentException)
            {
                _log("Dropped a relay message that is not valid: " + e.Message);
                return null;
            }
        }

        private IGameCommand FromMessage(string conn, JObject msg, RelayInboxItem item)
        {
            if (msg == null) return null;
            var v = Num(msg["v"]);
            if (v != ProtocolConstants.Version)
            {
                Send(conn, new RefusedMessage { Reason = RefusalReasons.BadVersion });
                return null;
            }

            var type = Str(msg["type"]);
            switch (type)
            {
                case "join":
                    return new PlayerJoin(conn, Str(msg["name"]), Str(msg["lang"]));
                case "rejoin":
                    return new PlayerRejoin(conn, Str(msg["token"]));
                case "leave":
                    return new PlayerLeave(conn, Str(msg["inputId"]));
                case "input":
                    var input = new SubmitAnswer(conn, Str(msg["inputId"]), Num(msg["phaseId"]) ?? -1, Str(msg["kind"]),
                        Int(msg["choice"]), Num(msg["ms"]), Int(msg["door"]), Dbl(msg["value"]), Bool(msg["answer"]),
                        Str(msg["playerId"]));
                    _log("input received conn=" + conn + " inputId=" + input.InputId + " kind=" + input.Kind
                         + " choice=" + input.Choice + " door=" + input.Door + " value=" + input.Value + " answer=" + input.Answer
                         + " ms=" + input.Ms + " phaseId=" + input.PhaseId
                         + " (socket at " + item.ReceivedAt.ToString("HH:mm:ss.fff") + ")");
                    return input;
                case "lobby":
                    return new LobbyAction(conn, Str(msg["inputId"]), Str(msg["action"]));
                case "character":
                    return new PickCharacter(conn, Str(msg["inputId"]), Int(msg["characterId"]) ?? -1);
                case "settings":
                    return new ChangeSettings(conn, Str(msg["lang"]), Dbl(msg["timerMultiplier"]));
                case "ping":
                    Send(conn, new PongMessage { T = msg["t"]?.DeepClone(), Tv = _clock.NowMs });
                    return null;
                default:
                    // poll (Day 4) and unknown types are ignored.
                    return null;
            }
        }

        private void Send(string conn, object msg)
        {
            _sender.Send(ProtocolJson.Serialize(new OutboundEnvelope { To = conn, Msg = msg }));
        }

        private static string Str(JToken token)
        {
            return token != null && token.Type == JTokenType.String ? (string)token : null;
        }

        private static long? Num(JToken token)
        {
            if (token == null) return null;
            if (token.Type == JTokenType.Integer) return (long)token;
            if (token.Type == JTokenType.Float)
            {
                var d = (double)token;
                if (double.IsNaN(d) || double.IsInfinity(d) || Math.Abs(d) > long.MaxValue / 2.0) return null;
                return (long)Math.Round(d);
            }

            return null;
        }

        /// <summary>A whole number that fits an int, or null.</summary>
        private static int? Int(JToken token)
        {
            var n = Num(token);
            if (!n.HasValue || n.Value < int.MinValue || n.Value > int.MaxValue) return null;
            return (int)n.Value;
        }

        /// <summary>A JSON true or false, or null.</summary>
        private static bool? Bool(JToken token)
        {
            return token != null && token.Type == JTokenType.Boolean ? (bool)token : (bool?)null;
        }

        /// <summary>A finite number as a double, not rounded (timerMultiplier 1.5 stays 1.5), or null.</summary>
        private static double? Dbl(JToken token)
        {
            if (token == null) return null;
            if (token.Type != JTokenType.Integer && token.Type != JTokenType.Float) return null;
            var d = (double)token;
            if (double.IsNaN(d) || double.IsInfinity(d)) return null;
            return d;
        }
    }
}
