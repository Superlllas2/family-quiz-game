using System;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The question bank chooser (decisions/0020, D116, D118): a test tool, not part of the finished game. When the app
    /// starts (banks.chooseOnStart) and from the pause menu, the person at the laptop clicks which bank the next game
    /// plays. Drawn like the topic vote (D100): two topic cards of card stock with a coloured header, dealt a little
    /// crooked on the dimmed table under a printed ribbon; the chosen one wears a green check. Phones can join while it
    /// is up; Escape closes it and keeps the current choice. It sends ChangeTvSettings and never changes state itself.
    /// </summary>
    public sealed class TvBankChooser : MonoBehaviour
    {
        private const float TileWidth = 520f;
        private const float TileHeight = 330f;
        private const float TileGap = 70f;
        private const float HeaderHeight = 120f;

        private static readonly Color Backdrop = new Color(0.08f, 0.05f, 0.03f, 0.7f);
        private static readonly string[] Ids = { BankIds.Full, BankIds.Curated };
        private static readonly Color[] Headers = { TvColors.Cobalt, TvColors.Mustard };
        private static readonly float[] Tilts = { 1.6f, -1.4f };

        private readonly TvPiece[] _tiles = new TvPiece[2];
        private readonly TextMeshProUGUI[] _names = new TextMeshProUGUI[2];
        private readonly TextMeshProUGUI[] _counts = new TextMeshProUGUI[2];
        private readonly GameObject[] _checks = new GameObject[2];
        private TvContext _context;
        private Action<IGameCommand> _send;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _note;

        public static TvBankChooser Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TvBankChooser", parent), 0, 0, 1, 1);
            var chooser = rect.gameObject.AddComponent<TvBankChooser>();
            chooser._context = context;
            chooser.Build();
            rect.gameObject.SetActive(false);
            return chooser;
        }

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Both banks were loaded, so there is something to choose.</summary>
        public bool HasChoice => _context.Game != null && _context.Game.Content.Banks.Count > 1;

        public void SetCommandSink(Action<IGameCommand> send)
        {
            _send = send;
        }

        public void SetOpen(bool open)
        {
            if (open && !HasChoice) open = false;
            // No TvTween pop here: its canvas group turns clicks off (it is made for pieces that are only looked at).
            gameObject.SetActive(open);
            if (open) Refresh();
        }

        /// <summary>Draws the texts, the size of each bank and which one the next game plays.</summary>
        public void Refresh()
        {
            var game = _context.Game;
            if (game == null) return;
            _title.text = _context.T("tv.bank.title");
            for (int i = 0; i < Ids.Length; i++)
            {
                var bank = game.Content.Bank(Ids[i]);
                bool selected = game.State.PendingSettings.Bank == Ids[i];
                _names[i].text = _context.T("tv.bank." + Ids[i]);
                int topics = bank.Topics.Count(t => !t.Mixed);
                _counts[i].text = _context.Format("tv.bank.count", "questions", bank.Questions.Count.ToString(), "topics", topics.ToString());
                _checks[i].SetActive(selected);
                _tiles[i].SetEdge(selected ? 12f : 7f);
            }

            bool running = game.State.Phase != GamePhase.Lobby;
            _note.gameObject.SetActive(running);
            if (running) _note.text = _context.T("tv.settings.nextGame");
        }

        private void Choose(int index)
        {
            _send?.Invoke(new ChangeTvSettings(bank: Ids[index]));
            SetOpen(false);
        }

        private void Build()
        {
            var backdrop = UiBuilder.Image("Backdrop", transform, Backdrop);
            backdrop.raycastTarget = true;
            UiBuilder.Anchor(backdrop.rectTransform, 0, 0, 1, 1);

            // The question on a printed ribbon above the cards.
            var ribbon = TvPiece.Card("Ribbon", transform, TvColors.Red, 16f, 7f).Tilt(-1.5f);
            var rr = ribbon.Root;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0.5f);
            rr.anchoredPosition = new Vector2(0, 285);
            rr.sizeDelta = new Vector2(980, 120);
            _title = UiBuilder.Display("Title", ribbon.Content, 56, TvColors.TextOn(TvColors.Red), TextAlignmentOptions.Center);
            UiBuilder.Fit(_title, 28, 56, 1);
            UiBuilder.Anchor(_title.rectTransform, 0.04f, 0.1f, 0.96f, 0.9f);

            for (int i = 0; i < Ids.Length; i++)
            {
                int index = i;
                var tile = TvPiece.Card("Tile " + Ids[i], transform, TvColors.Cream, 22f, 7f).Tilt(Tilts[i]);
                var root = tile.Root;
                root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
                root.anchoredPosition = new Vector2((i == 0 ? -1 : 1) * (TileWidth + TileGap) / 2, -40);
                root.sizeDelta = new Vector2(TileWidth, TileHeight);
                tile.Body.raycastTarget = true;
                var button = tile.Body.gameObject.AddComponent<Button>();
                button.targetGraphic = tile.Body;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() =>
                {
                    if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                    Choose(index);
                });
                _tiles[i] = tile;

                // A coloured header with the bank's name, like a topic card in the vote.
                var header = TvPiece.Card("Header", tile.Content, Headers[i], 18f, 0f);
                UiBuilder.Anchor(header.Root, 0, 1, 1, 1);
                header.Root.pivot = new Vector2(0.5f, 1f);
                header.Root.anchoredPosition = Vector2.zero;
                header.Root.sizeDelta = new Vector2(0, HeaderHeight);
                header.Body.raycastTarget = false;
                _names[i] = UiBuilder.Display("Name", header.Content, 50, TvColors.TextOn(Headers[i]), TextAlignmentOptions.Center);
                UiBuilder.Fit(_names[i], 26, 50, 1);
                UiBuilder.Anchor(_names[i].rectTransform, 0.05f, 0.1f, 0.95f, 0.9f);

                _counts[i] = UiBuilder.Text("Count", tile.Content, 40, TvColors.Ink, true, TextAlignmentOptions.Center);
                UiBuilder.Fit(_counts[i], 22, 40, 2);
                UiBuilder.Anchor(_counts[i].rectTransform, 0.06f, 0.12f, 0.94f, 0.58f);
                _counts[i].raycastTarget = false;

                // The chosen bank wears a green check counter on its corner.
                var check = TvPiece.Disc("Check", tile.Content, TvColors.Green, 5f);
                var cr = check.Root;
                cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f);
                cr.pivot = new Vector2(0.5f, 0.5f);
                cr.anchoredPosition = new Vector2(-14, -14);
                cr.sizeDelta = new Vector2(84, 84);
                check.Body.raycastTarget = false;
                var mark = UiBuilder.Image("Mark", check.Content, TvColors.Cream, TvSprites.Check);
                mark.raycastTarget = false;
                UiBuilder.Anchor(mark.rectTransform, 0.2f, 0.2f, 0.8f, 0.8f);
                _checks[i] = cr.gameObject;
            }

            _note = UiBuilder.Hand("Note", transform, 44, TvColors.Cream, TextAlignmentOptions.Center);
            var nr = _note.rectTransform;
            nr.anchorMin = nr.anchorMax = nr.pivot = new Vector2(0.5f, 0.5f);
            nr.anchoredPosition = new Vector2(0, -280);
            nr.sizeDelta = new Vector2(900, 60);
        }
    }
}
