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
        public const string EventRaceStarted = "raceStarted";
        public const string EventError = "error";
        public const string EventPowerUpDrawn = "powerUpDrawn";
        public const string EventPowerUpUsed = "powerUpUsed";
        public const string EventEffectCleared = "effectCleared";
        public const string EventHostLine = "hostLine";
        public const string EventPoll = "poll";
        public const string EventQuestionRated = "questionRated";
        public const string EventGamePaused = "gamePaused";
        public const string EventGameResumed = "gameResumed";

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

        /// <summary>Writes a hostLine line for a line a host said (Dialogue reports it through the composition root).</summary>
        public void HostLine(string lineId, string host, string trigger)
        {
            Safe(() => _writer.Write(EventHostLine, new JObject { ["lineId"] = lineId, ["host"] = host, ["trigger"] = trigger }));
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

        /// <summary>Private: a rejoin is told apart from a socket coming back by looking at the whole batch (OnEvents).</summary>
        private void OnEvent(IGameEvent e)
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
                        ["settings"] = new JObject
                        {
                            ["gameLang"] = started.GameLang,
                            ["powerUps"] = JArray.FromObject(started.PowerUps),
                            ["targeting"] = started.Targeting,
                            ["bank"] = started.Bank
                        }
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
                {
                    var fields = new JObject
                    {
                        ["playerId"] = locked.PlayerId,
                        ["kind"] = locked.Kind,
                        ["phaseId"] = locked.PhaseId
                    };
                    if (locked.Value.HasValue) fields["value"] = locked.Value.Value;
                    if (locked.TrueFalse.HasValue) fields["answer"] = locked.TrueFalse.Value;
                    fields["ms"] = locked.Ms;
                    fields["correct"] = locked.Correct;
                    fields["points"] = locked.Points;
                    _writer.Write(EventInput, fields);
                    break;
                }
                case VoteCast vote:
                    _writer.Write(EventInput, new JObject
                    {
                        ["playerId"] = vote.PlayerId,
                        ["kind"] = "vote",
                        ["phaseId"] = vote.PhaseId,
                        ["door"] = vote.Door,
                        ["topicId"] = state.Round != null && vote.Door < state.Round.Doors.Count ? state.Round.Doors[vote.Door].Id : null
                    });
                    break;
                case RaceStarted race:
                {
                    var heads = new JObject();
                    foreach (var pair in race.HeadStarts) heads[pair.Key] = pair.Value;
                    _writer.Write(EventRaceStarted, new JObject { ["headStarts"] = heads, ["lastPlaceCanWin"] = race.LastPlaceCanWin });
                    break;
                }
                case QuestionFallback fallback:
                    // docs/02_show_flow.md: a warning whenever the chosen topic had no unseen question for a slot.
                    _writer.Write(EventError, new JObject
                    {
                        ["where"] = "content",
                        ["message"] = "round " + fallback.Round + " slot " + fallback.Slot + ": no unseen question in topic "
                                      + fallback.TopicId + ", used " + fallback.QuestionId + " from group " + fallback.Group
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
                case PowerUpDrawn drawn:
                    _writer.Write(EventPowerUpDrawn, new JObject
                    {
                        ["playerId"] = drawn.PlayerId,
                        ["powerUp"] = drawn.PowerUp,
                        ["rank"] = drawn.Rank,
                        ["chance"] = Math.Round(drawn.Chance, 4)
                    });
                    break;
                case PowerUpUsed used:
                    _writer.Write(EventPowerUpUsed, new JObject
                    {
                        ["playerId"] = used.PlayerId,
                        ["powerUp"] = used.PowerUp,
                        ["target"] = used.TargetId,
                        ["phaseId"] = used.PhaseId
                    });
                    break;
                case EffectCleared cleared:
                    _writer.Write(EventEffectCleared, new JObject
                    {
                        ["playerId"] = cleared.PlayerId,
                        ["effect"] = cleared.Effect,
                        ["ms"] = cleared.Ms
                    });
                    break;
                case PollSubmitted poll:
                {
                    // Anonymous: never the player id (docs/15_schemas.md).
                    var answers = new JObject();
                    foreach (var pair in poll.Answers) answers[pair.Key] = JToken.FromObject(pair.Value);
                    _writer.Write(EventPoll, new JObject { ["answers"] = answers });
                    break;
                }
                case QuestionRated rated:
                {
                    // One line per accepted tap (decisions/0018, D111); the latest per player and item counts.
                    var fields = new JObject
                    {
                        ["playerId"] = rated.PlayerId,
                        ["questionId"] = rated.QuestionId,
                        ["topic"] = rated.Topic,
                        ["format"] = rated.Format,
                        ["difficulty"] = rated.Difficulty,
                        ["item"] = rated.Item,
                        ["stars"] = rated.Stars,
                        ["phaseId"] = rated.PhaseId
                    };
                    if (rated.Bank != null) fields["bank"] = rated.Bank;
                    fields["correct"] = rated.Correct.HasValue ? (JToken)rated.Correct.Value : JValue.CreateNull();
                    _writer.Write(EventQuestionRated, fields);
                    break;
                }
                case GamePaused paused:
                    _writer.Write(EventGamePaused, new JObject { ["phase"] = paused.Phase.ToString(), ["phaseId"] = paused.PhaseId });
                    break;
                case GameResumed resumed:
                    _writer.Write(EventGameResumed, new JObject { ["phase"] = resumed.Phase.ToString(), ["phaseId"] = resumed.PhaseId });
                    break;
                case GameEnded ended:
                {
                    var fields = new JObject
                    {
                        ["game"] = ended.Game,
                        ["reason"] = ended.Reason,
                        ["winner"] = ended.WinnerId,
                        ["durationMs"] = ended.DurationMs
                    };
                    if (ended.Reason == GameEnded.ReasonFinished)
                    {
                        // The Results rewards (D86, D87): the winner's fact and every other player's tag.
                        fields["factId"] = state.WinnerFact?.Id;
                        var tags = new JObject();
                        foreach (var pair in state.TagsByPlayer) tags[pair.Key] = pair.Value;
                        fields["tags"] = tags;
                    }

                    _writer.Write(EventGameEnded, fields);
                    break;
                }
            }
        }

        /// <summary>
        /// phase, phaseId, then round, slot and questionId (1 based) in question phases, round in TopicVote and RoundEnd,
        /// the statement id as questionId in the race, then the budget.
        /// </summary>
        private JObject PhaseStartedFields(PhaseStarted phase)
        {
            var fields = new JObject
            {
                ["phase"] = phase.Phase.ToString(),
                ["phaseId"] = phase.PhaseId
            };
            var state = _game.State;
            var question = state.Question;
            if (question != null && IsQuestionPhase(phase.Phase))
            {
                fields["round"] = question.RoundIndex + 1;
                fields["slot"] = question.Slot + 1;
                fields["questionId"] = question.Def.Id;
            }
            else if (state.Round != null && (phase.Phase == GamePhase.TopicVote || phase.Phase == GamePhase.RoundEnd))
            {
                fields["round"] = state.Round.Index + 1;
            }
            else if (state.Race?.Statement != null && (phase.Phase == GamePhase.RaceQuestion || phase.Phase == GamePhase.RaceReveal))
            {
                fields["questionId"] = state.Race.Statement.Id;
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
