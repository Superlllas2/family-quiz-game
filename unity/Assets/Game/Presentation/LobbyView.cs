using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The plain Day 1 lobby on the TV: room code, QR code, the joined players as character chips and the
    /// last input of each. Built from code; every text comes from ui_strings.json. Only reads state.
    /// Sizes are for a 1920 x 1080 canvas, large enough to read at 3 m.
    /// </summary>
    public sealed class LobbyView : MonoBehaviour
    {
        /// <summary>Answer letters A to D (docs/06_phone_screens.md), the same in both languages.</summary>
        private const string AnswerLetters = "ABCD";

        private const float QrTargetSize = 480f;

        private UiStrings _strings;
        private IReadOnlyList<CharacterDef> _characters;
        private string _lang = Core.Languages.Default;

        private RectTransform _roomPanel;
        private TextMeshProUGUI _roomCodeLabel;
        private TextMeshProUGUI _roomCode;
        private RawImage _qr;
        private TextMeshProUGUI _scanToJoin;
        private TextMeshProUGUI _relayDown;
        private TextMeshProUGUI _playersTitle;
        private RectTransform _playerList;
        private TextMeshProUGUI _contentError;
        private Texture2D _qrTexture;

        public static LobbyView Create()
        {
            var go = new GameObject("LobbyView");
            var view = go.AddComponent<LobbyView>();
            view.Build();
            return view;
        }

        public void Configure(UiStrings strings, IReadOnlyList<CharacterDef> characters, string lang)
        {
            _strings = strings;
            _characters = characters;
            _lang = lang;
            _roomCodeLabel.text = T("tv.lobby.roomCode");
            _scanToJoin.text = T("tv.lobby.scanToJoin");
            _relayDown.text = T("tv.error.relayDown");
            _playersTitle.text = T("tv.lobby.waiting");
        }

        public void ShowRoom(string code, string joinUrl)
        {
            _roomCode.text = code;
            if (_qrTexture != null) Destroy(_qrTexture);
            _qrTexture = QrTexture.Create(joinUrl);
            _qr.texture = _qrTexture;
            float module = Mathf.Max(1f, Mathf.Floor(QrTargetSize / _qrTexture.width));
            _qr.rectTransform.sizeDelta = new Vector2(module * _qrTexture.width, module * _qrTexture.width);
            _roomPanel.gameObject.SetActive(true);
            _relayDown.gameObject.SetActive(false);
        }

        /// <summary>While the relay socket is not open: the error text, no room code (docs/18_edge_cases.md).</summary>
        public void ShowRelayDown()
        {
            _roomPanel.gameObject.SetActive(false);
            _relayDown.gameObject.SetActive(true);
        }

        /// <summary>A broken config or content file: show the error instead of the lobby.</summary>
        public void ShowContentError(string message)
        {
            _roomPanel.gameObject.SetActive(false);
            _relayDown.gameObject.SetActive(false);
            _playersTitle.transform.parent.gameObject.SetActive(false);
            var title = _strings != null && _strings.Has("tv.error.content") ? T("tv.error.content") + "\n\n" : "";
            _contentError.text = title + message;
            _contentError.gameObject.SetActive(true);
        }

        public void Refresh(GameState state)
        {
            for (int i = _playerList.childCount - 1; i >= 0; i--) Destroy(_playerList.GetChild(i).gameObject);
            foreach (var player in state.Players) AddRow(player);
        }

        private void OnDestroy()
        {
            if (_qrTexture != null) Destroy(_qrTexture);
        }

        private string T(string key)
        {
            return _strings != null ? _strings.Get(key, _lang) : string.Empty;
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

            // Left half: room code and QR code.
            _roomPanel = UiBuilder.Anchor(UiBuilder.Rect("Room", root), 0, 0, 0.48f, 1);
            var roomLayout = _roomPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            roomLayout.childAlignment = TextAnchor.MiddleCenter;
            roomLayout.spacing = 12;
            roomLayout.padding = new RectOffset(60, 40, 40, 40);
            roomLayout.childControlWidth = true;
            roomLayout.childControlHeight = true;
            roomLayout.childForceExpandWidth = true;
            roomLayout.childForceExpandHeight = false;

            _roomCodeLabel = UiBuilder.Text("RoomCodeLabel", _roomPanel, 56, TvColors.DimText, false, TextAlignmentOptions.Center);
            UiBuilder.Size(_roomCodeLabel, -1, 70);
            _roomCode = UiBuilder.Text("RoomCode", _roomPanel, 200, TvColors.Text, true, TextAlignmentOptions.Center);
            _roomCode.characterSpacing = 20;
            _roomCode.textWrappingMode = TextWrappingModes.NoWrap;
            _roomCode.overflowMode = TextOverflowModes.Overflow;
            UiBuilder.Size(_roomCode, -1, 250);

            var qrHolder = UiBuilder.Rect("QrHolder", _roomPanel);
            UiBuilder.Size(qrHolder, -1, QrTargetSize + 20);
            _qr = UiBuilder.Rect("Qr", qrHolder).gameObject.AddComponent<RawImage>();
            _qr.raycastTarget = false;
            _qr.rectTransform.sizeDelta = new Vector2(QrTargetSize, QrTargetSize);

            _scanToJoin = UiBuilder.Text("ScanToJoin", _roomPanel, 40, TvColors.Text, false, TextAlignmentOptions.Center);
            UiBuilder.Size(_scanToJoin, -1, 110);

            _relayDown = UiBuilder.Text("RelayDown", root, 64, TvColors.Error, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_relayDown.rectTransform, 0.03f, 0.2f, 0.47f, 0.8f);

            // Right half: the players.
            var playersPanel = UiBuilder.Anchor(UiBuilder.Rect("Players", root), 0.5f, 0, 1, 1);
            var playersLayout = playersPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            playersLayout.childAlignment = TextAnchor.UpperLeft;
            playersLayout.spacing = 24;
            playersLayout.padding = new RectOffset(20, 60, 70, 40);
            playersLayout.childControlWidth = true;
            playersLayout.childControlHeight = true;
            playersLayout.childForceExpandWidth = true;
            playersLayout.childForceExpandHeight = false;

            _playersTitle = UiBuilder.Text("Title", playersPanel, 60, TvColors.Text, true, TextAlignmentOptions.Left);
            UiBuilder.Size(_playersTitle, -1, 80);

            _playerList = UiBuilder.Rect("List", playersPanel);
            var listLayout = _playerList.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 16;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;

            _contentError = UiBuilder.Text("ContentError", root, 40, TvColors.Error, false, TextAlignmentOptions.TopLeft);
            UiBuilder.Anchor(_contentError.rectTransform, 0.05f, 0.05f, 0.95f, 0.95f);
            _contentError.overflowMode = TextOverflowModes.Overflow;
            _contentError.gameObject.SetActive(false);

            ShowRelayDown();
        }

        private void AddRow(PlayerState player)
        {
            var row = UiBuilder.Rect("Player " + player.PlayerId, _playerList);
            UiBuilder.Size(row, -1, 88);
            var group = row.gameObject.AddComponent<CanvasGroup>();
            group.alpha = player.Connected ? 1f : 0.35f;
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 32;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            var chip = UiBuilder.Image("Chip", row, ChipColor(player.CharacterId), RoundedSprite.Get());
            UiBuilder.Size(chip, 540, 88);
            var name = UiBuilder.Text("Name", chip.transform, 50, TvColors.Background, true, TextAlignmentOptions.Left);
            name.textWrappingMode = TextWrappingModes.NoWrap;
            // Long names (up to 16 wide letters) shrink to fit the chip instead of being cut.
            name.enableAutoSizing = true;
            name.fontSizeMin = 32;
            name.fontSizeMax = 50;
            UiBuilder.Anchor(name.rectTransform, 0, 0, 1, 1);
            name.rectTransform.offsetMin = new Vector2(32, 0);
            name.rectTransform.offsetMax = new Vector2(-24, 0);
            name.text = player.Name;

            var input = UiBuilder.Text("LastInput", row, 50, TvColors.Text, false, TextAlignmentOptions.Left);
            input.textWrappingMode = TextWrappingModes.NoWrap;
            UiBuilder.Size(input, 300, 88);
            input.text = LastInputText(player.LastAnswer);
        }

        private string LastInputText(AnswerRecord answer)
        {
            if (answer == null || !answer.Choice.HasValue) return string.Empty;
            int choice = answer.Choice.Value;
            var letter = choice >= 0 && choice < AnswerLetters.Length ? AnswerLetters[choice].ToString() : "?";
            var values = new Dictionary<string, string>
            {
                { "letter", letter },
                { "ms", answer.Ms.HasValue ? answer.Ms.Value.ToString() : "-" }
            };
            return _strings != null ? _strings.Format("tv.lobby.lastInput", _lang, values) : letter;
        }

        private Color ChipColor(int characterId)
        {
            if (_characters != null)
            {
                foreach (var c in _characters)
                {
                    if (c.Id == characterId) return TvColors.FromHex(c.Color, TvColors.Text);
                }
            }

            return TvColors.Text;
        }
    }
}
