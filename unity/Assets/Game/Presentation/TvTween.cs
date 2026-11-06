using System;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The pieces' little moves (decisions/0017_game_night_ui.md): pop in with a bounce, slam down like a rubber stamp,
    /// flip like a card, wiggle in the last seconds. Presentation only, on unscaled time: they never hold a phase
    /// (pillar 4), and a disabled piece snaps to its end state.
    /// </summary>
    public sealed class TvTween : MonoBehaviour
    {
        private enum Kind { None, Pop, Slam, Flip, Rise }

        private Kind _kind;
        private float _age;
        private float _delay;
        private float _duration;
        private Action _midway;
        private bool _midwayDone;
        private bool _wiggle;
        private float _wiggleAge;
        private CanvasGroup _group;
        private Vector2 _basePosition;
        private bool _hasBase;

        /// <summary>The resting tilt in degrees; wiggles and flips turn around it.</summary>
        public float Tilt { get; set; }

        public static TvTween On(Component target)
        {
            var tween = target.GetComponent<TvTween>();
            if (tween == null) tween = target.gameObject.AddComponent<TvTween>();
            return tween;
        }

        private CanvasGroup Group
        {
            get
            {
                if (_group == null) _group = GetComponent<CanvasGroup>();
                if (_group == null)
                {
                    _group = gameObject.AddComponent<CanvasGroup>();
                    _group.blocksRaycasts = false;
                    _group.interactable = false;
                }

                return _group;
            }
        }

        /// <summary>Grows in from small with an overshoot, fading in.</summary>
        public void Pop(float delay = 0f, float duration = 0.45f)
        {
            Start(Kind.Pop, delay, duration);
        }

        /// <summary>Slams down from big, like a rubber stamp.</summary>
        public void Slam(float delay = 0f, float duration = 0.42f)
        {
            Start(Kind.Slam, delay, duration);
        }

        /// <summary>Rises a little from below, fading in.</summary>
        public void Rise(float delay = 0f, float duration = 0.4f)
        {
            Start(Kind.Rise, delay, duration);
        }

        /// <summary>Flips over the horizontal axis; midway (edge on) runs once, to swap what the piece shows.</summary>
        public void Flip(Action midway, float delay = 0f, float duration = 0.4f)
        {
            Start(Kind.Flip, delay, duration);
            _midway = midway;
            _midwayDone = false;
        }

        public void SetWiggle(bool on)
        {
            if (_wiggle == on) return;
            _wiggle = on;
            _wiggleAge = 0f;
            if (!on) ApplyRotation(0f);
        }

        /// <summary>Ends whatever runs, at its end state.</summary>
        public void Finish()
        {
            if (_kind == Kind.Flip && !_midwayDone) _midway?.Invoke();
            _kind = Kind.None;
            _midway = null;
            transform.localScale = Vector3.one;
            if (_hasBase) ((RectTransform)transform).anchoredPosition = _basePosition;
            if (_group != null) _group.alpha = 1f;
            ApplyRotation(0f);
        }

        private void Start(Kind kind, float delay, float duration)
        {
            if (_kind == Kind.Rise && _hasBase) ((RectTransform)transform).anchoredPosition = _basePosition;
            _kind = kind;
            _age = 0f;
            _delay = delay;
            _duration = Mathf.Max(0.05f, duration);
            _midway = null;
            _basePosition = ((RectTransform)transform).anchoredPosition;
            _hasBase = kind == Kind.Rise;
            Apply(0f);
        }

        private void Update()
        {
            if (_wiggle)
            {
                _wiggleAge += Time.unscaledDeltaTime;
                ApplyRotation(Mathf.Sin(_wiggleAge * 26f) * 4f);
            }

            if (_kind == Kind.None) return;
            _age += Time.unscaledDeltaTime;
            float t = (_age - _delay) / _duration;
            if (t >= 1f)
            {
                Finish();
                return;
            }

            Apply(Mathf.Max(0f, t));
        }

        private void OnDisable()
        {
            if (_kind != Kind.None) Finish();
            _wiggle = false;
        }

        private void Apply(float t)
        {
            switch (_kind)
            {
                case Kind.Pop:
                {
                    float s = t <= 0f ? 0.55f : 0.55f + 0.45f * BackOut(t);
                    transform.localScale = new Vector3(s, s, 1f);
                    Group.alpha = Mathf.Clamp01(t * 4f);
                    break;
                }
                case Kind.Slam:
                {
                    float s = t <= 0f ? 2.6f : Mathf.LerpUnclamped(2.6f, 1f, BounceOut(t));
                    transform.localScale = new Vector3(s, s, 1f);
                    Group.alpha = Mathf.Clamp01(t * 6f);
                    break;
                }
                case Kind.Rise:
                {
                    var rect = (RectTransform)transform;
                    rect.anchoredPosition = _basePosition + new Vector2(0, -40f * (1f - BackOut(t)));
                    Group.alpha = Mathf.Clamp01(t * 3f);
                    break;
                }
                case Kind.Flip:
                {
                    if (t >= 0.5f && !_midwayDone)
                    {
                        _midwayDone = true;
                        _midway?.Invoke();
                    }

                    float y = Mathf.Abs(Mathf.Cos(t * Mathf.PI));
                    transform.localScale = new Vector3(1f, Mathf.Max(0.02f, y), 1f);
                    break;
                }
            }
        }

        private void ApplyRotation(float extra)
        {
            transform.localRotation = Quaternion.Euler(0, 0, Tilt + extra);
        }

        private static float BackOut(float t)
        {
            const float c1 = 1.9f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        private static float BounceOut(float t)
        {
            // Lands at t = 0.55, overshoots a little below 1 and settles.
            if (t < 0.55f) return (t / 0.55f) * (t / 0.55f);
            float u = (t - 0.55f) / 0.45f;
            return 1f + 0.08f * Mathf.Sin(u * Mathf.PI) * (1f - u);
        }
    }
}
