using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Intro (docs/02_show_flow.md): the title card and the prediction hint "Who wins tonight?". Each player's chip gets
    /// its rim and a pop once they picked on their phone, never who they picked.
    /// </summary>
    public sealed class IntroView : MonoBehaviour
    {
        private const float ChipHeight = 86f;

        private TvContext _context;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private ChipRow _chips;

        public static IntroView Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("IntroView", parent), 0, 0, 1, 1);
            var view = rect.gameObject.AddComponent<IntroView>();
            view._context = context;
            view.Build();
            return view;
        }

        public void Enter()
        {
            Refresh();
        }

        /// <summary>Texts (a language change) and chips (roster, connection, predictions).</summary>
        public void Refresh()
        {
            var state = _context.Game.State;
            _title.text = _context.T("tv.intro.title");
            _hint.text = _context.T("tv.intro.prediction");
            _chips.Sync(state, ChipHeight);
            for (int i = 0; i < state.Players.Count && i < _chips.Chips.Count; i++)
            {
                _chips.Chips[i].SetLocked(state.Predictions.ContainsKey(state.Players[i].PlayerId));
            }
        }

        public void OnPredictionMade(string playerId)
        {
            var chip = _chips.Find(playerId);
            if (chip == null) return;
            chip.SetLocked(true);
            chip.Pop();
        }

        private void Build()
        {
            _title = UiBuilder.Text("Title", transform, 140, TvColors.Text, true, TextAlignmentOptions.Center);
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 96;
            _title.fontSizeMax = 140;
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.5f, 0.95f, 0.8f);

            _hint = UiBuilder.Text("Prediction", transform, 64, TvColors.DimText, true, TextAlignmentOptions.Center);
            UiBuilder.Anchor(_hint.rectTransform, 0.05f, 0.3f, 0.95f, 0.45f);

            _chips = ChipRow.Create(transform, _context);
            UiBuilder.Anchor((RectTransform)_chips.transform, 0.02f, 0.06f, 0.98f, 0.14f);
        }
    }
}
