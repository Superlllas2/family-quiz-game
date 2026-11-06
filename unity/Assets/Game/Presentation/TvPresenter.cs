using System;
using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation.Staging;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The TV's root view: one screen space overlay canvas (1920 x 1080 reference) with one plain view per phase,
    /// switched on PhaseStarted, the hosts' text bubbles above the views and the TV settings panel (Tab) above
    /// everything. A listener only: it reads Game and GameState and the events it is handed, and never changes state
    /// or delays a phase; animations run on unscaled time and are simply cut when the phase changes. The settings
    /// panel's commands go out through the command sink like any other input.
    /// </summary>
    public sealed class TvPresenter : MonoBehaviour
    {
        private readonly TvContext _context = new TvContext();
        private LobbyView _lobby;
        private CountdownView _countdown;
        private IntroView _intro;
        private TopicVoteView _topicVote;
        private QuestionView _question;
        private LeaderboardView _leaderboard;
        private RoundEndView _roundEnd;
        private RaceView _race;
        private ResultsView _results;
        private PollView _poll;
        private HostBubblesView _hosts;
        private TvSettingsPanel _settings;
        private TvPauseMenu _pauseMenu;
        private TvBankChooser _bankChooser;
        private TvGraphicsPanel _graphics;
        private Action<IGameCommand> _send;
        private GameObject _relayBannerRoot;
        private TextMeshProUGUI _relayBanner;
        private TextMeshProUGUI _contentError;
        private GameObject _current;
        private Image _background;
        private StudioStage _stage;
        private StudioToggleFile _toggleFile;
        private GamePhase _phase = GamePhase.Lobby;
        private bool _relayDown = true;
        private bool _broken;

        /// <summary>Builds the canvas. Shows the Lobby with the relay down text until ShowRoom.</summary>
        public static TvPresenter Create()
        {
            var go = new GameObject("TvPresenter");
            var presenter = go.AddComponent<TvPresenter>();
            presenter.Build();
            return presenter;
        }

        /// <summary>Strings and characters before a Game exists (the content error path); Attach replaces them.</summary>
        public void Configure(UiStrings strings, IReadOnlyList<CharacterDef> characters, string lang)
        {
            _context.Configure(strings, characters, lang);
            Relabel();
        }

        /// <summary>Reads strings, characters and the game language from the game from now on. Call once, before Game.Start events.</summary>
        public void Attach(Game game)
        {
            _context.Attach(game);
            Relabel();
            _lobby.Refresh();
            // The 3D studio behind the overlay (decisions 0015, 0016), on unless switched off on this laptop.
            _stage.SetOn(_toggleFile.Load());
            // The question bank test (decisions/0020, D118): ask which bank to play before anyone joins.
            if (game.Config.Banks.ChooseOnStart) _bankChooser.SetOpen(true);
            OpenFromCommandLine();
        }

        public void ShowRoom(string code, string joinUrl)
        {
            _relayDown = false;
            _lobby.ShowRoom(code, joinUrl);
            UpdateRelayBanner();
        }

        /// <summary>While the relay socket is not open (docs/18_edge_cases.md): in the Lobby instead of the room code, over any other phase as a banner.</summary>
        public void ShowRelayDown()
        {
            _relayDown = true;
            _lobby.SetRelayDown(true);
            UpdateRelayBanner();
        }

        /// <summary>
        /// Where the TV settings panel sends its commands (ChangeTvSettings, ResetSeen, BackToLobby); the composition
        /// root passes cmd => Dispatch(game.Handle(cmd)), so the events come back through OnEvents.
        /// </summary>
        public void SetCommandSink(Action<IGameCommand> send)
        {
            _send = send;
            _settings.SetCommandSink(send);
            _bankChooser.SetCommandSink(send);
        }

        /// <summary>The music slider and credit line of the TV settings panel (decisions/0019, D114, D115).</summary>
        public void SetMusic(Func<float> volume, Action<float> setVolume, Func<string> creditText)
        {
            _settings.SetMusic(volume, setVolume, creditText);
        }

        /// <summary>The Graphics panel of the pause menu (decisions/0021): where the options come from and go to.</summary>
        public void SetGraphics(Func<FamilyQuiz.Presentation.Graphics.GraphicsOptions> get, Action<FamilyQuiz.Presentation.Graphics.GraphicsOptions> set,
            Func<List<Vector2Int>> resolutions)
        {
            _graphics.SetGraphics(get, set, resolutions);
        }

        /// <summary>The sound effects slider of the TV settings panel (decisions/0020, D120).</summary>
        public void SetSfx(Func<float> volume, Action<float> setVolume)
        {
            _settings.SetSfx(volume, setVolume);
        }

        /// <summary>
        /// A host line that starts now (docs/15_schemas.md, "Selection"): the host's portrait box and text bubble until
        /// endsAtMs on the core clock (Game.NowMs), replacing that host's bubble. textInLang gives the text in a language;
        /// the bubble is redrawn when the game language changes. Unknown hosts are ignored.
        /// </summary>
        public void ShowHostLine(string host, Func<string, string> textInLang, long endsAtMs)
        {
            if (_broken) return;
            _hosts.Show(host, textInLang, endsAtMs);
            _stage.OnHostLine(host, endsAtMs);
        }

        /// <summary>A broken config or content file: the error instead of everything else, for good.</summary>
        public void ShowContentError(string message)
        {
            _broken = true;
            _stage.SetOn(false);
            HideViews();
            _hosts.HideAll();
            _settings.SetOpen(false);
            _pauseMenu.SetOpen(false);
            _bankChooser.SetOpen(false);
            _graphics.SetOpen(false);
            _relayBannerRoot.SetActive(false);
            var title = _context.Has("tv.error.content") ? _context.T("tv.error.content") + "\n\n" : string.Empty;
            _contentError.text = title + message;
            _contentError.gameObject.SetActive(true);
        }

        /// <summary>Hand every batch of events Core returns (Start, Handle, Tick), in order.</summary>
        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (_broken || _context.Game == null || events == null || events.Count == 0) return;
            _stage.OnEvents(events);
            bool dirty = false;
            foreach (var e in events)
            {
                switch (e)
                {
                    case PhaseStarted started:
                        Enter(started.Phase);
                        dirty = false; // Enter draws the whole view
                        break;
                    case AnswerLocked locked:
                        if (_phase == GamePhase.Answer) _question.OnAnswerLocked(locked.PlayerId);
                        else if (_phase == GamePhase.RaceQuestion) _race.OnAnswerLocked(locked.PlayerId);
                        break;
                    case Revealed revealed:
                        if (_phase == GamePhase.Reveal) _question.OnRevealed(revealed.CorrectDisplayIndex);
                        break;
                    case VoteCast vote:
                        if (_phase == GamePhase.TopicVote) _topicVote.OnVoteCast(vote.PlayerId);
                        break;
                    case RaceRevealed _:
                        if (_phase == GamePhase.RaceReveal) _race.OnRaceRevealed();
                        break;
                    case PowerUpApplied _:
                        if (_phase == GamePhase.Answer) _question.RefreshHits();
                        break;
                    case PowerUpUsed _:
                        if (_phase == GamePhase.Reveal) _question.RefreshHits();
                        else if (_phase == GamePhase.Scores) _leaderboard.RefreshHits();
                        break;
                    case PollSubmitted _:
                        if (_phase == GamePhase.Poll) _poll.Refresh();
                        break;
                    case GamePaused _:
                    case GameResumed _:
                        break; // the menu follows GameState.Paused below

                    case SettingsChanged _:
                        Relabel();
                        dirty = true;
                        break;
                    case RosterChanged _:
                    case PlayerConnectionChanged _:
                    case PlayerUpdated _:
                    case PlayerLeft _:
                    case ReadyChanged _:
                        dirty = true;
                        break;
                }
            }

            if (dirty) RefreshCurrent();
            // The pause menu (D105) is up exactly while the game is paused; New game hides in the Lobby.
            _pauseMenu.SetOpen(_context.Game.State.Paused);
            // Settings, the running game's copy (GameStarted, GameEnded), seen questions: whatever changed shows at once.
            if (_settings.IsOpen) _settings.Refresh();
            if (_bankChooser.IsOpen) _bankChooser.Refresh();
            if (_graphics.IsOpen && events.Count > 0) _graphics.Refresh();
        }

        private void Update()
        {
            if (_broken || _context.Game == null) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame) _settings.Toggle();
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) OnEscape();
            _hosts.Tick();
            switch (_phase)
            {
                case GamePhase.Lobby:
                    _lobby.Tick();
                    break;
                case GamePhase.StartCountdown:
                    _countdown.Tick();
                    break;
                case GamePhase.TopicVote:
                    _topicVote.Tick();
                    break;
                case GamePhase.Results:
                    _results.Tick();
                    break;
                case GamePhase.Poll:
                    _poll.Tick();
                    break;
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                case GamePhase.Reveal:
                    _question.Tick();
                    break;
                case GamePhase.RaceQuestion:
                    _race.Tick();
                    break;
            }
        }

        /// <summary>
        /// A developer aid for screenshots without a keyboard: "-tvOpen settings|graphics|bank" on the command line opens
        /// that panel at start (and closes the bank chooser for the others).
        /// </summary>
        private void OpenFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != "-tvOpen") continue;
                string what = args[i + 1];
                if (what != "bank") _bankChooser.SetOpen(false);
                if (what == "settings") _settings.SetOpen(true);
                else if (what == "graphics") _graphics.SetOpen(true);
            }
        }

        /// <summary>Escape (decisions/0018, D105, D106): pause, close the settings opened from the pause menu, or resume.</summary>
        private void OnEscape()
        {
            if (_graphics.IsOpen)
            {
                // Back to the pause menu.
                _graphics.SetOpen(false);
                return;
            }

            if (_bankChooser.IsOpen)
            {
                // Escape keeps the current bank (and stays paused when the chooser came from the pause menu).
                _bankChooser.SetOpen(false);
                return;
            }

            switch (EscapeKey.For(_context.Game.State.Paused, _settings.IsOpen))
            {
                case EscapeKey.Action.Pause:
                    _settings.SetOpen(false);
                    _send?.Invoke(new PauseGame());
                    break;
                case EscapeKey.Action.CloseSettings:
                    _settings.SetOpen(false);
                    break;
                case EscapeKey.Action.Resume:
                    _send?.Invoke(new ResumeGame());
                    break;
            }
        }

        private void OnPauseContinue()
        {
            _settings.SetOpen(false);
            _graphics.SetOpen(false);
            _send?.Invoke(new ResumeGame());
        }

        private void OnPauseNewGame()
        {
            _settings.SetOpen(false);
            _send?.Invoke(new RestartGame());
        }

        private void OnPauseSettings()
        {
            _settings.SetOpen(true);
        }

        private void OnPauseGraphics()
        {
            _settings.SetOpen(false);
            _graphics.SetOpen(true);
        }

        private void OnPauseBank()
        {
            _settings.SetOpen(false);
            _bankChooser.SetOpen(true);
        }

        private void Enter(GamePhase phase)
        {
            _phase = phase;
            switch (phase)
            {
                case GamePhase.Lobby:
                    Show(_lobby.gameObject);
                    _lobby.Refresh();
                    break;
                case GamePhase.StartCountdown:
                    Show(_countdown.gameObject);
                    _countdown.Enter();
                    break;
                case GamePhase.Intro:
                    Show(_intro.gameObject);
                    _intro.Enter();
                    break;
                case GamePhase.TopicVote:
                    Show(_topicVote.gameObject);
                    _topicVote.Enter();
                    break;
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                case GamePhase.Reveal:
                    Show(_question.gameObject);
                    _question.Enter(phase);
                    break;
                case GamePhase.Scores:
                    Show(_leaderboard.gameObject);
                    _leaderboard.Enter();
                    break;
                case GamePhase.RoundEnd:
                    Show(_roundEnd.gameObject);
                    _roundEnd.Enter();
                    break;
                case GamePhase.FinalIntro:
                case GamePhase.RaceQuestion:
                case GamePhase.RaceReveal:
                    Show(_race.gameObject);
                    _race.Enter(phase);
                    break;
                case GamePhase.Results:
                    Show(_results.gameObject);
                    _results.Enter();
                    break;
                case GamePhase.Poll:
                    Show(_poll.gameObject);
                    _poll.Enter();
                    break;
            }

            UpdateRelayBanner();
        }

        private void RefreshCurrent()
        {
            switch (_phase)
            {
                case GamePhase.Lobby:
                    _lobby.Refresh();
                    break;
                case GamePhase.Intro:
                    _intro.Refresh();
                    break;
                case GamePhase.TopicVote:
                    _topicVote.Refresh();
                    break;
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                case GamePhase.Reveal:
                    _question.Refresh();
                    break;
                case GamePhase.Scores:
                    _leaderboard.Refresh();
                    break;
                case GamePhase.RoundEnd:
                    _roundEnd.Refresh();
                    break;
                case GamePhase.FinalIntro:
                case GamePhase.RaceQuestion:
                case GamePhase.RaceReveal:
                    _race.Refresh();
                    break;
                case GamePhase.Results:
                    _results.Refresh();
                    break;
                case GamePhase.Poll:
                    _poll.Refresh();
                    break;
            }
        }

        private void Relabel()
        {
            _lobby.Relabel();
            if (_pauseMenu != null && _pauseMenu.IsOpen) _pauseMenu.Refresh();
            _relayBanner.text = _context.T("tv.error.relayDown");
        }

        private void UpdateRelayBanner()
        {
            _relayBannerRoot.SetActive(!_broken && _relayDown && _phase != GamePhase.Lobby);
        }

        /// <summary>
        /// Shows one view and hides the others. A view that stays (QuestionShow, Answer, Reveal; FinalIntro, RaceQuestion,
        /// RaceReveal) is not toggled, so a pop that started in the same batch (the last lock in, then Reveal) keeps playing.
        /// </summary>
        private void Show(GameObject view)
        {
            if (_current == view && view.activeSelf) return;
            HideViews();
            view.SetActive(true);
            _current = view;
        }

        private void HideViews()
        {
            _current = null;
            _lobby.gameObject.SetActive(false);
            _countdown.gameObject.SetActive(false);
            _intro.gameObject.SetActive(false);
            _topicVote.gameObject.SetActive(false);
            _question.gameObject.SetActive(false);
            _leaderboard.gameObject.SetActive(false);
            _roundEnd.gameObject.SetActive(false);
            _race.gameObject.SetActive(false);
            _results.gameObject.SetActive(false);
            _poll.gameObject.SetActive(false);
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>(); // the settings panel's buttons take mouse clicks
            EnsureEventSystem();

            var root = (RectTransform)transform;
            _background = UiBuilder.Image("Background", root, TvColors.Background);
            UiBuilder.Anchor(_background.rectTransform, 0, 0, 1, 1);

            _lobby = LobbyView.Create(root, _context);
            _countdown = CountdownView.Create(root, _context);
            _intro = IntroView.Create(root, _context);
            _topicVote = TopicVoteView.Create(root, _context);
            _question = QuestionView.Create(root, _context);
            _leaderboard = LeaderboardView.Create(root, _context);
            _roundEnd = RoundEndView.Create(root, _context);
            _race = RaceView.Create(root, _context);
            _results = ResultsView.Create(root, _context);
            _poll = PollView.Create(root, _context);

            // Overlays in drawing order: the host bubbles above the views, then the relay banner, then the settings panel.
            _hosts = HostBubblesView.Create(root, _context);

            var banner = UiBuilder.Image("RelayBanner", root, TvColors.Background);
            UiBuilder.Anchor(banner.rectTransform, 0, 0.9f, 1, 1);
            _relayBanner = UiBuilder.Text("Text", banner.transform, 56, TvColors.Error, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_relayBanner.rectTransform, 0.02f, 0, 0.98f, 1);
            _relayBannerRoot = banner.gameObject;

            // The pause menu under the settings panel, so Settings in the menu opens the panel over it (D106).
            _pauseMenu = TvPauseMenu.Create(root, _context, OnPauseContinue, OnPauseNewGame, OnPauseSettings, OnPauseBank,
                () => _bankChooser != null && _bankChooser.HasChoice, OnPauseGraphics);
            _settings = TvSettingsPanel.Create(root, _context);
            _bankChooser = TvBankChooser.Create(root, _context);
            _graphics = TvGraphicsPanel.Create(root, _context);
            _toggleFile = new StudioToggleFile(System.IO.Path.Combine(Application.persistentDataPath, StudioToggleFile.FileName));
            _stage = StudioStage.Create(_context, OnStudioVisible);
            _settings.SetStudioToggle(() => _stage.IsOn, on =>
            {
                _toggleFile.Save(on);
                _stage.SetOn(on);
            });

            _contentError = UiBuilder.Text("ContentError", root, 40, TvColors.Error, false, TextAlignmentOptions.TopLeft);
            UiBuilder.Anchor(_contentError.rectTransform, 0.05f, 0.05f, 0.95f, 0.95f);
            _contentError.overflowMode = TextOverflowModes.Overflow;
            _contentError.gameObject.SetActive(false);

            HideViews();
            Show(_lobby.gameObject);
            UpdateRelayBanner();
        }

        /// <summary>The 3D studio shows behind the overlay: the flat background goes away and the views make room for the set.</summary>
        private void OnStudioVisible(bool visible)
        {
            _background.enabled = !visible;
            _lobby.SetStudioMode(visible);
            _question.SetStudioMode(visible);
            _intro.SetStudioMode(visible);
        }

        /// <summary>UGUI needs one EventSystem for mouse clicks; with the new Input System only, its UI input module.</summary>
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.AddComponent<InputSystemUIInputModule>(); // assigns the default UI actions when enabled
        }
    }
}
