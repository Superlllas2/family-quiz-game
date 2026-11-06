using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One board game piece on the TV (decisions/0017_game_night_ui.md): a body of card stock or printed ink with its
    /// ink outline, sitting on a solid ink bottom edge, slightly tilted if asked. Content goes under Body. The root
    /// has no graphic of its own; size and place it like any rect.
    /// </summary>
    public sealed class TvPiece
    {
        private float _edge;

        private TvPiece(RectTransform root, Image body, Image edge, float edgeSize)
        {
            Root = root;
            Body = body;
            Edge = edge;
            _edge = edgeSize;
        }

        public RectTransform Root { get; }

        public Image Body { get; }

        public Image Edge { get; }

        public Transform Content => Body.transform;

        /// <summary>A rounded card: radius and bottom edge in canvas units.</summary>
        public static TvPiece Card(string name, Transform parent, Color fill, float radius = 22f, float edge = 7f)
        {
            var root = UiBuilder.Rect(name, parent);
            var edgeImage = UiBuilder.Image("Edge", root, TvColors.Ink, TvSprites.Solid);
            edgeImage.pixelsPerUnitMultiplier = TvSprites.Radius / Mathf.Max(4f, radius);
            var body = UiBuilder.Image("Body", root, fill, TvSprites.Card);
            body.type = Image.Type.Tiled;
            body.pixelsPerUnitMultiplier = TvSprites.Radius / Mathf.Max(4f, radius);
            UiBuilder.Anchor(body.rectTransform, 0, 0, 1, 1);
            var piece = new TvPiece(root, body, edgeImage, edge);
            piece.SetEdge(edge);
            return piece;
        }

        /// <summary>A round piece: tokens, coins, the clock.</summary>
        public static TvPiece Disc(string name, Transform parent, Color fill, float edge = 5f)
        {
            var root = UiBuilder.Rect(name, parent);
            var edgeImage = UiBuilder.Image("Edge", root, TvColors.Ink);
            edgeImage.sprite = TvSprites.DiscSolid;
            edgeImage.type = Image.Type.Simple;
            var body = UiBuilder.Image("Body", root, fill);
            body.sprite = TvSprites.Disc;
            body.type = Image.Type.Simple;
            UiBuilder.Anchor(body.rectTransform, 0, 0, 1, 1);
            var piece = new TvPiece(root, body, edgeImage, edge);
            piece.SetEdge(edge);
            return piece;
        }

        /// <summary>A sticker burst: "×2!", points.</summary>
        public static TvPiece Star(string name, Transform parent, Color fill)
        {
            var root = UiBuilder.Rect(name, parent);
            var edgeImage = UiBuilder.Image("Edge", root, TvColors.Ink);
            edgeImage.sprite = TvSprites.Star;
            edgeImage.type = Image.Type.Simple;
            var body = UiBuilder.Image("Body", root, fill);
            body.sprite = TvSprites.Star;
            body.type = Image.Type.Simple;
            UiBuilder.Anchor(body.rectTransform, 0, 0, 1, 1);
            var piece = new TvPiece(root, body, edgeImage, 5f);
            piece.SetEdge(5f);
            return piece;
        }

        public void SetFill(Color fill)
        {
            Body.color = fill;
        }

        /// <summary>The thickness of the solid bottom edge; 0 hides it (a piece pressed flat).</summary>
        public void SetEdge(float edge)
        {
            _edge = edge;
            Edge.enabled = edge > 0.01f;
            UiBuilder.Anchor(Edge.rectTransform, 0, 0, 1, 1);
            Edge.rectTransform.offsetMin = new Vector2(0, -edge);
            Edge.rectTransform.offsetMax = new Vector2(0, -edge);
        }

        public float EdgeSize => _edge;

        /// <summary>Turns the piece by degrees (positive is counter clockwise), around its centre.</summary>
        public TvPiece Tilt(float degrees)
        {
            Root.localRotation = Quaternion.Euler(0, 0, degrees);
            return this;
        }
    }
}
