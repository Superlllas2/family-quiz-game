using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// One player's chip on the race track, in its own lane, in the cell of its step. It slides to a new step, forward
    /// or back (D157: a wrong answer is a step back), on unscaled time after a short wait; hidden mid slide it jumps to
    /// the end. A small mark on its corner says what the turn did on RaceReveal («Пас», −1). It never holds a phase.
    /// </summary>
    public sealed class RaceRunner : MonoBehaviour
    {
        private const float Padding = 6f;
        private const float NameSize = 36f;

        private int _finishSteps;
        private float _laneTop;
        private float _laneHeight;
        private float _from;
        private float _to;
        private float _age;
        private bool _moving;
        private TvPiece _mark;
        private TextMeshProUGUI _markText;

        public string PlayerId { get; private set; }

        public CharacterChip Chip { get; private set; }

        /// <summary>The step the runner is on, or sliding to.</summary>
        public int Step { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        public static RaceRunner Create(Transform parent, string playerId)
        {
            var rect = UiBuilder.Rect("Runner " + playerId, parent);
            var runner = rect.gameObject.AddComponent<RaceRunner>();
            runner.PlayerId = playerId;
            runner.Chip = CharacterChip.Create(rect, playerId, NameSize);
            UiBuilder.Anchor(runner.Chip.Rect, 0, 0, 1, 1);
            runner._mark = TvPiece.Card("Mark", rect, TvColors.Cream, 10f, 4f).Tilt(6f);
            var mark = runner._mark.Root;
            mark.anchorMin = mark.anchorMax = new Vector2(1f, 1f);
            mark.pivot = new Vector2(0.75f, 0.6f);
            mark.anchoredPosition = Vector2.zero;
            mark.sizeDelta = new Vector2(76f, 34f);
            TvTween.On(mark).Tilt = 6f;
            runner._markText = UiBuilder.Display("Text", runner._mark.Content, 22, TvColors.Ink, TextAlignmentOptions.Center);
            UiBuilder.Fit(runner._markText, 12, 22, 1);
            UiBuilder.Anchor(runner._markText.rectTransform, 0.06f, 0.06f, 0.94f, 0.94f);
            mark.gameObject.SetActive(false);
            return runner;
        }

        /// <summary>A small tag on the chip's top right corner; null or empty hides it.</summary>
        public void SetMark(string text, Color fill, bool animate)
        {
            bool show = !string.IsNullOrEmpty(text);
            bool was = _mark.Root.gameObject.activeSelf;
            _mark.Root.gameObject.SetActive(show);
            if (!show) return;
            _markText.text = text;
            _markText.color = TvColors.TextOn(fill);
            _mark.SetFill(fill);
            if (animate && !was) TvTween.On(_mark.Root).Slam(TvMotion.RaceMoveDelaySeconds, 0.35f);
        }

        /// <summary>The lane: top edge and height in pixels from the top of the lanes.</summary>
        public void SetLane(float top, float height)
        {
            _laneTop = top;
            _laneHeight = height;
            Apply(_moving ? Current() : _to);
        }

        /// <summary>Puts the runner on a step at once.</summary>
        public void Place(int step, int finishSteps)
        {
            _moving = false;
            _finishSteps = finishSteps;
            Step = step;
            _to = RaceTrackLayout.CellCenter(step, finishSteps);
            _from = _to;
            Apply(_to);
        }

        /// <summary>Slides from where the runner is drawn to a new step after delay seconds; nothing when it is already going there.</summary>
        public void Slide(int step, int finishSteps, float delay)
        {
            if (step == Step && finishSteps == _finishSteps) return;
            float from = _moving ? Current() : _to;
            _finishSteps = finishSteps;
            Step = step;
            _from = from;
            _to = RaceTrackLayout.CellCenter(step, finishSteps);
            _age = -delay;
            _moving = true;
            Apply(_from);
        }

        private float Current()
        {
            float t = Mathf.Clamp01(_age / TvMotion.RaceMoveSeconds);
            return Mathf.Lerp(_from, _to, t * t * (3f - 2f * t));
        }

        private void Update()
        {
            if (!_moving) return;
            _age += Time.unscaledDeltaTime;
            if (_age < 0f) return;
            Apply(Current());
            if (_age >= TvMotion.RaceMoveSeconds) _moving = false;
        }

        private void OnDisable()
        {
            if (!_moving) return;
            _moving = false;
            Apply(_to);
        }

        /// <summary>Centers the runner on a fraction of the track, one cell wide, in its lane.</summary>
        private void Apply(float center)
        {
            float half = RaceTrackLayout.CellWidth(_finishSteps) / 2f;
            Rect.anchorMin = new Vector2(center - half, 1f);
            Rect.anchorMax = new Vector2(center + half, 1f);
            Rect.pivot = new Vector2(0.5f, 1f);
            Rect.anchoredPosition = new Vector2(0f, -(_laneTop + Padding));
            Rect.sizeDelta = new Vector2(-2f * Padding, Mathf.Max(0f, _laneHeight - 2f * Padding));
        }
    }
}
