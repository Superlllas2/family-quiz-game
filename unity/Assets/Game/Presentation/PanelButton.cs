using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A mouse button of the TV settings panel: a rounded card with a label. It never keeps the keyboard focus, so a
    /// later Enter or Space on the laptop cannot click it again.
    /// </summary>
    public sealed class PanelButton
    {
        private const float LabelPadding = 12f;
        private const float LabelMinSize = 22f;

        private readonly Image _image;
        private readonly TextMeshProUGUI _label;

        private PanelButton(RectTransform rect, Image image, TextMeshProUGUI label)
        {
            Rect = rect;
            _image = image;
            _label = label;
        }

        public RectTransform Rect { get; }

        public static PanelButton Create(Transform parent, string name, float labelSize, Action onClick)
        {
            var image = UiBuilder.Image(name, parent, TvColors.ButtonOff, RoundedSprite.Get());
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() =>
            {
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                onClick?.Invoke();
            });

            var label = UiBuilder.Text("Label", image.transform, labelSize, TvColors.Text, true, TextAlignmentOptions.Center);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Min(LabelMinSize, labelSize);
            label.fontSizeMax = labelSize;
            UiBuilder.Anchor(label.rectTransform, 0, 0, 1, 1);
            label.rectTransform.offsetMin = new Vector2(LabelPadding, 0);
            label.rectTransform.offsetMax = new Vector2(-LabelPadding, 0);
            return new PanelButton(image.rectTransform, image, label);
        }

        public void Set(string label, Color color)
        {
            _label.text = label ?? string.Empty;
            _image.color = color;
        }
    }
}
