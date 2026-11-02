using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Net;
using FamilyQuiz.Presentation;
using Newtonsoft.Json;
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

        private LobbyView _view;
        private Game _game;
        private PausableClock _clock;
        private CryptoRandom _random;
        private RelayClient _relay;
        private RelayEndpoint _endpoint;
        private ProtocolRouter _router;
        private PhoneOutbox _outbox;
        private string _loggedRoomCode;

        private void Awake()
        {
            // The TV app keeps running when the laptop window loses focus.
            Application.runInBackground = true;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }

        private void Start()
        {
            _view = LobbyView.Create();

            GameConfig config;
            IReadOnlyList<CharacterDef> characters;
            UiStrings strings = null;
            try
            {
                strings = UiStrings.Parse(ReadContent(UiStrings.FileName));
                config = ConfigLoader.Parse(ReadContent(ConfigLoader.FileName));
                characters = CharacterLoader.Parse(ReadContent(CharacterLoader.FileName), config);
            }
            catch (Exception e) when (e is ContentException || e is IOException)
            {
                TvLog.Error("Content error: " + e.Message);
                if (strings != null) _view.Configure(strings, null, Languages.Default);
                _view.ShowContentError(e.Message);
                return;
            }

            _clock = new PausableClock(() => _stopwatch.ElapsedMilliseconds);
            _clock.Pause(); // the core clock runs only while the relay is connected
            _random = new CryptoRandom();
            _game = new Game(config, characters, _clock, _random);
            _view.Configure(strings, characters, _game.State.GameLanguage);

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
        }

        private void OnDestroy()
        {
            _relay?.Dispose();
            _random?.Dispose();
        }

        private void Handle(RelayInboxItem item)
        {
            switch (item.Kind)
            {
                case RelayInboxKind.SocketOpened:
                    _clock.Resume();
                    if (item.RoomCode != _loggedRoomCode)
                    {
                        TvLog.Info("Room code: " + item.RoomCode);
                        _loggedRoomCode = item.RoomCode;
                    }
                    else
                    {
                        TvLog.Info("Relay connected again, room " + item.RoomCode);
                    }

                    _view.ShowRoom(item.RoomCode, _endpoint.JoinUrl(item.RoomCode));
                    break;
                case RelayInboxKind.SocketClosed:
                    TvLog.Info("Relay socket closed" + (item.CloseCode.HasValue ? " (" + item.CloseCode + ")" : "")
                               + (string.IsNullOrEmpty(item.Detail) ? "" : ": " + item.Detail));
                    _view.ShowRelayDown();
                    Dispatch(_game.Handle(new RelayConnectionLost()));
                    _clock.Pause();
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
            _outbox?.OnEvents(events);
            bool refresh = false;
            foreach (var e in events)
            {
                LogEvent(e);
                if (e is RosterChanged || e is PlayerConnectionChanged || e is AnswerReceived) refresh = true;
            }

            if (refresh) _view.Refresh(_game.State);
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
