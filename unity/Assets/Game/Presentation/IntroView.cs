using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Intro (docs/02_show_flow.md): the title card and every player's chip. The Intro prediction was dropped (D85).
    /// </summary>
    public sealed class IntroView : MonoBehaviour
    {
        private const float ChipHeight = 86f;

        private TvContext _context;
        private TextMeshProUGUI _title;
        private ChipRow _chips;

        public static IntroView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("IntroView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<IntroView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>With the 3D studio on (decisions 0016) the title moves up out of the way of the host solo shot.</summary>
        public void SetStudioMode(bool on)
        {
            UiBuilder.Anchor(_title.rectTransform, 0.05f, on ? 0.8f : 0.5f, 0.95f, on ? 0.97f : 0.8f);
        }

        public void Enter()
        {
            Refresh();
        }

        /// <summary>Texts (a language change) and chips (roster, connection).</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _title.text = _context.T("tv.intro.title");
            _chips.Sync(state, ChipHeight);
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 140, TvColors.Text, true, TextAlignmentOptions.Center);
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 96;
            _title.fontSizeMax = 140;
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.5f, 0.95f, 0.8f);

            _chips = ChipRow.Create(transform, _context);
            UiBuilder.Anchor((RectTransform)_chips.transform, 0.02f, 0.06f, 0.98f, 0.14f);
        }
    }
}
