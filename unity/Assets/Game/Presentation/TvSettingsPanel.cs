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
    /// the 3D studio on or off (this laptop only, at once), question feedback on or off (at once, decisions/0018 D108), the music volume with the credit of the track playing (this laptop only, at once, decisions/0019 D114, D115),
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
        private const float PanelHeight = 1050f;
        private const float EdgeMargin = 24f;
        private const float Padding = 32f;
        private const float TitleSize = 52f;
        private const float HintSize = 28f;
        private const float LabelSize = 36f;
        private const float NoteSize = 24f;
        private const float ButtonLabelSize = 32f;
        private const float RowsTop = 100f;
        private const float RowStep = 76f;
        private const float LabelWidth = 360f;
        private const float ButtonWidth = 204f;
        private const float ButtonGap = 8f;
        private const float ButtonHeight = 60f;
        private const float CreditSize = 22f;
        private const float SliderHandleWidth = 38f;
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
        private TextMeshProUGUI _musicLabel;
        private TextMeshProUGUI _musicValue;
        private UnityEngine.UI.Slider _musicSlider;
        private TextMeshProUGUI _sfxValue;
        private UnityEngine.UI.Slider _sfxSlider;
        private Func<float> _sfxVolume;
        private Action<float> _setSfxVolume;
        private TextMeshProUGUI _musicCredit;
        private Func<float> _musicVolume;
        private Action<float> _setMusicVolume;
        private Func<string> _musicCreditText;
        private string _creditShown;
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

        /// <summary>
        /// The music volume slider and the credit line of the track playing (decisions/0019, D114, D115): a TV only
        /// choice handled by the composition root, never a game command. creditText gives the credit already in the
        /// game language, or null when nothing plays.
        /// </summary>
        public void SetMusic(Func<float> volume, Action<float> setVolume, Func<string> creditText)
        {
            _musicVolume = volume;
            _setMusicVolume = setVolume;
            _musicCreditText = creditText;
        }

        /// <summary>The sound effects slider (decisions/0020, D120), beside the music slider.</summary>
        public void SetSfx(Func<float> volume, Action<float> setVolume)
        {
            _sfxVolume = volume;
            _setSfxVolume = setVolume;
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

            _musicLabel.text = _context.T("tv.settings.volume");
            float volume = _musicVolume != null ? _musicVolume() : 0f;
            _musicSlider.SetValueWithoutNotify(volume);
            _musicValue.text = Caption("tv.settings.music", volume);
            float sfx = _sfxVolume != null ? _sfxVolume() : 0f;
            _sfxSlider.SetValueWithoutNotify(sfx);
            _sfxValue.text = Caption("tv.settings.sounds", sfx);
            DrawCredit();

            _resetSeen.Set(_context.T("tv.settings.resetSeen"), TvColors.ButtonOff);
            _seenCount.text = _context.Format("tv.settings.seenCount", "questions", state.SeenQuestionIds.Count.ToString(),
                "statements", state.SeenRaceIds.Count.ToString());
            DrawBackToLobby();
        }

        private void Update()
        {
            // The confirm window runs out on real time: redraw the button when it does.
            if (_backConfirm.Armed(Now()) != _backArmedShown) DrawBackToLobby();
            DrawCredit(); // the next track may start while the panel is open
        }

        private void DrawCredit()
        {
            var credit = _musicCreditText?.Invoke() ?? string.Empty;
            if (credit == _creditShown) return;
            _creditShown = credit;
            _musicCredit.text = credit;
        }

        private void OnMusicVolume(float volume)
        {
            _setMusicVolume?.Invoke(volume);
            _musicValue.text = Caption("tv.settings.music", volume);
        }

        private void OnSfxVolume(float volume)
        {
            _setSfxVolume?.Invoke(volume);
            _sfxValue.text = Caption("tv.settings.sounds", volume);
        }

        /// <summary>"Музыка 35%" over a volume slider.</summary>
        private string Caption(string key, float volume)
        {
            return _context.T(key) + " " + Mathf.RoundToInt(volume * 100f) + "%";
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
            button.SetPressed(selected);
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
            // A card of card stock on the table (D100), a little turned; everything sits on its body.
            var piece = TvPiece.Card("Card", transform, TvColors.Panel, 26f, 10f).Tilt(-0.4f);
            piece.Body.raycastTarget = true; // clicks on the card never reach the views
            var root = piece.Root;
            root.anchorMin = new Vector2(1, 0.5f);
            root.anchorMax = new Vector2(1, 0.5f);
            root.pivot = new Vector2(1, 0.5f);
            root.anchoredPosition = new Vector2(-EdgeMargin, 4);
            root.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            _card = piece.Body.rectTransform;

            _title = UiBuilder.Display("Title", _card, TitleSize, TvColors.Ink, TextAlignmentOptions.Left);
            UiBuilder.Fit(_title, 30, TitleSize, 1);
            Box(_title.rectTransform, Padding, 24, 480, 64);
            _hint = Label("Hint", PanelWidth - Padding - 280, 24, 280, 64, HintSize, TvColors.InkSoft, false, TextAlignmentOptions.Right);

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
            y += RowStep;

            _musicLabel = RowLabel("Music", y, out _);
            float half = (2 * ButtonWidth + ButtonGap - 16) / 2;
            float sx = Padding + LabelWidth + 12;
            _musicSlider = VolumeSlider("MusicSlider", sx, y, half, OnMusicVolume, out _musicValue);
            _sfxSlider = VolumeSlider("SfxSlider", sx + half + 16, y, half, OnSfxVolume, out _sfxValue);
            y += RowStep + 20;

            _resetSeen = PanelButton.Create(_card, "ResetSeen", ButtonLabelSize, OnResetSeen);
            Box(_resetSeen.Rect, Padding, y, PanelWidth - 2 * Padding, WideButtonHeight);
            y += WideButtonHeight + 6;
            _seenCount = Label("SeenCount", Padding, y, PanelWidth - 2 * Padding, 40, 28, TvColors.InkSoft, false, TextAlignmentOptions.Center);
            y += 40 + 16;

            _backToLobby = PanelButton.Create(_card, "BackToLobby", ButtonLabelSize, OnBackToLobby);
            Box(_backToLobby.Rect, Padding, y, PanelWidth - 2 * Padding, WideButtonHeight);
            y += WideButtonHeight + 8;

            // The CC BY credit of the track playing (D115): the license asks for it, the settings are where games put it.
            _musicCredit = Label("MusicCredit", Padding, y, PanelWidth - 2 * Padding, 32, CreditSize, TvColors.InkSoft, false,
                TextAlignmentOptions.Center);
        }

        /// <summary>
        /// A volume slider (music D114, sounds D120): half of a row's button space, its caption with the value in percent
        /// above it. Drag or click with the mouse; every change applies at once.
        /// </summary>
        private UnityEngine.UI.Slider VolumeSlider(string name, float x, float y, float width, UnityEngine.Events.UnityAction<float> onChange,
            out TextMeshProUGUI caption)
        {
            caption = Label(name + "Caption", x, y + 2, width, 26, 22f, TvColors.InkSoft, true, TextAlignmentOptions.Left);
            var root = UiBuilder.Rect(name, _card);
            Box(root, x, y + 30, width, 40);
            // A groove cut in the card: ink, filled with teal up to the handle, a cream counter as the handle.
            var track = UiBuilder.Image("Track", root, TvColors.Ink, TvSprites.Solid);
            track.type = UnityEngine.UI.Image.Type.Sliced;
            track.pixelsPerUnitMultiplier = TvSprites.Radius / 8f;
            track.raycastTarget = true;
            UiBuilder.Anchor(track.rectTransform, 0, 0.32f, 1, 0.68f);

            var fillArea = UiBuilder.Rect("FillArea", root);
            UiBuilder.Anchor(fillArea, 0, 0.3f, 1, 0.7f);
            fillArea.offsetMin = new Vector2(0, 0);
            fillArea.offsetMax = new Vector2(-SliderHandleWidth / 2, 0);
            var fill = UiBuilder.Image("Fill", fillArea, TvColors.Teal, TvSprites.Solid);
            fill.type = UnityEngine.UI.Image.Type.Sliced;
            fill.pixelsPerUnitMultiplier = TvSprites.Radius / 8f;
            fill.rectTransform.sizeDelta = Vector2.zero;

            var handleArea = UiBuilder.Rect("HandleArea", root);
            UiBuilder.Anchor(handleArea, 0, 0, 1, 1);
            handleArea.offsetMin = new Vector2(SliderHandleWidth / 2, 0);
            handleArea.offsetMax = new Vector2(-SliderHandleWidth / 2, 0);
            var handleRect = UiBuilder.Rect("Handle", handleArea);
            handleRect.sizeDelta = new Vector2(SliderHandleWidth, 0);
            var counter = TvPiece.Disc("Counter", handleRect, TvColors.Mustard, 4f);
            UiBuilder.Anchor(counter.Root, 0f, 0.02f, 1f, 0.98f);
            counter.Body.raycastTarget = true;
            var handle = counter.Body;

            var slider = root.gameObject.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            slider.onValueChanged.AddListener(onChange);
            return slider;
        }

        /// <summary>A row's label in its upper part and a hidden "applies from the next game" note below it.</summary>
        private TextMeshProUGUI RowLabel(string name, float y, out TextMeshProUGUI note)
        {
            var label = Label(name, Padding, y, LabelWidth, 56, LabelSize, TvColors.Ink, true, TextAlignmentOptions.Left);
            note = Label(name + "Note", Padding, y + 48, LabelWidth, 28, NoteSize, TvColors.Red, true, TextAlignmentOptions.Left);
            note.gameObject.SetActive(false);
            return label;
        }

        /// <summary>One of the two choice buttons at the right of a row (column 0 or 1).</summary>
        private PanelButton RowButton(string name, float y, int column, Action onClick)
        {
            var button = PanelButton.Create(_card, name, ButtonLabelSize, onClick);
            float x = Padding + LabelWidth + 12 + column * (ButtonWidth + ButtonGap);
            Box(button.Rect, x, y + 10, ButtonWidth, ButtonHeight);
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
