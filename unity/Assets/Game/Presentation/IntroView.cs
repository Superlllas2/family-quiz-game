using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Intro (docs/02_show_flow.md, decisions/0017_game_night_ui.md): the welcome on the title ribbon and every player's
    /// name piece along the bottom. The Intro prediction was dropped (D85).
    /// </summary>
    public sealed class IntroView : MonoBehaviour
    {
        private const float ChipHeight = 76f;

        private TvContext _context;
        private TitleRibbon _title;
        private ChipRow _chips;

        public static IntroView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("IntroView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<IntroView>();
            view._context = context;
            view.Build();
            return view;
        }

        /// <summary>Kept for TvPresenter: the layout is the same with the 3D studio on or off.</summary>
        public void SetStudioMode(bool on)
        {
        }

        public void Enter()
        {
            Refresh();
            _title.Rise();
        }

        /// <summary>Texts (a language change) and chips (roster, connection).</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _title.Set(_context.T("tv.intro.title"), null);
            _chips.Sync(state, ChipHeight);
        }

        private void Build()
        {
            _title = TitleRibbon.Create(transform, TvColors.Mustard);
            _chips = ChipRow.Create(transform, _context);
            UiBuilder.Anchor((RectTransform)_chips.transform, 0.02f, 0.04f, 0.98f, 0.115f);
        }
    }
}
