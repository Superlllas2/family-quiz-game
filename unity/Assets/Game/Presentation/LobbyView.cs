using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The Lobby on the TV: room code and QR code (or the relay down text instead), the seated players as character
    /// chips (dimmed while disconnected), and "Tap Start on any phone" once Start is possible. Only reads state.
    /// Sizes are for a 1920 x 1080 canvas, large enough to read at 3 m.
    /// </summary>
    public sealed class LobbyView : MonoBehaviour
    {
        private const float QrTargetSize = 480f;
        private const float ChipWidth = 620f;
        private const float ChipHeight = 76f;

        private readonly List<CharacterChip> _chips = new List<CharacterChip>();
        private TvContext _context;
        private RectTransform _roomPanel;
        private TextMeshProUGUI _roomCodeLabel;
        private TextMeshProUGUI _roomCode;
        private RawImage _qr;
        private TextMeshProUGUI _scanToJoin;
        private TextMeshProUGUI _relayDown;
        private TextMeshProUGUI _playersTitle;
        private RectTransform _playerList;
        private TextMeshProUGUI _tapStart;
        private Texture2D _qrTexture;
        private bool _hasRoom;
        private bool _relayIsDown = true;

        public static LobbyView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("LobbyView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<LobbyView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Static labels in the current game language.</summary>
        public void Relabel()
        {
            _roomCodeLabel.text = _context.T("tv.lobby.roomCode");
            _scanToJoin.text = _context.T("tv.lobby.scanToJoin");
            _relayDown.text = _context.T("tv.error.relayDown");
            _playersTitle.text = _context.T("tv.lobby.waiting");
            _tapStart.text = _context.T("tv.lobby.tapStart");
        }

        public void ShowRoom(string code, string joinUrl)
        {
            _roomCode.text = code;
            if (_qrTexture != null) Destroy(_qrTexture);
            _qrTexture = QrTexture.Create(joinUrl);
            _qr.texture = _qrTexture;
            float module = Mathf.Max(1f, Mathf.Floor(QrTargetSize / _qrTexture.width));
            _qr.rectTransform.sizeDelta = new Vector2(module * _qrTexture.width, module * _qrTexture.width);
            _hasRoom = true;
            SetRelayDown(false);
        }

        /// <summary>While the relay socket is not open: the error text, no room code (docs/18_edge_cases.md).</summary>
        public void SetRelayDown(bool down)
        {
            _relayIsDown = down;
            _roomPanel.gameObject.SetActive(!down && _hasRoom);
            _relayDown.gameObject.SetActive(down);
        }

        /// <summary>Rebuilds the player chips from the roster.</summary>
        public void Refresh()
        {
            foreach (var chip in _chips) Destroy(chip.gameObject);
            _chips.Clear();
            var game = _context.Game;
            if (game == null) return;
            foreach (var player in game.State.Players)
            {
                var chip = CharacterChip.Create(_playerList, player.PlayerId, 48);
                UiBuilder.Size(chip, ChipWidth, ChipHeight);
                chip.Set(player.Name, _context.ChipColor(player.CharacterId), player.Connected);
                _chips.Add(chip);
            }

            Tick();
        }

        public void Tick()
        {
            var game = _context.Game;
            _tapStart.gameObject.SetActive(game != null && game.CanStart && !_relayIsDown);
        }

        private void OnDestroy()
        {
            if (_qrTexture != null) Destroy(_qrTexture);
        }

        private void Build()
        {
            var root = (RectTransform)transform;

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
            var playersPanel = UiBuilder.Anchor(UiBuilder.Rect("Players", root), 0.5f, 0.14f, 1, 1);
            var playersLayout = playersPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            playersLayout.childAlignment = TextAnchor.UpperLeft;
            playersLayout.spacing = 24;
            playersLayout.padding = new RectOffset(20, 60, 70, 0);
            playersLayout.childControlWidth = true;
            playersLayout.childControlHeight = true;
            playersLayout.childForceExpandWidth = true;
            playersLayout.childForceExpandHeight = false;

            _playersTitle = UiBuilder.Text("Title", playersPanel, 60, TvColors.Text, true, TextAlignmentOptions.Left);
            UiBuilder.Size(_playersTitle, -1, 80);

            _playerList = UiBuilder.Rect("List", playersPanel);
            var listLayout = _playerList.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 12;
            listLayout.childAlignment = TextAnchor.UpperLeft;
            listLayout.childControlWidth = true;
            listLayout.childControlHeight = true;
            listLayout.childForceExpandWidth = false;
            listLayout.childForceExpandHeight = false;

            _tapStart = UiBuilder.Text("TapStart", root, 56, TvColors.Text, true, TextAlignmentOptions.Left);
            UiBuilder.Anchor(_tapStart.rectTransform, 0.51f, 0.03f, 0.98f, 0.13f);
            _tapStart.gameObject.SetActive(false);

            SetRelayDown(true);
        }
    }
}
