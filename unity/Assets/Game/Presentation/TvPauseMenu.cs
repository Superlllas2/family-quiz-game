using System;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The pause menu (decisions/0018, D105 to D107), drawn as a board game piece (D100): a card of card stock on the
    /// dimmed table with a printed ribbon for its title and chunky buttons in the answer inks (Continue green, Settings
    /// cobalt, Graphics teal, New game card stock that turns red when armed). Shown while the game is paused
    /// (GameState.Paused). Escape opens it; Escape again or Continue closes it. New game takes a confirming second click
    /// and is hidden in the Lobby. A view only: its buttons call the presenter, which sends the commands (ResumeGame,
    /// RestartGame) or opens the TV settings panel or the graphics panel.
    /// </summary>
    public sealed class TvPauseMenu : MonoBehaviour
    {
        /// <summary>How long a first click on New game waits for the confirming click; the same as Back to lobby.</summary>
        public const double NewGameConfirmSeconds = TvSettingsPanel.BackToLobbyConfirmSeconds;

        private const float CardWidth = 700f;
        private const float Padding = 48f;
        private const float RibbonWidth = 420f;
        private const float RibbonHeight = 110f;
        private const float HintSize = 28f;
        private const float ButtonLabelSize = 40f;
        private const float ButtonHeight = 88f;
        private const float ButtonGap = 24f;
        private const float ButtonsTop = 132f;

        private static readonly Color Backdrop = new Color(0.08f, 0.05f, 0.03f, 0.66f);

        private readonly ConfirmClick _newGameConfirm = new ConfirmClick(NewGameConfirmSeconds);
        private TvContext _context;
        private Action _onContinue;
        private Action _onNewGame;
        private Action _onSettings;
        private Action _onGraphics;
        private PanelButton _graphics;
        private TvPiece _card;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private PanelButton _continue;
        private PanelButton _newGame;
        private PanelButton _settings;
        private bool _armedShown;

        public static TvPauseMenu Create(Transform parent, TvContext context, Action onContinue, Action onNewGame, Action onSettings,
            Action onGraphics = null)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TvPauseMenu", parent), 0, 0, 1, 1);
            var menu = rect.gameObject.AddComponent<TvPauseMenu>();
            menu._context = context;
            menu._onContinue = onContinue;
            menu._onNewGame = onNewGame;
            menu._onSettings = onSettings;
            menu._onGraphics = onGraphics;
            menu.Build();
            rect.gameObject.SetActive(false);
            return menu;
        }

        public bool IsOpen => gameObject.activeSelf;

        public void SetOpen(bool open)
        {
            if (open == IsOpen)
            {
                if (open) Refresh();
                return;
            }

            _newGameConfirm.Cancel();
            gameObject.SetActive(open);
            // No TvTween pop here: its canvas group turns clicks off (it is made for pieces that are only looked at).
            if (open) Refresh();
        }

        /// <summary>Draws the texts in the game language; New game only outside the Lobby.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            if (game == null) return;
            _title.text = _context.T("tv.pause.title");
            _hint.text = _context.T("tv.pause.hint");
            _continue.Set(_context.T("tv.pause.continue"), TvColors.Green);
            _settings.Set(_context.T("tv.pause.settings"), TvColors.Cobalt);
            _graphics.Set(_context.T("tv.pause.graphics"), TvColors.Teal);
            bool newGame = game.State.Phase != GamePhase.Lobby;
            _newGame.Rect.gameObject.SetActive(newGame);
            Layout(newGame);
            DrawNewGame();
        }

        private void Update()
        {
            // The confirm window runs out on real time (the core clock is stopped): redraw the button when it does.
            if (_newGameConfirm.Armed(Now()) != _armedShown) DrawNewGame();
        }

        /// <summary>The buttons stack with no gap where one is hidden, and the card fits them.</summary>
        private void Layout(bool newGame)
        {
            float y = ButtonsTop;
            Place(_continue, y);
            y += ButtonHeight + ButtonGap;
            Place(_settings, y);
            y += ButtonHeight + ButtonGap;
            Place(_graphics, y);
            if (newGame)
            {
                y += ButtonHeight + ButtonGap;
                Place(_newGame, y);
            }

            _card.Root.sizeDelta = new Vector2(CardWidth, y + ButtonHeight + Padding);
        }

        private void Place(PanelButton button, float y)
        {
            Box(button.Rect, Padding, y, CardWidth - 2 * Padding, ButtonHeight);
        }

        private void DrawNewGame()
        {
            _armedShown = _newGameConfirm.Armed(Now());
            _newGame.Set(_context.T(_armedShown ? "tv.pause.newGameConfirm" : "tv.pause.newGame"),
                _armedShown ? TvColors.Red : TvColors.ButtonOff);
        }

        private void OnNewGame()
        {
            if (_newGameConfirm.Press(Now()))
            {
                _newGameConfirm.Cancel();
                _onNewGame?.Invoke();
            }

            DrawNewGame();
        }

        private static double Now()
        {
            return Time.unscaledTimeAsDouble;
        }

        private void Build()
        {
            var backdrop = UiBuilder.Image("Backdrop", transform, Backdrop);
            backdrop.raycastTarget = true; // clicks never reach the views under the menu
            UiBuilder.Anchor(backdrop.rectTransform, 0, 0, 1, 1);

            _card = TvPiece.Card("Card", transform, TvColors.Panel, 26f, 10f).Tilt(0.6f);
            _card.Body.raycastTarget = true;
            var root = _card.Root;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, -20);
            root.sizeDelta = new Vector2(CardWidth, 600);
            var card = _card.Body.rectTransform;

            // The title on a printed ribbon that sticks out over the card's top edge.
            var ribbon = TvPiece.Card("Ribbon", card, TvColors.Red, 16f, 7f).Tilt(-2f);
            var rr = ribbon.Root;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 1f);
            rr.anchoredPosition = new Vector2(0, RibbonHeight * 0.45f);
            rr.sizeDelta = new Vector2(RibbonWidth, RibbonHeight);
            _title = UiBuilder.Display("Title", ribbon.Content, 60, TvColors.TextOn(TvColors.Red), TextAlignmentOptions.Center);
            UiBuilder.Fit(_title, 30, 60, 1);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.08f, 0.95f, 0.92f);

            _hint = UiBuilder.Text("Hint", card, HintSize, TvColors.InkSoft, true, TextAlignmentOptions.Center);
            UiBuilder.Fit(_hint, 18, HintSize, 1);
            Box(_hint.rectTransform, Padding, 72, CardWidth - 2 * Padding, 40);

            _continue = PanelButton.Create(card, "Continue", ButtonLabelSize, () => _onContinue?.Invoke());
            _settings = PanelButton.Create(card, "Settings", ButtonLabelSize, () => _onSettings?.Invoke());
            _graphics = PanelButton.Create(card, "Graphics", ButtonLabelSize, () => _onGraphics?.Invoke());
            _newGame = PanelButton.Create(card, "NewGame", ButtonLabelSize, OnNewGame);
        }

        /// <summary>A box at x, y from the card's top left corner.</summary>
        private static void Box(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
