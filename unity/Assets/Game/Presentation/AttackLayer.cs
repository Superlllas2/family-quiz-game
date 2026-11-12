using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The attack arrows over a view (D149): one arc per power up used in this pick window, in the power up's colour on
    /// an ink outline, from the attacker's piece to the target's (a status token on Reveal, a score card on Scores and
    /// Attacks). A new arrow flies in from the attacker on unscaled time; the ones already there stay. It never holds a
    /// phase: a phase change simply redraws or hides it.
    /// </summary>
    public sealed class AttackLayer : MonoBehaviour
    {
        private const int Segments = 18;
        private const float Thickness = 9f;
        private const float InkThickness = 17f;
        private const float HeadSize = 40f;
        private const float FlySeconds = 0.45f;

        private readonly List<Arrow> _arrows = new List<Arrow>();

        private sealed class Arrow
        {
            public string Key;
            public GameObject Root;
            public List<Vector2> Points;
            public List<RectTransform> Ink;
            public List<RectTransform> Body;
            public RectTransform Head;
            public float Age;
        }

        public RectTransform Rect => (RectTransform)transform;

        /// <summary>A full size layer; put it last among the view's children so it draws on top.</summary>
        public static AttackLayer Create(Transform parent)
        {
            var rect = UiBuilder.Anchor(UiBuilder.Rect("Attacks", parent), 0, 0, 1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect.gameObject.AddComponent<AttackLayer>();
        }

        /// <summary>
        /// Draws these arrows. anchorOf gives a player's end point as a RectTransform and a point in its local space
        /// (null when the player has no piece); side is the direction the arcs bulge toward; scale shrinks the arcs.
        /// Arrows already drawn keep their place and do not fly again.
        /// </summary>
        public void Show(IReadOnlyList<AttackArrow> arrows, Func<string, (RectTransform Rect, Vector2 Local)?> anchorOf,
            Vector2 side, float scale = 1f)
        {
            var keep = new HashSet<string>();
            var fresh = new List<Arrow>();
            foreach (var a in arrows)
            {
                var from = anchorOf(a.AttackerId);
                var to = anchorOf(a.TargetId);
                if (from == null || to == null) continue;
                var p0 = ToLocal(from.Value.Rect, from.Value.Local);
                var p1 = ToLocal(to.Value.Rect, to.Value.Local);
                float bulge = ArrowPath.Bulge(Vector2.Distance(p0, p1), a.Lane) * scale;
                var points = ArrowPath.Points(p0, p1, side, bulge, Segments);
                var old = _arrows.Find(x => x.Key == a.Key);
                keep.Add(a.Key);
                if (old != null && Same(old.Points, points))
                {
                    fresh.Add(old);
                    continue;
                }

                var arrow = Build(a, points);
                // An arrow that only moved (a roster change) is redrawn in place; a new one flies.
                arrow.Age = old != null ? FlySeconds : 0f;
                if (old != null) Destroy(old.Root);
                fresh.Add(arrow);
                Apply(arrow);
            }

            foreach (var a in _arrows)
            {
                if (!fresh.Contains(a)) Destroy(a.Root);
            }

            _arrows.Clear();
            _arrows.AddRange(fresh);
        }

        public void Clear()
        {
            foreach (var a in _arrows) Destroy(a.Root);
            _arrows.Clear();
        }

        private void Update()
        {
            foreach (var a in _arrows)
            {
                if (a.Age >= FlySeconds) continue;
                a.Age += Time.unscaledDeltaTime;
                Apply(a);
            }
        }

        private void OnDisable()
        {
            foreach (var a in _arrows)
            {
                a.Age = FlySeconds;
                Apply(a);
            }
        }

        private Vector2 ToLocal(RectTransform rect, Vector2 local)
        {
            Vector2 p = Rect.InverseTransformPoint(rect.TransformPoint(local));
            return p - Rect.rect.center;
        }

        private static bool Same(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if ((a[i] - b[i]).sqrMagnitude > 1f) return false;
            return true;
        }

        private Arrow Build(AttackArrow a, List<Vector2> points)
        {
            var root = UiBuilder.Rect("Arrow " + a.Key, transform);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = Vector2.zero;
            var color = TvColors.PowerUp(a.PowerUp);
            var arrow = new Arrow
            {
                Key = a.Key,
                Root = root.gameObject,
                Points = points,
                Ink = new List<RectTransform>(),
                Body = new List<RectTransform>(),
            };
            // The ink outline under the whole line first, then the colour over it, so joints never show ink; the head
            // has its ink outline drawn in.
            for (int i = 0; i < points.Count - 1; i++) arrow.Ink.Add(Piece("Ink", root, TvColors.Ink, TvSprites.Solid));
            for (int i = 0; i < points.Count - 1; i++) arrow.Body.Add(Piece("Body", root, color, TvSprites.Solid));
            arrow.Head = Piece("Head", root, color, TvIcons.ArrowHead);
            return arrow;
        }

        private static RectTransform Piece(string name, Transform parent, Color color, Sprite sprite)
        {
            var image = UiBuilder.Image(name, parent, color, sprite);
            image.type = Image.Type.Simple;
            var r = image.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            return r;
        }

        /// <summary>Shows the arrow up to how far it has flown: the segments behind the head and the head on its tip.</summary>
        private static void Apply(Arrow a)
        {
            float t = Mathf.Clamp01(a.Age / FlySeconds);
            float eased = 1f - (1f - t) * (1f - t);
            int segments = a.Points.Count - 1;
            float reach = eased * segments;
            for (int i = 0; i < segments; i++)
            {
                float part = Mathf.Clamp01(reach - i);
                var start = a.Points[i];
                var end = Vector2.Lerp(a.Points[i], a.Points[i + 1], part);
                bool on = part > 0f;
                Segment(a.Ink[i], start, end, InkThickness, on);
                Segment(a.Body[i], start, end, Thickness, on);
            }

            int at = Mathf.Clamp(Mathf.FloorToInt(reach), 0, segments - 1);
            var tip = Vector2.Lerp(a.Points[at], a.Points[at + 1], Mathf.Clamp01(reach - at));
            var dir = a.Points[at + 1] - a.Points[at];
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Head(a.Head, tip, angle, HeadSize, t > 0f);
        }

        private static void Segment(RectTransform r, Vector2 a, Vector2 b, float thickness, bool on)
        {
            r.gameObject.SetActive(on);
            if (!on) return;
            var d = b - a;
            r.anchoredPosition = (a + b) * 0.5f;
            // A little longer than the gap, so neighbouring segments overlap at the bends.
            r.sizeDelta = new Vector2(d.magnitude + thickness * 0.5f, thickness);
            r.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        private static void Head(RectTransform r, Vector2 tip, float angle, float size, bool on)
        {
            r.gameObject.SetActive(on);
            if (!on) return;
            // The sprite's tip is its right edge's middle: put that on the tip.
            var dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            r.anchoredPosition = tip - dir * (size * 0.5f);
            r.sizeDelta = new Vector2(size, size);
            r.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
