using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.DevTools;
using FamilyQuiz.Dialogue;
using FamilyQuiz.Net;
using FamilyQuiz.Presentation;
using FamilyQuiz.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace FamilyQuiz.Bootstrap
{
    /// <summary>
    /// The composition root: loads local.json and the content copy from StreamingAssets, builds Core, Net and
    /// Presentation, starts the relay connection and drains the message queue once per frame.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string ContentFolder = "content";

        private readonly MessageQueue _inbox = new MessageQueue();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        private const string PlaytestFolder = "playtests";

        private TvPresenter _view;
        private DebugOverlay _debug;
        private EventLogWriter _eventLog;
        private TelemetryListener _telemetry;
        private SeenFile _seen;
        private Game _game;
        private PausableClock _clock;
        private CryptoRandom _random;
        private RelayClient _relay;
        private RelayEndpoint _endpoint;
        private ProtocolRouter _router;
        private PhoneOutbox _outbox;
        private HostDialogue _dialogue;
        private string _loggedRoomCode;
        private bool _sessionLogged;
        private bool _relayUp;

        private void Awake()
        {
            // The TV app keeps running when the laptop window loses focus.
            Application.runInBackground = true;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }

        private void Start()
        {
            _view = TvPresenter.Create();
            _clock = new PausableClock(() => _stopwatch.ElapsedMilliseconds);
            _clock.Pause(); // the core clock runs only while the relay is connected
            _eventLog = CreateEventLog(_clock);

            GameContent content;
            try
            {
                content = LoadContent();
            }
            catch (Exception e) when (e is ContentException || e is IOException)
            {
                TvLog.Error("Content error: " + e.Message);
                _eventLog.Write(TelemetryListener.EventError, new JObject { ["where"] = "content", ["message"] = e.Message });
                var strings = TryLoadStrings();
                if (strings != null) _view.Configure(strings, null, Languages.Default);
                _view.ShowContentError(e.Message);
                return;
            }

            var config = content.Config;
            _random = new CryptoRandom();
            // Seen questions and race statements survive app restarts (docs/02_show_flow.md, "Seen questions").
            _seen = new SeenFile(Path.Combine(Application.persistentDataPath, SeenFile.FileName),
                message => TvLog.Error("Seen questions: " + message));
            var (seenQuestions, seenRace, seenFacts) = _seen.Load();
            TvLog.Info("Seen questions: " + seenQuestions.Count + ", race statements: " + seenRace.Count + ", facts: "
                       + seenFacts.Count + " (" + _seen.Path + ")");
            _game = new Game(content, _clock, _random, () => _random.Next(int.MaxValue), seenQuestions, seenRace, seenFacts);
            _seen.Attach(_game);
            _telemetry = new TelemetryListener(_game, _eventLog);
            // Host lines (docs/15_schemas.md, "Selection"): the bags survive across evenings in host_bags.json.
            var bags = new HostBagFile(Path.Combine(Application.persistentDataPath, HostBagFile.FileName),
                message => TvLog.Error("Host bags: " + message));
            _dialogue = new HostDialogue(content, _clock, seed => new SeededRandom(seed), bags, _random.Next(int.MaxValue));
            _view.Attach(_game); // before Start(): the presenter ignores events until it has a game
            // The TV settings panel sends commands like any other input (docs/13_architecture.md).
            _view.SetCommandSink(cmd => Dispatch(_game.Handle(cmd)));
            _debug = DebugOverlay.Create(_game);

            _endpoint = LoadEndpoint();
            if (_endpoint == null)
            {
                _view.ShowRelayDown();
                Dispatch(_game.Start());
                return;
            }

            _relay = new RelayClient(_endpoint, _inbox, _random, config.Net.TvReconnectSeconds, TvLog.Info);
            _router = new ProtocolRouter(_relay, _clock, TvLog.Info);
            _outbox = new PhoneOutbox(_game, _relay, () => _relay.Room.Code, TvLog.Info);
            Dispatch(_game.Start());
            _view.ShowRelayDown();
            TvLog.Info("Connecting to the relay at " + _endpoint.TvSocket(_relay.Room).GetLeftPart(UriPartial.Authority));
            _relay.Start();
        }

        private void Update()
        {
            if (_game == null) return;
            while (_inbox.TryDequeue(out var item))
            {
                try
                {
                    Handle(item);
                }
                catch (Exception e)
                {
                    // One bad message must never stop the others or the game.
                    TvLog.Error("Error while handling a relay message: " + e);
                }
            }

            Dispatch(_game.Tick());
            if (_dialogue != null) Speak(_dialogue.Tick());
        }

        private void OnDestroy()
        {
            _relay?.Dispose();
            _random?.Dispose();
            if (_telemetry != null) _telemetry.Dispose();
            else _eventLog?.Dispose();
        }

        /// <summary>The event log (docs/15_schemas.md): persistentDataPath/playtests/yyyy-mm-dd/events_session.jsonl.</summary>
        private static EventLogWriter CreateEventLog(IClock clock)
        {
            var now = DateTime.Now;
            var session = now.ToString("yyyy-MM-dd'T'HH-mm-ss");
            var path = Path.Combine(Application.persistentDataPath, PlaytestFolder, now.ToString("yyyy-MM-dd"),
                "events_" + session + ".jsonl");
            var writer = new EventLogWriter(path, session, Application.version, clock,
                message => TvLog.Error("Event log: " + message));
            TvLog.Info("Event log: " + path);
            return writer;
        }

        private void Handle(RelayInboxItem item)
        {
            switch (item.Kind)
            {
                case RelayInboxKind.SocketOpened:
                    _relayUp = true;
                    SyncClock();
                    if (item.RoomCode != _loggedRoomCode)
                    {
                        TvLog.Info("Room code: " + item.RoomCode);
                        _loggedRoomCode = item.RoomCode;
                    }
                    else
                    {
                        TvLog.Info("Relay connected again, room " + item.RoomCode);
                    }

                    if (!_sessionLogged)
                    {
                        _telemetry.SessionStarted(item.RoomCode);
                        _sessionLogged = true;
                    }

                    _view.ShowRoom(item.RoomCode, _endpoint.JoinUrl(item.RoomCode));
                    break;
                case RelayInboxKind.SocketClosed:
                    TvLog.Info("Relay socket closed" + (item.CloseCode.HasValue ? " (" + item.CloseCode + ")" : "")
                               + (string.IsNullOrEmpty(item.Detail) ? "" : ": " + item.Detail));
                    _view.ShowRelayDown();
                    Dispatch(_game.Handle(new RelayConnectionLost()));
                    _relayUp = false;
                    SyncClock();
                    break;
                case RelayInboxKind.Text:
                    var command = _router.ToCommand(item);
                    if (command != null) Dispatch(_game.Handle(command));
                    break;
            }
        }

        /// <summary>Hands the events to every listener. Listeners only read; they never change state.</summary>
        private void Dispatch(IReadOnlyList<IGameEvent> events)
        {
            if (events.Count == 0) return;
            SyncClock(); // a pause or resume in this batch (D105) stops or starts the clock before anyone reads it
            _seen?.OnEvents(events);
            _outbox?.OnEvents(events);
            _view.OnEvents(events);
            _debug?.OnEvents(events);
            _telemetry?.OnEvents(events);
            if (_dialogue != null) Speak(_dialogue.OnEvents(events));
            foreach (var e in events) LogEvent(e);
        }

        /// <summary>
        /// The core clock runs only while the relay is connected (docs/18_edge_cases.md) and the game is not paused
        /// (the pause menu, decisions/0018 D105).
        /// </summary>
        private void SyncClock()
        {
            if (_clock == null || _game == null) return;
            if (_relayUp && !_game.State.Paused) _clock.Resume();
            else _clock.Pause();
        }

        /// <summary>Lines the hosts start saying now: a bubble on the TV and a hostLine log line.</summary>
        private void Speak(IReadOnlyList<SpokenLine> lines)
        {
            foreach (var line in lines)
            {
                _view.ShowHostLine(line.Host, line.Text, line.EndMs);
                _telemetry?.HostLine(line.LineId, line.Host, line.Trigger);
                TvLog.Info("host " + line.Host + " (" + line.Trigger + "): " + line.Text(_game.State.GameLanguage));
            }
        }

        private static void LogEvent(IGameEvent e)
        {
            switch (e)
            {
                case PlayerJoined joined:
                    TvLog.Info("player joined " + joined.PlayerId + " conn=" + joined.Conn);
                    break;
                case PlayerRejoined rejoined:
                    TvLog.Info("player rejoined " + rejoined.PlayerId + " conn=" + rejoined.Conn
                               + (rejoined.IsRefresh ? " (refresh)" : " awayMs=" + rejoined.AwayMs));
                    break;
                case JoinRefused refused:
                    TvLog.Info("join refused conn=" + refused.Conn + " reason=" + refused.Reason);
                    break;
                case PlayerLeft left:
                    TvLog.Info("player left " + left.PlayerId + " reason=" + left.Reason);
                    break;
                case PlayerConnectionChanged changed:
                    TvLog.Info("player " + changed.PlayerId + (changed.Connected ? " connected" : " disconnected"));
                    break;
                case PhaseStarted phase:
                    TvLog.Info("phase " + phase.Phase + " started, phaseId=" + phase.PhaseId);
                    break;
            }
        }

        /// <summary>Loads and validates every content file; throws ContentException naming the file, item and field.</summary>
        private static GameContent LoadContent()
        {
            return new ContentLoader(ReadContent).Load();
        }

        /// <summary>The TV texts alone, so a content error can still show its title; null if they are broken too.</summary>
        private static UiStrings TryLoadStrings()
        {
            try
            {
                return UiStrings.Parse(ReadContent(UiStrings.FileName));
            }
            catch (ContentException)
            {
                return null;
            }
        }

        private static string ReadContent(string file)
        {
            var path = Path.Combine(Application.streamingAssetsPath, ContentFolder, file);
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                throw new ContentException(file, "cannot read " + path + " (" + e.Message + ")", e);
            }
        }

        private static RelayEndpoint LoadEndpoint()
        {
            var path = Path.Combine(Application.streamingAssetsPath, LocalSettings.FileName);
            try
            {
                var settings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(path));
                return RelayEndpoint.Parse(settings?.RelayHost);
            }
            catch (Exception e) when (e is IOException || e is JsonException || e is FormatException || e is UnauthorizedAccessException)
            {
                TvLog.Error("No relay: " + path + " is missing or has no valid relayHost (" + e.Message + ")");
                return null;
            }
        }
    }
}
