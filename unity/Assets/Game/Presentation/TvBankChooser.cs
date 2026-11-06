using System;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The question bank chooser (decisions/0020, D116, D118): a test tool, not part of the finished game. When the app
    /// starts (banks.chooseOnStart) and from the pause menu, the person at the laptop clicks which bank the next game
    /// plays: every question or the curated test bank. Phones can join while it is up; Escape closes it and keeps the
    /// current choice. It sends ChangeTvSettings like the settings panel and never changes state itself.
    /// </summary>
    public sealed class TvBankChooser : MonoBehaviour
    {
        private const float CardWidth = 1240f;
        private const float CardHeight = 560f;
        private const float Padding = 48f;
        private const float TileGap = 40f;
        private const float TileTop = 170f;
        private const float TileHeight = 300f;

        private static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.7f);

        private TvContext _context;
        private Action<IGameCommand> _send;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _note;
        private readonly PanelButton[] _tiles = new PanelButton[2];
        private readonly TextMeshProUGUI[] _counts = new TextMeshProUGUI[2];
        private static readonly string[] Ids = { BankIds.Full, BankIds.Curated };

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
                _tiles[i].Set(_context.T("tv.bank." + Ids[i]), selected ? TvColors.ButtonOn : TvColors.ButtonOff);
                int topics = bank.Topics.Count(t => !t.Mixed);
                _counts[i].text = _context.Format("tv.bank.count", "questions", bank.Questions.Count.ToString(), "topics", topics.ToString());
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

            var opaque = new Color(TvColors.Panel.r, TvColors.Panel.g, TvColors.Panel.b, 1f);
            var card = UiBuilder.Image("Card", transform, opaque, RoundedSprite.Get()).rectTransform;
            card.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(CardWidth, CardHeight);

            _title = Label(card, "Title", Padding, 40, CardWidth - 2 * Padding, 90, 64f, TvColors.Text, true);
            float tileWidth = (CardWidth - 2 * Padding - TileGap) / 2;
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                float x = Padding + i * (tileWidth + TileGap);
                _tiles[i] = PanelButton.Create(card, "Tile " + Ids[i], 56f, () => Choose(index));
                Box(_tiles[i].Rect, x, TileTop, tileWidth, TileHeight);
                _counts[i] = Label(_tiles[i].Rect, "Count", 0, TileHeight - 90, tileWidth, 60, 32f, TvColors.DimText, false);
                _counts[i].raycastTarget = false;
            }

            _note = Label(card, "Note", Padding, TileTop + TileHeight + 24, CardWidth - 2 * Padding, 44, 30f, TvColors.Crown, false);
        }

        private static TextMeshProUGUI Label(RectTransform parent, string name, float x, float y, float width, float height,
            float size, Color color, bool bold)
        {
            var text = UiBuilder.Text(name, parent, size, color, bold, TextAlignmentOptions.Center);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(20f, size);
            text.fontSizeMax = size;
            Box(text.rectTransform, x, y, width, height);
            return text;
        }

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
