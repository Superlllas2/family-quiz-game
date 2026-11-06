using System;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The TV settings panel (docs/02_show_flow.md, "TV settings panel"; docs/13_architecture.md): opened and closed with
    /// Tab on the laptop, used with the mouse, never pausing anything. Game language RU or EN (at once), power ups on or
    /// off, each type on or off and targeting (from the next game, with a note while they differ from the running game),
    /// the 3D studio on or off (this laptop only, at once), question feedback on or off (at once, decisions/0018 D108),
    /// Reset seen questions and Back to lobby. It shows the pending values and sends commands through the sink like any
    /// other input; it never changes state itself.
    /// </summary>
    public sealed class TvSettingsPanel : MonoBehaviour
    {
        /// <summary>
        /// How long a first click on Back to lobby waits for the confirming second click. A UI choice, not a game rule,
        /// so not in content/config.json.
        /// </summary>
        public const double BackToLobbyConfirmSeconds = 4;

        private const float PanelWidth = 860f;
        private const float PanelHeight = 1000f;
        private const float EdgeMargin = 24f;
        private const float Padding = 32f;
        private const float TitleSize = 52f;
        private const float HintSize = 28f;
        private const float LabelSize = 36f;
        private const float NoteSize = 24f;
        private const float ButtonLabelSize = 32f;
        private const float RowsTop = 110f;
        private const float RowStep = 82f;
        private const float LabelWidth = 360f;
        private const float ButtonWidth = 204f;
        private const float ButtonGap = 8f;
        private const float ButtonHeight = 64f;
        private const float WideButtonHeight = 68f;

        private readonly ConfirmClick _backConfirm = new ConfirmClick(BackToLobbyConfirmSeconds);
        private readonly TextMeshProUGUI[] _typeLabels = new TextMeshProUGUI[3];
        private readonly TextMeshProUGUI[] _typeNotes = new TextMeshProUGUI[3];
        private readonly PanelButton[] _typeOn = new PanelButton[3];
        private readonly PanelButton[] _typeOff = new PanelButton[3];
        private TvContext _context;
        private Action<IGameCommand> _send;
        private RectTransform _card;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private TextMeshProUGUI _langLabel;
        private PanelButton _langRu;
        private PanelButton _langEn;
        private TextMeshProUGUI _powerUpsLabel;
        private TextMeshProUGUI _powerUpsNote;
        private PanelButton _powerUpsOn;
        private PanelButton _powerUpsOff;
        private TextMeshProUGUI _targetingLabel;
        private TextMeshProUGUI _targetingNote;
        private PanelButton _targetingFree;
        private PanelButton _targetingAimUp;
        private TextMeshProUGUI _studioLabel;
        private PanelButton _studioOn;
        private PanelButton _studioOff;
        private Func<bool> _studioIsOn;
        private Action<bool> _setStudio;
        private TextMeshProUGUI _feedbackLabel;
        private PanelButton _feedbackOn;
        private PanelButton _feedbackOff;
        private PanelButton _resetSeen;
        private TextMeshProUGUI _seenCount;
        private PanelButton _backToLobby;
        private bool _backArmedShown;

        public static TvSettingsPanel Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TvSettingsPanel", parent), 0, 0, 1, 1);
            var panel = rect.gameObject.AddComponent<TvSettingsPanel>();
            panel._context = context;
            panel.Build();
            rect.gameObject.SetActive(false);
            return panel;
        }

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Where the panel's commands go; the composition root passes cmd => Dispatch(game.Handle(cmd)).</summary>
        public void SetCommandSink(Action<IGameCommand> send)
        {
            _send = send;
        }

        /// <summary>
        /// The 3D studio toggle (decisions 0016): a TV only choice handled by the presenter, never a game command.
        /// </summary>
        public void SetStudioToggle(Func<bool> isOn, Action<bool> set)
        {
            _studioIsOn = isOn;
            _setStudio = set;
        }

        /// <summary>Tab: opens or closes the panel. Nothing else stops or waits.</summary>
        public void Toggle()
        {
            SetOpen(!IsOpen);
        }

        public void SetOpen(bool open)
        {
            if (open == IsOpen) return;
            _backConfirm.Cancel();
            gameObject.SetActive(open);
            if (open) Refresh();
        }

        /// <summary>Draws the pending values in the game language; called on open and after every event batch while open.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            if (game == null) return;
            var state = game.State;
            var pending = state.PendingSettings;

            _title.text = _context.T("tv.settings.title");
            _hint.text = _context.T("tv.settings.closeHint");

            _langLabel.text = _context.T("tv.settings.gameLanguage");
            Choice(_langRu, "tv.settings.langRu", state.GameLanguage == Languages.Russian);
            Choice(_langEn, "tv.settings.langEn", state.GameLanguage == Languages.English);

            _powerUpsLabel.text = _context.T("tv.settings.powerUps");
            Choice(_powerUpsOn, "tv.settings.on", pending.PowerUpsEnabled);
            Choice(_powerUpsOff, "tv.settings.off", !pending.PowerUpsEnabled);
            Note(_powerUpsNote, SettingsChoices.PowerUpsDiffer(state));

            for (int i = 0; i < _typeLabels.Length; i++)
            {
                string type = PowerUpTypes.All[i];
                bool on = pending.IsTypeOn(type);
                _typeLabels[i].text = _context.T("tv.powerUps." + type);
                Choice(_typeOn[i], "tv.settings.on", on);
                Choice(_typeOff[i], "tv.settings.off", !on);
                Note(_typeNotes[i], SettingsChoices.TypeDiffers(state, type));
            }

            _targetingLabel.text = _context.T("tv.settings.targeting");
            Choice(_targetingFree, "tv.settings.targetingFree", pending.Targeting == TargetingModes.Free);
            Choice(_targetingAimUp, "tv.settings.targetingAimUp", pending.Targeting == TargetingModes.AimUp);
            Note(_targetingNote, SettingsChoices.TargetingDiffers(state));

            _studioLabel.text = _context.T("tv.settings.studio3d");
            bool studio = _studioIsOn != null && _studioIsOn();
            Choice(_studioOn, "tv.settings.on", studio);
            Choice(_studioOff, "tv.settings.off", !studio);

            _feedbackLabel.text = _context.T("tv.settings.feedback");
            Choice(_feedbackOn, "tv.settings.on", state.FeedbackEnabled);
            Choice(_feedbackOff, "tv.settings.off", !state.FeedbackEnabled);

            _resetSeen.Set(_context.T("tv.settings.resetSeen"), TvColors.ButtonOff);
            _seenCount.text = _context.Format("tv.settings.seenCount", "questions", state.SeenQuestionIds.Count.ToString(),
                "statements", state.SeenRaceIds.Count.ToString());
            DrawBackToLobby();
        }

        private void Update()
        {
            // The confirm window runs out on real time: redraw the button when it does.
            if (_backConfirm.Armed(Now()) != _backArmedShown) DrawBackToLobby();
        }

        private void DrawBackToLobby()
        {
            _backArmedShown = _backConfirm.Armed(Now());
            _backToLobby.Set(_context.T(_backArmedShown ? "tv.settings.backToLobbyConfirm" : "tv.settings.backToLobby"),
                _backArmedShown ? TvColors.Error : TvColors.ButtonOff);
        }

        private void Choice(PanelButton button, string key, bool selected)
        {
            button.Set(_context.T(key), selected ? TvColors.ButtonOn : TvColors.ButtonOff);
        }

        private void Note(TextMeshProUGUI note, bool show)
        {
            note.gameObject.SetActive(show);
            if (show) note.text = _context.T("tv.settings.nextGame");
        }

        private static double Now()
        {
            return Time.unscaledTimeAsDouble;
        }

        private void Send(IGameCommand command)
        {
            if (_send == null)
            {
                Debug.LogWarning("TV settings panel: no command sink, ignored " + command.GetType().Name);
                return;
            }

            _send(command);
            Refresh();
        }

        private void OnLanguage(string lang)
        {
            if (_context.Game == null) return;
            Send(new ChangeTvSettings(gameLang: lang));
        }

        private void OnPowerUps(bool on)
        {
            if (_context.Game == null) return;
            Send(new ChangeTvSettings(powerUpsEnabled: on));
        }

        private void OnType(string type, bool on)
        {
            if (_context.Game == null) return;
            var types = SettingsChoices.WithType(_context.Game.State.PendingSettings.PowerUpTypes, type, on);
            Send(new ChangeTvSettings(powerUpTypes: types));
        }

        private void OnTargeting(string targeting)
        {
            if (_context.Game == null) return;
            Send(new ChangeTvSettings(targeting: targeting));
        }

        private void OnStudio(bool on)
        {
            _setStudio?.Invoke(on);
            Refresh();
        }

        private void OnFeedback(bool on)
        {
            if (_context.Game == null) return;
            Send(new ChangeTvSettings(feedbackEnabled: on));
        }

        private void OnResetSeen()
        {
            if (_context.Game == null) return;
            Send(new ResetSeen());
        }

        /// <summary>In the Lobby one click (nothing to abandon); in any other phase a second click within the window.</summary>
        private void OnBackToLobby()
        {
            var game = _context.Game;
            if (game == null) return;
            bool act = game.State.Phase == GamePhase.Lobby || _backConfirm.Press(Now());
            if (act)
            {
                _backConfirm.Cancel();
                Send(new BackToLobby());
            }

            DrawBackToLobby();
        }

        private void Build()
        {
            _card = UiBuilder.Image("Card", transform, TvColors.Panel, RoundedSprite.Get()).rectTransform;
            _card.GetComponent<UnityEngine.UI.Image>().raycastTarget = true; // clicks on the card never reach the views
            _card.anchorMin = new Vector2(1, 0.5f);
            _card.anchorMax = new Vector2(1, 0.5f);
            _card.pivot = new Vector2(1, 0.5f);
            _card.anchoredPosition = new Vector2(-EdgeMargin, 0);
            _card.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            _title = Label("Title", Padding, 24, 480, 64, TitleSize, TvColors.Text, true, TextAlignmentOptions.Left);
            _hint = Label("Hint", PanelWidth - Padding - 280, 24, 280, 64, HintSize, TvColors.DimText, false, TextAlignmentOptions.Right);

            float y = RowsTop;
            _langLabel = RowLabel("Language", y, out _);
            _langRu = RowButton("Ru", y, 0, () => OnLanguage(Languages.Russian));
            _langEn = RowButton("En", y, 1, () => OnLanguage(Languages.English));
            y += RowStep;

            _powerUpsLabel = RowLabel("PowerUps", y, out _powerUpsNote);
            _powerUpsOn = RowButton("PowerUpsOn", y, 0, () => OnPowerUps(true));
            _powerUpsOff = RowButton("PowerUpsOff", y, 1, () => OnPowerUps(false));
            y += RowStep;

            for (int i = 0; i < _typeLabels.Length; i++)
            {
                string type = PowerUpTypes.All[i];
                _typeLabels[i] = RowLabel("Type " + type, y, out _typeNotes[i]);
                _typeOn[i] = RowButton(type + "On", y, 0, () => OnType(type, true));
                _typeOff[i] = RowButton(type + "Off", y, 1, () => OnType(type, false));
                y += RowStep;
            }

            _targetingLabel = RowLabel("Targeting", y, out _targetingNote);
            _targetingFree = RowButton("TargetingFree", y, 0, () => OnTargeting(TargetingModes.Free));
            _targetingAimUp = RowButton("TargetingAimUp", y, 1, () => OnTargeting(TargetingModes.AimUp));
            y += RowStep;

            _studioLabel = RowLabel("Studio3d", y, out _);
            _studioOn = RowButton("Studio3dOn", y, 0, () => OnStudio(true));
            _studioOff = RowButton("Studio3dOff", y, 1, () => OnStudio(false));
            y += RowStep;

            _feedbackLabel = RowLabel("Feedback", y, out _);
            _feedbackOn = RowButton("FeedbackOn", y, 0, () => OnFeedback(true));
            _feedbackOff = RowButton("FeedbackOff", y, 1, () => OnFeedback(false));
            y += RowStep + 20;

            _resetSeen = PanelButton.Create(_card, "ResetSeen", ButtonLabelSize, OnResetSeen);
            Box(_resetSeen.Rect, Padding, y, PanelWidth - 2 * Padding, WideButtonHeight);
            y += WideButtonHeight + 6;
            _seenCount = Label("SeenCount", Padding, y, PanelWidth - 2 * Padding, 40, 28, TvColors.DimText, false, TextAlignmentOptions.Center);
            y += 40 + 16;

            _backToLobby = PanelButton.Create(_card, "BackToLobby", ButtonLabelSize, OnBackToLobby);
            Box(_backToLobby.Rect, Padding, y, PanelWidth - 2 * Padding, WideButtonHeight);
        }

        /// <summary>A row's label in its upper part and a hidden "applies from the next game" note below it.</summary>
        private TextMeshProUGUI RowLabel(string name, float y, out TextMeshProUGUI note)
        {
            var label = Label(name, Padding, y, LabelWidth, 56, LabelSize, TvColors.Text, true, TextAlignmentOptions.Left);
            note = Label(name + "Note", Padding, y + 54, LabelWidth, 32, NoteSize, TvColors.Crown, false, TextAlignmentOptions.Left);
            note.gameObject.SetActive(false);
            return label;
        }

        /// <summary>One of the two choice buttons at the right of a row (column 0 or 1).</summary>
        private PanelButton RowButton(string name, float y, int column, Action onClick)
        {
            var button = PanelButton.Create(_card, name, ButtonLabelSize, onClick);
            float x = Padding + LabelWidth + 12 + column * (ButtonWidth + ButtonGap);
            Box(button.Rect, x, y + 12, ButtonWidth, ButtonHeight);
            return button;
        }

        private TextMeshProUGUI Label(string name, float x, float y, float width, float height, float size, Color color,
            bool bold, TextAlignmentOptions alignment)
        {
            var text = UiBuilder.Text(name, _card, size, color, bold, alignment);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(18f, size);
            text.fontSizeMax = size;
            Box(text.rectTransform, x, y, width, height);
            return text;
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
