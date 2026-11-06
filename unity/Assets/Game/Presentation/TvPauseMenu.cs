using System;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The pause menu (decisions/0018, D105 to D107): a dimmed screen with Continue, New game and Settings, shown while
    /// the game is paused (GameState.Paused). Escape opens it; Escape again or Continue closes it. New game takes a
    /// confirming second click and is hidden in the Lobby. A view only: its buttons call the presenter, which sends the
    /// commands (ResumeGame, RestartGame) or opens the TV settings panel.
    /// </summary>
    public sealed class TvPauseMenu : MonoBehaviour
    {
        /// <summary>How long a first click on New game waits for the confirming click; the same as Back to lobby.</summary>
        public const double NewGameConfirmSeconds = TvSettingsPanel.BackToLobbyConfirmSeconds;

        private const float CardWidth = 760f;
        private const float Padding = 40f;
        private const float TitleSize = 72f;
        private const float HintSize = 30f;
        private const float ButtonLabelSize = 40f;
        private const float ButtonHeight = 92f;
        private const float ButtonGap = 22f;
        private const float ButtonsTop = 170f;

        private static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.62f);

        private readonly ConfirmClick _newGameConfirm = new ConfirmClick(NewGameConfirmSeconds);
        private TvContext _context;
        private Action _onContinue;
        private Action _onNewGame;
        private Action _onSettings;
        private RectTransform _card;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private PanelButton _continue;
        private PanelButton _newGame;
        private PanelButton _settings;
        private bool _armedShown;

        public static TvPauseMenu Create(Transform parent, TvContext context, Action onContinue, Action onNewGame, Action onSettings)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TvPauseMenu", parent), 0, 0, 1, 1);
            var menu = rect.gameObject.AddComponent<TvPauseMenu>();
            menu._context = context;
            menu._onContinue = onContinue;
            menu._onNewGame = onNewGame;
            menu._onSettings = onSettings;
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
            if (open) Refresh();
        }

        /// <summary>Draws the texts in the game language; New game only outside the Lobby.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            if (game == null) return;
            _title.text = _context.T("tv.pause.title");
            _hint.text = _context.T("tv.pause.hint");
            _continue.Set(_context.T("tv.pause.continue"), TvColors.ButtonOn);
            _settings.Set(_context.T("tv.pause.settings"), TvColors.ButtonOff);
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

        /// <summary>The buttons stack with no gap where New game is hidden, and the card fits them.</summary>
        private void Layout(bool newGame)
        {
            float y = ButtonsTop;
            Box(_continue.Rect, Padding, y, CardWidth - 2 * Padding, ButtonHeight);
            y += ButtonHeight + ButtonGap;
            if (newGame)
            {
                Box(_newGame.Rect, Padding, y, CardWidth - 2 * Padding, ButtonHeight);
                y += ButtonHeight + ButtonGap;
            }

            Box(_settings.Rect, Padding, y, CardWidth - 2 * Padding, ButtonHeight);
            _card.sizeDelta = new Vector2(CardWidth, y + ButtonHeight + Padding);
        }

        private void DrawNewGame()
        {
            _armedShown = _newGameConfirm.Armed(Now());
            _newGame.Set(_context.T(_armedShown ? "tv.pause.newGameConfirm" : "tv.pause.newGame"),
                _armedShown ? TvColors.Error : TvColors.ButtonOff);
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

            // Opaque, unlike the settings panel: nothing of the view under it shows through.
            var opaque = new Color(TvColors.Panel.r, TvColors.Panel.g, TvColors.Panel.b, 1f);
            var card = UiBuilder.Image("Card", transform, opaque, RoundedSprite.Get()).rectTransform;
            _card = card;
            card.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(CardWidth, ButtonsTop + 3 * ButtonHeight + 2 * ButtonGap + Padding);

            _title = Label(card, "Title", 30, 96, TitleSize, TvColors.Text, true);
            _hint = Label(card, "Hint", 112, 44, HintSize, TvColors.DimText, false);

            float y = ButtonsTop;
            _continue = Button(card, "Continue", y, () => _onContinue?.Invoke());
            y += ButtonHeight + ButtonGap;
            _newGame = Button(card, "NewGame", y, OnNewGame);
            y += ButtonHeight + ButtonGap;
            _settings = Button(card, "Settings", y, () => _onSettings?.Invoke());
        }

        private static TextMeshProUGUI Label(RectTransform card, string name, float y, float height, float size, Color color, bool bold)
        {
            var text = UiBuilder.Text(name, card, size, color, bold, TextAlignmentOptions.Center);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(22f, size);
            text.fontSizeMax = size;
            Box(text.rectTransform, Padding, y, CardWidth - 2 * Padding, height);
            return text;
        }

        private static PanelButton Button(RectTransform card, string name, float y, Action onClick)
        {
            var button = PanelButton.Create(card, name, ButtonLabelSize, onClick);
            Box(button.Rect, Padding, y, CardWidth - 2 * Padding, ButtonHeight);
            return button;
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
