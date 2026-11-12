using System;
using System.Collections.Generic;
using FamilyQuiz.Presentation.Graphics;
using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The Graphics panel (decisions/0021, D122 to D124), opened from the pause menu: one row per setting, each a label and
    /// a "&lt; value &gt;" selector of board game pieces (D100). The value piece steps forward on a click, the arrows step
    /// either way. Every change applies at once and is saved on this laptop; it never goes through Core. Escape closes
    /// it and shows the pause menu again.
    /// </summary>
    public sealed class TvGraphicsPanel : MonoBehaviour
    {
        private const float PanelWidth = 1000f;
        private const float Padding = 40f;
        private const float RowsTop = 112f;
        private const float RowStep = 56f; // 14 rows fit 1080 with the ribbon on top (60 before D161 added Brightness)
        private const float LabelWidth = 440f;
        private const float ArrowWidth = 58f;
        private const float ValueWidth = 330f;
        private const float ControlHeight = 50f;
        private const float LabelSize = 30f;
        private const float ValueSize = 28f;

        private readonly List<Row> _rows = new List<Row>();
        private TvContext _context;
        private Func<GraphicsOptions> _get;
        private Action<GraphicsOptions> _set;
        private Func<List<Vector2Int>> _resolutions;
        private RectTransform _card;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _hint;
        private TextMeshProUGUI _footer;

        /// <summary>One setting: its label key, how to draw its value and how to step it.</summary>
        private sealed class Row
        {
            public string LabelKey;
            public Func<GraphicsOptions, string> Value;
            public Action<GraphicsOptions, int> Step;
            public bool ChangesPreset = true;
            public TextMeshProUGUI Label;
            public PanelButton ValueButton;
            public PanelButton Back;
            public PanelButton Forward;
        }

        public static TvGraphicsPanel Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("TvGraphicsPanel", parent), 0, 0, 1, 1);
            var panel = rect.gameObject.AddComponent<TvGraphicsPanel>();
            panel._context = context;
            panel.DefineRows();
            panel.Build();
            rect.gameObject.SetActive(false);
            return panel;
        }

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Where the options come from and go to (the composition root applies and saves them).</summary>
        public void SetGraphics(Func<GraphicsOptions> get, Action<GraphicsOptions> set, Func<List<Vector2Int>> resolutions)
        {
            _get = get;
            _set = set;
            _resolutions = resolutions;
        }

        public void SetOpen(bool open)
        {
            if (open && _get == null) open = false;
            gameObject.SetActive(open);
            if (open) Refresh();
        }

        public void Refresh()
        {
            if (_get == null) return;
            var o = _get();
            _title.text = _context.T("tv.graphics.title");
            _hint.text = _context.T("tv.graphics.hint");
            _footer.text = _context.T("tv.graphics.footer");
            foreach (var row in _rows)
            {
                row.Label.text = _context.T(row.LabelKey);
                row.ValueButton.Set(row.Value(o), TvColors.ButtonOff);
                row.Back.Set("<", TvColors.Mustard);
                row.Forward.Set(">", TvColors.Mustard);
            }
        }

        private void Change(Row row, int step)
        {
            if (_get == null) return;
            var o = _get().Clone();
            row.Step(o, step);
            if (row.ChangesPreset) o.Preset = o.MatchingPreset();
            _set?.Invoke(o);
            Refresh();
        }

        // ------------------------------------------------------------ the rows

        private string T(string key)
        {
            return _context.T(key);
        }

        private string OnOff(bool on)
        {
            return T(on ? "tv.settings.on" : "tv.settings.off");
        }

        private void DefineRows()
        {
            Add("tv.graphics.displayMode", o => T("tv.graphics.mode." + o.DisplayMode),
                (o, s) => o.DisplayMode = GraphicsOptions.Cycle(GraphicsOptions.DisplayModes, o.DisplayMode, s), false);
            Add("tv.graphics.resolution", ResolutionText, StepResolution, false);
            Add("tv.graphics.vsync", o => OnOff(o.VSync), (o, s) => o.VSync = !o.VSync, false);
            Add("tv.graphics.frameLimit", o => o.VSync ? T("tv.graphics.byVsync") : o.FrameLimit <= 0 ? T("tv.graphics.unlimited") : o.FrameLimit.ToString(),
                (o, s) => o.FrameLimit = GraphicsOptions.Cycle(GraphicsOptions.FrameLimits, o.FrameLimit, s), false);
            // D161: stops on the studio's exposure only, never the UI; not part of the quality presets.
            Add("tv.graphics.brightness", o => BrightnessText(o.Brightness),
                (o, s) => o.Brightness = GraphicsOptions.Cycle(GraphicsOptions.BrightnessSteps, o.Brightness, s), false);
            Add("tv.graphics.preset", o => T("tv.graphics.level." + o.Preset), (o, s) =>
            {
                var next = GraphicsOptions.Cycle(GraphicsOptions.Presets, o.Preset == GraphicsOptions.Custom ? "high" : o.Preset, s);
                o.ApplyPreset(next);
            }, false);
            Add("tv.graphics.textures", o => T("tv.graphics.level." + o.Textures),
                (o, s) => o.Textures = GraphicsOptions.Cycle(GraphicsOptions.TextureLevels, o.Textures, s));
            Add("tv.graphics.shadows", o => T("tv.graphics.level." + o.Shadows),
                (o, s) => o.Shadows = GraphicsOptions.Cycle(GraphicsOptions.ShadowLevels, o.Shadows, s));
            Add("tv.graphics.antiAliasing", o => T("tv.graphics.aa." + o.AntiAliasing),
                (o, s) => o.AntiAliasing = GraphicsOptions.Cycle(GraphicsOptions.AntiAliasingModes, o.AntiAliasing, s));
            Add("tv.graphics.renderScale", o => o.RenderScale + "%",
                (o, s) => o.RenderScale = GraphicsOptions.Cycle(GraphicsOptions.RenderScales, o.RenderScale, s));
            Add("tv.graphics.ambientOcclusion", o => OnOff(o.AmbientOcclusion), (o, s) => o.AmbientOcclusion = !o.AmbientOcclusion);
            Add("tv.graphics.bloom", o => OnOff(o.Bloom), (o, s) => o.Bloom = !o.Bloom);
            Add("tv.graphics.depthOfField", o => OnOff(o.DepthOfField), (o, s) => o.DepthOfField = !o.DepthOfField);
            Add("tv.graphics.anisotropic", o => o.Anisotropic <= 0 ? T("tv.settings.off") : o.Anisotropic + "×",
                (o, s) => o.Anisotropic = GraphicsOptions.Cycle(GraphicsOptions.AnisoLevels, o.Anisotropic, s));
        }

        private void Add(string labelKey, Func<GraphicsOptions, string> value, Action<GraphicsOptions, int> step, bool changesPreset = true)
        {
            _rows.Add(new Row { LabelKey = labelKey, Value = value, Step = step, ChangesPreset = changesPreset });
        }

        /// <summary>Stops with their sign: -0.5, 0, +1.25.</summary>
        private static string BrightnessText(double stops)
        {
            if (stops == 0) return "0";
            var text = Math.Abs(stops).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            return (stops < 0 ? "-" : "+") + text;
        }

        private string ResolutionText(GraphicsOptions o)
        {
            if (o.Width <= 0 || o.Height <= 0) return Screen.currentResolution.width + "×" + Screen.currentResolution.height;
            return o.Width + "×" + o.Height;
        }

        private void StepResolution(GraphicsOptions o, int step)
        {
            var list = _resolutions != null ? _resolutions() : new List<Vector2Int>();
            if (list.Count == 0) return;
            var current = o.Width > 0 ? new Vector2Int(o.Width, o.Height)
                : new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            var next = GraphicsOptions.Cycle(list, current, step);
            o.Width = next.x;
            o.Height = next.y;
        }

        // ------------------------------------------------------------ the card

        private void Build()
        {
            var backdrop = UiBuilder.Image("Backdrop", transform, new Color(0.08f, 0.05f, 0.03f, 0.5f));
            backdrop.raycastTarget = true;
            UiBuilder.Anchor(backdrop.rectTransform, 0, 0, 1, 1);

            float height = RowsTop + _rows.Count * RowStep + 70;
            var piece = TvPiece.Card("Card", transform, TvColors.Panel, 26f, 10f).Tilt(-0.4f);
            piece.Body.raycastTarget = true;
            var root = piece.Root;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, -10);
            root.sizeDelta = new Vector2(PanelWidth, height);
            _card = piece.Body.rectTransform;

            var ribbon = TvPiece.Card("Ribbon", _card, TvColors.Cobalt, 16f, 7f).Tilt(-1.5f);
            var rr = ribbon.Root;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0f, 1f);
            rr.anchoredPosition = new Vector2(Padding - 10, 46);
            rr.sizeDelta = new Vector2(380, 96);
            _title = UiBuilder.Display("Title", ribbon.Content, 50, TvColors.Cream, TextAlignmentOptions.Center);
            UiBuilder.Fit(_title, 26, 50, 1);
            UiBuilder.Anchor(_title.rectTransform, 0.05f, 0.08f, 0.95f, 0.92f);

            _hint = Text("Hint", PanelWidth - Padding - 360, 24, 360, 50, 26, TvColors.InkSoft, TextAlignmentOptions.Right);

            float y = RowsTop;
            float valueX = PanelWidth - Padding - ArrowWidth - 8 - ValueWidth;
            foreach (var row in _rows)
            {
                var r = row;
                r.Label = Text("Label", Padding, y, LabelWidth, ControlHeight, LabelSize, TvColors.Ink, TextAlignmentOptions.Left);
                r.Back = PanelButton.Create(_card, "Back", 34f, () => Change(r, -1));
                Box(r.Back.Rect, valueX - 8 - ArrowWidth, y, ArrowWidth, ControlHeight);
                r.ValueButton = PanelButton.Create(_card, "Value", ValueSize, () => Change(r, 1));
                Box(r.ValueButton.Rect, valueX, y, ValueWidth, ControlHeight);
                r.Forward = PanelButton.Create(_card, "Forward", 34f, () => Change(r, 1));
                Box(r.Forward.Rect, valueX + ValueWidth + 8, y, ArrowWidth, ControlHeight);
                y += RowStep;
            }

            _footer = Text("Footer", Padding, y + 10, PanelWidth - 2 * Padding, 36, 24, TvColors.InkSoft, TextAlignmentOptions.Center);
        }

        private TextMeshProUGUI Text(string name, float x, float y, float width, float height, float size, Color color,
            TextAlignmentOptions alignment)
        {
            var text = UiBuilder.Text(name, _card, size, color, true, alignment);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(18f, size);
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
