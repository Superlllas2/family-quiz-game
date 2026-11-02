using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The TV's root view: one screen space overlay canvas (1920 x 1080 reference) with one plain view per phase,
    /// switched on PhaseStarted. A listener only: it reads Game and GameState and the events it is handed, and never
    /// changes state or delays a phase; animations run on unscaled time and are simply cut when the phase changes.
    /// </summary>
    public sealed class TvPresenter : MonoBehaviour
    {
        private readonly TvContext _context = new TvContext();
        private LobbyView _lobby;
        private CountdownView _countdown;
        private QuestionView _question;
        private LeaderboardView _leaderboard;
        private ResultsView _results;
        private GameObject _relayBannerRoot;
        private TextMeshProUGUI _relayBanner;
        private TextMeshProUGUI _contentError;
        private GameObject _current;
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

        /// <summary>A broken config or content file: the error instead of everything else, for good.</summary>
        public void ShowContentError(string message)
        {
            _broken = true;
            HideViews();
            _relayBannerRoot.SetActive(false);
            var title = _context.Has("tv.error.content") ? _context.T("tv.error.content") + "\n\n" : string.Empty;
            _contentError.text = title + message;
            _contentError.gameObject.SetActive(true);
        }

        /// <summary>Hand every batch of events Core returns (Start, Handle, Tick), in order.</summary>
        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (_broken || _context.Game == null || events == null || events.Count == 0) return;
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
                        break;
                    case Revealed revealed:
                        if (_phase == GamePhase.Reveal) _question.OnRevealed(revealed.CorrectDisplayIndex);
                        break;
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
        }

        private void Update()
        {
            if (_broken || _context.Game == null) return;
            switch (_phase)
            {
                case GamePhase.Lobby:
                    _lobby.Tick();
                    break;
                case GamePhase.StartCountdown:
                    _countdown.Tick();
                    break;
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                case GamePhase.Reveal:
                    _question.Tick();
                    break;
            }
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
                case GamePhase.Results:
                    Show(_results.gameObject);
                    _results.Enter();
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
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                case GamePhase.Reveal:
                    _question.Refresh();
                    break;
                case GamePhase.Scores:
                    _leaderboard.Refresh();
                    break;
                case GamePhase.Results:
                    _results.Refresh();
                    break;
            }
        }

        private void Relabel()
        {
            _lobby.Relabel();
            _relayBanner.text = _context.T("tv.error.relayDown");
        }

        private void UpdateRelayBanner()
        {
            _relayBannerRoot.SetActive(!_broken && _relayDown && _phase != GamePhase.Lobby);
        }

        /// <summary>
        /// Shows one view and hides the others. A view that stays (QuestionShow, Answer, Reveal) is not toggled, so
        /// a pop that started in the same batch (the last lock in, then Reveal) keeps playing.
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
            _question.gameObject.SetActive(false);
            _leaderboard.gameObject.SetActive(false);
            _results.gameObject.SetActive(false);
        }

        private void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var root = (RectTransform)transform;
            UiBuilder.Anchor(UiBuilder.Image("Background", root, TvColors.Background).rectTransform, 0, 0, 1, 1);

            _lobby = LobbyView.Create(root, _context);
            _countdown = CountdownView.Create(root, _context);
            _question = QuestionView.Create(root, _context);
            _leaderboard = LeaderboardView.Create(root, _context);
            _results = ResultsView.Create(root, _context);

            var banner = UiBuilder.Image("RelayBanner", root, TvColors.Background);
            UiBuilder.Anchor(banner.rectTransform, 0, 0.9f, 1, 1);
            _relayBanner = UiBuilder.Text("Text", banner.transform, 56, TvColors.Error, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_relayBanner.rectTransform, 0.02f, 0, 0.98f, 1);
            _relayBannerRoot = banner.gameObject;

            _contentError = UiBuilder.Text("ContentError", root, 40, TvColors.Error, false, TextAlignmentOptions.TopLeft);
            UiBuilder.Anchor(_contentError.rectTransform, 0.05f, 0.05f, 0.95f, 0.95f);
            _contentError.overflowMode = TextOverflowModes.Overflow;
            _contentError.gameObject.SetActive(false);

            HideViews();
            Show(_lobby.gameObject);
            UpdateRelayBanner();
        }
    }
}
