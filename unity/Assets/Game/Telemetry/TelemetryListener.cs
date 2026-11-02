using System;
using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Telemetry
{
    /// <summary>
    /// Turns Core events into event log lines (docs/15_schemas.md, "Event log"), reading Game.State for details.
    /// A listener: it only reads, never changes state, and never throws into the game. Owns the writer.
    /// </summary>
    public sealed class TelemetryListener : IDisposable
    {
        public const string EventSessionStarted = "sessionStarted";
        public const string EventPlayerJoined = "playerJoined";
        public const string EventPlayerLeft = "playerLeft";
        public const string EventPlayerRejoined = "playerRejoined";
        public const string EventLobbyAction = "lobbyAction";
        public const string EventSettingsChanged = "settingsChanged";
        public const string EventGameStarted = "gameStarted";
        public const string EventPhaseStarted = "phaseStarted";
        public const string EventPhaseEnded = "phaseEnded";
        public const string EventInput = "input";
        public const string EventInputRefused = "inputRefused";
        public const string EventScoreChanged = "scoreChanged";
        public const string EventGameEnded = "gameEnded";
        public const string EventError = "error";

        /// <summary>playerLeft.reason when the phone went away and the seat is still held.</summary>
        public const string ReasonDisconnected = "disconnected";

        private readonly Game _game;
        private readonly EventLogWriter _writer;
        private readonly HashSet<string> _rejoinedInBatch = new HashSet<string>();

        public TelemetryListener(Game game, EventLogWriter writer)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public EventLogWriter Writer => _writer;

        /// <summary>The first I/O error of the log file, or null.</summary>
        public Exception FirstError => _writer.FirstError;

        /// <summary>Writes sessionStarted. The coordinator calls it once the room code is known.</summary>
        public void SessionStarted(string room)
        {
            Safe(() => _writer.Write(EventSessionStarted, new JObject
            {
                ["room"] = room,
                ["configVersion"] = _game.Config.ConfigVersion
            }));
        }

        /// <summary>Writes an error line, for example where "content" or "relay".</summary>
        public void Error(string where, string message)
        {
            Safe(() => _writer.Write(EventError, new JObject { ["where"] = where, ["message"] = message }));
        }

        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (events == null) return;
            _rejoinedInBatch.Clear();
            foreach (var e in events)
            {
                if (e is PlayerRejoined r) _rejoinedInBatch.Add(r.PlayerId);
            }

            foreach (var e in events) OnEvent(e);
            _rejoinedInBatch.Clear();
        }

        public void OnEvent(IGameEvent e)
        {
            Safe(() => Log(e));
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        private void Log(IGameEvent e)
        {
            var state = _game.State;
            switch (e)
            {
                case PlayerJoined joined:
                {
                    var fields = new JObject { ["playerId"] = joined.PlayerId };
                    var player = state.FindById(joined.PlayerId);
                    if (player != null)
                    {
                        fields["name"] = player.Name;
                        fields["lang"] = player.Lang;
                        fields["characterId"] = player.CharacterId;
                    }

                    _writer.Write(EventPlayerJoined, fields);
                    break;
                }
                case PlayerLeft left:
                    _writer.Write(EventPlayerLeft, new JObject { ["playerId"] = left.PlayerId, ["reason"] = left.Reason });
                    break;
                case PlayerConnectionChanged changed:
                    if (!changed.Connected)
                    {
                        _writer.Write(EventPlayerLeft, new JObject { ["playerId"] = changed.PlayerId, ["reason"] = ReasonDisconnected });
                    }
                    else if (!_rejoinedInBatch.Contains(changed.PlayerId))
                    {
                        // Back without a rejoin message: the relay reported the same socket again after a TV reconnect.
                        var player = state.FindById(changed.PlayerId);
                        long awayMs = player != null ? Math.Max(0, _game.NowMs - player.DisconnectedAtMs) : 0;
                        WriteRejoined(changed.PlayerId, awayMs);
                    }

                    break;
                case PlayerRejoined rejoined:
                    if (!rejoined.IsRefresh) WriteRejoined(rejoined.PlayerId, rejoined.AwayMs);
                    break;
                case LobbyActionHandled lobby:
                    _writer.Write(EventLobbyAction, new JObject
                    {
                        ["playerId"] = lobby.PlayerId,
                        ["action"] = lobby.Action,
                        ["ok"] = lobby.Ok
                    });
                    break;
                case SettingsChanged settings:
                {
                    var fields = new JObject { ["source"] = settings.Source };
                    if (settings.PlayerId != null) fields["playerId"] = settings.PlayerId;
                    fields["setting"] = settings.Setting;
                    fields["value"] = settings.Value;
                    _writer.Write(EventSettingsChanged, fields);
                    break;
                }
                case GameStarted started:
                    _writer.Write(EventGameStarted, new JObject
                    {
                        ["game"] = started.Game,
                        ["mode"] = started.Mode,
                        ["players"] = started.PlayerIds != null ? JArray.FromObject(started.PlayerIds) : new JArray(),
                        ["settings"] = new JObject { ["gameLang"] = started.GameLang }
                    }, started.Seed);
                    break;
                case PhaseStarted phase:
                    _writer.Write(EventPhaseStarted, PhaseStartedFields(phase));
                    break;
                case PhaseEnded ended:
                    _writer.Write(EventPhaseEnded, new JObject
                    {
                        ["phase"] = ended.Phase.ToString(),
                        ["phaseId"] = ended.PhaseId,
                        ["reason"] = ended.Reason,
                        ["durationMs"] = ended.DurationMs
                    });
                    break;
                case AnswerLocked locked:
                    _writer.Write(EventInput, new JObject
                    {
                        ["playerId"] = locked.PlayerId,
                        ["kind"] = locked.Kind,
                        ["phaseId"] = locked.PhaseId,
                        ["ms"] = locked.Ms,
                        ["correct"] = locked.Correct,
                        ["points"] = locked.Points
                    });
                    break;
                case InputRefused refused:
                    _writer.Write(EventInputRefused, new JObject
                    {
                        ["playerId"] = refused.PlayerId,
                        ["kind"] = refused.Kind,
                        ["phaseId"] = refused.PhaseId,
                        ["reason"] = refused.Reason
                    });
                    break;
                case ScoreChanged score:
                    _writer.Write(EventScoreChanged, new JObject
                    {
                        ["playerId"] = score.PlayerId,
                        ["delta"] = score.Delta,
                        ["score"] = score.Score,
                        ["rank"] = score.Rank
                    });
                    break;
                case GameEnded ended:
                    _writer.Write(EventGameEnded, new JObject
                    {
                        ["game"] = ended.Game,
                        ["reason"] = ended.Reason,
                        ["winner"] = ended.WinnerId,
                        ["durationMs"] = ended.DurationMs
                    });
                    break;
            }
        }

        /// <summary>phase, phaseId, then round, slot and questionId (1 based) in question phases, then the budget.</summary>
        private JObject PhaseStartedFields(PhaseStarted phase)
        {
            var fields = new JObject
            {
                ["phase"] = phase.Phase.ToString(),
                ["phaseId"] = phase.PhaseId
            };
            var question = _game.State.Question;
            if (question != null && IsQuestionPhase(phase.Phase))
            {
                fields["round"] = question.RoundIndex + 1;
                fields["slot"] = question.Slot + 1;
                fields["questionId"] = question.Def.Id;
            }

            fields["budgetMs"] = phase.BudgetMs;
            fields["stagingExtraMs"] = phase.StagingExtraMs;
            return fields;
        }

        private static bool IsQuestionPhase(GamePhase phase)
        {
            return phase == GamePhase.QuestionShow || phase == GamePhase.Answer || phase == GamePhase.Reveal || phase == GamePhase.Scores;
        }

        private void WriteRejoined(string playerId, long awayMs)
        {
            _writer.Write(EventPlayerRejoined, new JObject { ["playerId"] = playerId, ["awayMs"] = awayMs });
        }

        /// <summary>Telemetry must never break the game: anything that goes wrong here is dropped.</summary>
        private static void Safe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // The writer keeps its own I/O errors; a bad event detail only loses this line.
            }
        }
    }
}
