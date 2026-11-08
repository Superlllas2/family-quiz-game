using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// Where things go on the TV (decisions/0017_game_night_ui.md), in canvas units of the 1920 x 1080 reference, y up.
    /// Three fixed zones keep the pieces from ever covering each other: the hosts' bubbles own the top left, the status
    /// corner (clock and player tokens, or a view's title) owns the top right, and every view draws its content below
    /// ContentTop. Each view's pieces are named here too, so TvLayoutTests can check that no two of them meet. The
    /// content leaves the guests visible in the Wide and Seats shots: the guests sit between about 33 % and 57 % of the
    /// frame height, so the question card and tiles stay under them.
    /// </summary>
    public static class TvLayout
    {
        public const float Width = 1920f;
        public const float Height = 1080f;
        public const float Margin = 24f;

        /// <summary>The hosts' comic bubbles, stacked from the top: at most one per host.</summary>
        public static readonly Rect Hosts = new Rect(Margin, 806f, 960f, 250f);

        /// <summary>The clock token and the player tokens with their stickers.</summary>
        public static readonly Rect Status = new Rect(1004f, 890f, 892f, 166f);

        /// <summary>A view's title ribbon, when it has no status strip.</summary>
        public static readonly Rect Title = new Rect(1004f, 916f, 892f, 120f);

        /// <summary>Power up tags under the status corner.</summary>
        public static readonly Rect Side = new Rect(1436f, 620f, 460f, 250f);

        /// <summary>Everything else stays below this line.</summary>
        public const float ContentTop = 790f;

        /// <summary>QuestionShow: the question card on the board, framed by the Stage shot (D92).</summary>
        public static readonly Rect ShowCard = new Rect(326f, 400f, 1268f, 300f);

        /// <summary>QuestionShow of a closest value question: the ruler with the range under the card.</summary>
        public static readonly Rect ShowRuler = new Rect(420f, 190f, 1080f, 150f);

        /// <summary>Answer and Reveal: the question card under the guests.</summary>
        public static readonly Rect AnswerCard = new Rect(307f, 246f, 1306f, 146f);

        /// <summary>Reveal: the four tiles with their texts, two by two, under the card.</summary>
        public static readonly Rect Tiles = new Rect(307f, 27f, 1306f, 196f);

        /// <summary>
        /// Answer of a multiple choice question (D141): the options are on the phones only, so the card grows and the
        /// tiles under it are empty, A to D in the phone's 2 x 2 layout and colours. Reveal goes back to AnswerCard and Tiles.
        /// </summary>
        public static readonly Rect AnswerCardLarge = new Rect(307f, 200f, 1306f, 192f);

        /// <summary>Answer of a multiple choice question (D141): the four empty tiles under the large card.</summary>
        public static readonly Rect TilesEmpty = new Rect(307f, 27f, 1306f, 158f);

        /// <summary>A closest value question's ruler in Answer, and its pins and flag on Reveal.</summary>
        public static readonly Rect Ruler = new Rect(150f, 27f, 1620f, 186f);

        /// <summary>The closest value Reveal: the ruler with the pins needs the card's room too.</summary>
        public static readonly Rect RulerReveal = new Rect(150f, 27f, 1620f, 380f);

        /// <summary>TopicVote: its title banner, left, under the hosts' zone.</summary>
        public static readonly Rect VoteTitle = new Rect(80f, 690f, 1100f, 92f);

        /// <summary>TopicVote: the fanned topic cards.</summary>
        public static readonly Rect VoteCards = new Rect(170f, 40f, 1580f, 610f);

        /// <summary>Scores: the "why" card where the question card was, and the row of score cards under it.</summary>
        public static readonly Rect ScoreFact = AnswerCard;

        public static readonly Rect ScoreRow = new Rect(Margin, 27f, Width - 2 * Margin, 186f);

        /// <summary>Lobby: the ticket with the room code and the QR code, on the floor under the seats.</summary>
        public static readonly Rect Ticket = new Rect(400f, 30f, 1120f, 300f);

        /// <summary>A full width content area for the other views (race, round end, results, poll).</summary>
        public static readonly Rect Content = new Rect(Margin, 27f, Width - 2 * Margin, ContentTop - 27f);

        /// <summary>Stretches rect over r (canvas units) inside a full screen parent.</summary>
        public static RectTransform Put(RectTransform rect, Rect r)
        {
            rect.anchorMin = new Vector2(r.xMin / Width, r.yMin / Height);
            rect.anchorMax = new Vector2(r.xMax / Width, r.yMax / Height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static void PlaceStatus(RectTransform rect)
        {
            Put(rect, Status);
        }

        /// <summary>The pieces each phase shows at once, for TvLayoutTests (title or status, never both).</summary>
        public static Rect[][] PhaseSets()
        {
            return new[]
            {
                new[] { Hosts, Status, ShowCard, ShowRuler },
                new[] { Hosts, Status, Side, AnswerCardLarge, TilesEmpty },
                new[] { Hosts, Status, Side, AnswerCard, Tiles },
                new[] { Hosts, Status, Side, AnswerCard, Ruler },
                new[] { Hosts, Status, Side, RulerReveal },
                new[] { Hosts, Status, VoteTitle, VoteCards },
                new[] { Hosts, Title, Side, ScoreFact, ScoreRow },
                new[] { Hosts, Title, Ticket },
                new[] { Hosts, Title, Content },
            };
        }
    }
}
