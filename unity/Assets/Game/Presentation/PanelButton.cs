using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A mouse button of the TV's menus (settings panel, pause menu, bank chooser): a board game piece
    /// (decisions/0017, D100) of card stock or printed ink with an ink outline and a solid bottom edge, its label in ink
    /// or cream as the answer inks have it (D102). A selected choice is pressed flat into the table. It never keeps the keyboard focus, so
    /// a later Enter or Space on the laptop cannot click it again.
    /// </summary>
    public sealed class PanelButton
    {
        private const float LabelPadding = 12f;
        private const float LabelMinSize = 22f;
        private const float RaisedEdge = 6f;
        private const float PressedEdge = 2f;

        private readonly TvPiece _piece;
        private readonly TextMeshProUGUI _label;

        private PanelButton(TvPiece piece, TextMeshProUGUI label)
        {
            _piece = piece;
            _label = label;
        }

        /// <summary>The piece's root: size and place it like any rect.</summary>
        public RectTransform Rect => _piece.Root;

        public static PanelButton Create(Transform parent, string name, float labelSize, Action onClick)
        {
            var piece = TvPiece.Card(name, parent, TvColors.ButtonOff, 14f, RaisedEdge);
            var body = piece.Body;
            body.raycastTarget = true;
            var button = body.gameObject.AddComponent<Button>();
            button.targetGraphic = body;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f),
                pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.7f, 0.7f, 0.7f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() =>
            {
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                onClick?.Invoke();
            });

            var label = UiBuilder.Text("Label", piece.Content, labelSize, TvColors.Ink, true, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Min(LabelMinSize, labelSize);
            label.fontSizeMax = labelSize;
            label.raycastTarget = false;
            UiBuilder.Anchor(label.rectTransform, 0, 0, 1, 1);
            label.rectTransform.offsetMin = new Vector2(LabelPadding, 0);
            label.rectTransform.offsetMax = new Vector2(-LabelPadding, 0);
            return new PanelButton(piece, label);
        }

        /// <summary>The label and the fill; the label takes ink or cream to read on it.</summary>
        public void Set(string label, Color color)
        {
            _label.text = label ?? string.Empty;
            _piece.SetFill(color);
            _label.color = LabelOn(color);
        }

        /// <summary>The answer inks' rule (D102): ink on card stock and mustard, cream on every other printed colour.</summary>
        private static Color LabelOn(Color fill)
        {
            if (fill == TvColors.Mustard || fill == TvColors.Cream || fill == TvColors.Cream2) return TvColors.Ink;
            return TvColors.Cream;
        }

        /// <summary>A selected choice sits pressed flat; the others stand on their edge.</summary>
        public void SetPressed(bool pressed)
        {
            _piece.SetEdge(pressed ? PressedEdge : RaisedEdge);
        }
    }
}
