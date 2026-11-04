using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The Lobby on the TV: room code and QR code (or the relay down text instead), the seated players as character
    /// chips (dimmed while disconnected) with a white rim and "Ready" once they tapped Ready (D76), and "Everyone taps
    /// Ready on their phone" once enough players are connected. Only reads state.
    /// Sizes are for a 1920 x 1080 canvas, large enough to read at 3 m.
    /// </summary>
    public sealed class LobbyView : MonoBehaviour
    {
        private const float QrTargetSize = 480f;
        private const float ChipWidth = 620f;
        private const float ChipHeight = 76f;
        private const float ReadyWidth = 240f;

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
        private readonly List<GameObject> _rows = new List<GameObject>();
        private TextMeshProUGUI _tapReady;
        private Image _roomBackdrop;
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
            _tapReady.text = _context.T("tv.lobby.tapReady");
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

        /// <summary>
        /// With the 3D studio behind the overlay (decisions 0016, D91) the seats show the players: the room code and QR
        /// code shrink into the top left corner on a dark backing and the player list hides; "Waiting for players" and
        /// "tap ready" stay. Off, the flat layout comes back.
        /// </summary>
        public void SetStudioMode(bool on)
        {
            _roomPanel.pivot = new Vector2(0, 1);
            UiBuilder.Anchor(_roomPanel, 0, on ? 0.3f : 0f, 0.48f, 1);
            _roomPanel.localScale = Vector3.one * (on ? 0.56f : 1f);
            _roomBackdrop.enabled = on;
            _playerList.gameObject.SetActive(!on);
        }

        /// <summary>While the relay socket is not open: the error text, no room code (docs/18_edge_cases.md).</summary>
        public void SetRelayDown(bool down)
        {
            _relayIsDown = down;
            _roomPanel.gameObject.SetActive(!down && _hasRoom);
            _relayDown.gameObject.SetActive(down);
        }

        /// <summary>Rebuilds the player rows (chip and ready mark) from the roster.</summary>
        public void Refresh()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();
            _chips.Clear();
            var game = _context.Game;
            if (game == null) return;
            foreach (var player in game.State.Players)
            {
                var row = UiBuilder.Rect("Row " + player.PlayerId, _playerList);
                UiBuilder.Size(row, ChipWidth + ReadyWidth, ChipHeight);
                var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 24;
                rowLayout.childAlignment = TextAnchor.MiddleLeft;
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = true;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;

                var chip = CharacterChip.Create(row, player.PlayerId, 48);
                UiBuilder.Size(chip, ChipWidth, ChipHeight);
                chip.Set(player.Name, _context.ChipColor(player.CharacterId), player.Connected);
                chip.SetLocked(player.Ready);

                var ready = UiBuilder.Text("Ready", row, 48, TvColors.Correct, true, TextAlignmentOptions.Left);
                ready.textWrappingMode = TextWrappingModes.NoWrap;
                UiBuilder.Size(ready, ReadyWidth, ChipHeight);
                ready.text = player.Ready ? _context.T("tv.lobby.ready") : "";

                _rows.Add(row.gameObject);
                _chips.Add(chip);
            }

            Tick();
        }

        public void Tick()
        {
            var game = _context.Game;
            _tapReady.gameObject.SetActive(game != null && game.EnoughPlayers && !_relayIsDown);
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

            _roomBackdrop = UiBuilder.Image("Backdrop", _roomPanel, new Color(0, 0, 0, 0.55f), RoundedSprite.Get());
            UiBuilder.Anchor(_roomBackdrop.rectTransform, 0.04f, 0.04f, 0.96f, 0.96f);
            _roomBackdrop.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _roomBackdrop.enabled = false;

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

            _tapReady = UiBuilder.Text("TapReady", root, 56, TvColors.Text, true, TextAlignmentOptions.Left);
            UiBuilder.Anchor(_tapReady.rectTransform, 0.51f, 0.03f, 0.98f, 0.13f);
            _tapReady.gameObject.SetActive(false);

            SetRelayDown(true);
        }
    }
}
