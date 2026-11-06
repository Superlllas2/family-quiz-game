using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The Lobby on the TV (decisions/0017_game_night_ui.md): the room code printed on a big cinema ticket on the floor
    /// under the seats, the QR code on its stub, and the tokens of the seated players along its bottom edge (half see
    /// through while disconnected, a check once they tapped Ready, D76) with a "3 of 8" sticker. The title ribbon at the
    /// top right says "Waiting for players", and "Everyone taps Ready on their phone" once enough players are
    /// connected. With the relay down the ticket shows the error instead of the code (docs/18_edge_cases.md). The same
    /// layout with the 3D studio on or off: the seats above the ticket show the players. Only reads state.
    /// </summary>
    public sealed class LobbyView : MonoBehaviour
    {
        private const float QrTargetSize = 236f;
        private const float TokenSize = 54f;
        private const float TokenGap = 10f;

        private readonly List<PlayerToken> _tokens = new List<PlayerToken>();
        private TvContext _context;
        private TvPiece _ticket;
        private TextMeshProUGUI _top;
        private TextMeshProUGUI _roomCode;
        private TextMeshProUGUI _scanToJoin;
        private TextMeshProUGUI _relayDown;
        private RawImage _qr;
        private RectTransform _tokenRow;
        private TvPiece _count;
        private TextMeshProUGUI _countText;
        private TitleRibbon _title;
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
            _top.text = _context.T("tv.lobby.ticketTop");
            _scanToJoin.text = _context.T("tv.lobby.scanToJoin");
            _relayDown.text = _context.T("tv.error.relayDown");
            Tick();
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

        /// <summary>Kept for TvPresenter: the ticket sits under the seats with the 3D studio on or off.</summary>
        public void SetStudioMode(bool on)
        {
        }

        /// <summary>While the relay socket is not open: the error text, no room code (docs/18_edge_cases.md).</summary>
        public void SetRelayDown(bool down)
        {
            _relayIsDown = down;
            bool room = !down && _hasRoom;
            _roomCode.gameObject.SetActive(room);
            _scanToJoin.gameObject.SetActive(room);
            _qr.gameObject.SetActive(room);
            _relayDown.gameObject.SetActive(down);
            Tick();
        }

        /// <summary>The seated players' tokens from the roster: letters, colours, connection, Ready.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            if (game == null) return;
            var players = game.State.Players;
            bool same = players.Count == _tokens.Count;
            for (int i = 0; same && i < players.Count; i++) same = players[i].PlayerId == _tokens[i].PlayerId;
            if (!same)
            {
                var before = new HashSet<string>();
                foreach (var t in _tokens) before.Add(t.PlayerId);
                foreach (var t in _tokens) Destroy(t.gameObject);
                _tokens.Clear();
                foreach (var p in players)
                {
                    var token = PlayerToken.Create(_tokenRow, p.PlayerId);
                    _tokens.Add(token);
                    if (!before.Contains(p.PlayerId)) TvTween.On(token.Rect).Pop(0f, 0.4f);
                }
            }

            var names = new string[players.Count];
            for (int i = 0; i < names.Length; i++) names[i] = players[i].Name;
            var labels = TokenLabels.For(names);
            for (int i = 0; i < players.Count; i++)
            {
                var rect = _tokens[i].Rect;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(i * (TokenSize + TokenGap), 0f);
                rect.sizeDelta = new Vector2(TokenSize, TokenSize);
                _tokens[i].Set(labels[i], _context.ChipColor(players[i].CharacterId), players[i].Connected);
                _tokens[i].SetLocked(players[i].Ready);
            }

            _countText.text = _context.Format("tv.lobby.players", "count", players.Count.ToString(), "max",
                game.Config.Players.Max.ToString());
            _count.Root.gameObject.SetActive(players.Count > 0);
            Tick();
        }

        public void Tick()
        {
            var game = _context.Game;
            bool ready = game != null && game.EnoughPlayers && !_relayIsDown;
            string subtitle = ready ? _context.T("tv.lobby.tapReady") : null;
            string title = _context.T("tv.lobby.waiting");
            if (title != _shownTitle || subtitle != _shownSubtitle)
            {
                _shownTitle = title;
                _shownSubtitle = subtitle;
                _title.Set(title, subtitle);
            }
        }

        private string _shownTitle;
        private string _shownSubtitle;

        private void OnDestroy()
        {
            if (_qrTexture != null) Destroy(_qrTexture);
        }

        private void Build()
        {
            var zone = TvLayout.Put(UiBuilder.Rect("TicketZone", transform), TvLayout.Ticket);
            _ticket = TvPiece.Card("Ticket", zone, TvColors.Cream, 22f, 10f).Tilt(-1.5f);
            UiBuilder.Anchor(_ticket.Root, 0.02f, 0.04f, 0.98f, 0.96f);
            var body = _ticket.Content;

            // The stub on the right: mustard, behind a dashed tear line, with the QR code on a white square.
            var stub = UiBuilder.Image("Stub", body, TvColors.Mustard, TvSprites.Solid);
            stub.pixelsPerUnitMultiplier = TvSprites.Radius / 18f;
            UiBuilder.Anchor(stub.rectTransform, 0.735f, 0f, 1f, 1f);
            stub.rectTransform.offsetMin = new Vector2(0f, 5f);
            stub.rectTransform.offsetMax = new Vector2(-5f, -5f);
            for (int i = 0; i < 12; i++)
            {
                var dash = UiBuilder.Image("Dash " + i, body, TvColors.Ink);
                var d = dash.rectTransform;
                d.anchorMin = d.anchorMax = new Vector2(0.735f, (i + 0.5f) / 12f);
                d.sizeDelta = new Vector2(4f, 12f);
            }

            var qrBack = UiBuilder.Image("QrBack", stub.transform, Color.white, TvSprites.Solid);
            qrBack.pixelsPerUnitMultiplier = 3f;
            var qb = qrBack.rectTransform;
            qb.anchorMin = qb.anchorMax = new Vector2(0.5f, 0.5f);
            qb.sizeDelta = new Vector2(QrTargetSize + 20f, QrTargetSize + 20f);
            _qr = UiBuilder.Rect("Qr", qrBack.transform).gameObject.AddComponent<RawImage>();
            _qr.raycastTarget = false;
            _qr.rectTransform.sizeDelta = new Vector2(QrTargetSize, QrTargetSize);

            _top = UiBuilder.Text("Top", body, 24, TvColors.Red, true, TextAlignmentOptions.Left);
            UiBuilder.Fit(_top, 14, 24, 1);
            _top.characterSpacing = 8f;
            UiBuilder.Anchor(_top.rectTransform, 0.04f, 0.8f, 0.7f, 0.94f);

            _roomCode = UiBuilder.Display("RoomCode", body, 132, TvColors.Ink, TextAlignmentOptions.Left);
            UiBuilder.Fit(_roomCode, 60, 132, 1);
            _roomCode.characterSpacing = 10f;
            UiBuilder.Anchor(_roomCode.rectTransform, 0.04f, 0.38f, 0.7f, 0.82f);

            _scanToJoin = UiBuilder.Text("ScanToJoin", body, 26, TvColors.Ink, true, TextAlignmentOptions.Left);
            UiBuilder.Fit(_scanToJoin, 16, 26, 1);
            UiBuilder.Anchor(_scanToJoin.rectTransform, 0.04f, 0.27f, 0.7f, 0.39f);

            _relayDown = UiBuilder.Display("RelayDown", body, 44, TvColors.Red, TextAlignmentOptions.Left);
            UiBuilder.Fit(_relayDown, 24, 44, 3);
            UiBuilder.Anchor(_relayDown.rectTransform, 0.04f, 0.3f, 0.7f, 0.82f);

            _tokenRow = UiBuilder.Anchor(UiBuilder.Rect("Players", body), 0.04f, 0.04f, 0.7f, 0.26f);

            _count = TvPiece.Star("Count", zone, TvColors.Teal);
            var c = _count.Root;
            c.anchorMin = c.anchorMax = new Vector2(0.735f, 1f);
            c.pivot = new Vector2(0.5f, 0.5f);
            c.anchoredPosition = new Vector2(-6f, -18f);
            c.sizeDelta = new Vector2(104f, 104f);
            c.localRotation = Quaternion.Euler(0, 0, -10f);
            TvTween.On(c).Tilt = -10f;
            _countText = UiBuilder.Display("Text", _count.Content, 22, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_countText, 12, 22, 2);
            UiBuilder.Anchor(_countText.rectTransform, 0.18f, 0.22f, 0.82f, 0.78f);
            _count.Root.gameObject.SetActive(false);

            _title = TitleRibbon.Create(transform, TvColors.Teal);
            SetRelayDown(true);
        }
    }
}
