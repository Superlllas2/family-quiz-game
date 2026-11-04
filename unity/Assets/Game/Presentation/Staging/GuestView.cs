using TMPro;
using UnityEngine;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// One guest on the 3D set for game night 1 (D89, D98): a capsule in the character's signature colour with eyes,
    /// a name tag in front of the seat and an answer card it holds up, animated in code. A listener's view only: every
    /// animation runs on unscaled time and is cut or replaced the moment the director plays another one.
    /// </summary>
    public sealed class GuestView : MonoBehaviour
    {
        public enum Move
        {
            None,
            Enter,
            Nod,
            Happy,
            Sad,
            Shrug,
            Wave,
            Flinch,
            Leave
        }

        private const string LitShader = "Universal Render Pipeline/Lit";
        private static readonly Color AwayColor = new Color(0.32f, 0.32f, 0.34f);
        private static readonly Color CardColor = new Color(0.95f, 0.93f, 0.88f);

        private Transform _root;
        private Transform _body;
        private Transform _card;
        private TextMeshPro _cardText;
        private TextMeshPro _characterTag;
        private TextMeshPro _playerTag;
        private Material _bodyMaterial;
        private Color _color;
        private bool _away;
        private Move _move;
        private float _moveStart;
        private float _moveLength;
        private float _cardUpSince = -1f;
        private float _cardUpSeconds = 0.5f;
        private bool _cardUp;
        private bool _applauding;
        private bool _celebrating;
        private bool _lookingDown;
        private float _phase;
        private float _popSeconds = 0.5f;

        public bool Gone { get; private set; }

        public static GuestView Create(Transform parent, Transform seat, string name)
        {
            var go = new GameObject("Guest " + name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(seat.position, seat.rotation);
            var view = go.AddComponent<GuestView>();
            view.Build();
            return view;
        }

        public void Configure(Color color, float popSeconds, float cardUpSeconds)
        {
            _color = color;
            _popSeconds = Mathf.Max(0.05f, popSeconds);
            _cardUpSeconds = Mathf.Max(0.05f, cardUpSeconds);
            ApplyColor();
        }

        public void SetLabels(string character, string player, bool highlight)
        {
            _characterTag.text = character;
            _playerTag.text = player;
            _playerTag.color = highlight ? TvColors.Correct : TvColors.Text;
        }

        /// <summary>Neighbours on the sofa alternate between two tag heights so their names never overlap.</summary>
        public void SetTagRow(int row)
        {
            float drop = row % 2 == 1 ? 0.3f : 0f;
            _characterTag.transform.localPosition = new Vector3(0, -0.02f - drop, 0.62f);
            _playerTag.transform.localPosition = new Vector3(0, -0.16f - drop, 0.62f);
        }

        public void SetAway(bool away)
        {
            if (_away == away) return;
            _away = away;
            ApplyColor();
        }

        public void Play(Move move)
        {
            if (Gone) return;
            _move = move;
            _moveStart = Time.unscaledTime;
            _moveLength = move == Move.Enter ? Mathf.Max(_popSeconds, 1.0f)
                : move == Move.Nod ? 0.4f
                : move == Move.Leave ? 0.6f
                : move == Move.Flinch ? 0.5f
                : 1.5f;
        }

        /// <summary>Holds up the answer card: a letter in its option colour, a number, or nothing (no answer).</summary>
        public void RaiseCard(string text, Color textColor)
        {
            _cardText.text = text ?? string.Empty;
            _cardText.color = textColor;
            _card.gameObject.SetActive(true);
            _cardUp = true;
            _cardUpSince = Time.unscaledTime;
        }

        public void LowerCard()
        {
            _cardUp = false;
            _card.gameObject.SetActive(false);
        }

        public void SetApplauding(bool on) => _applauding = on;

        public void SetCelebrating(bool on) => _celebrating = on;

        public void SetLookingDown(bool on) => _lookingDown = on;

        /// <summary>Turns the name tag and the card towards the camera so they stay readable from every shot.</summary>
        public void FaceCamera(Camera camera)
        {
            if (camera == null) return;
            var forward = camera.transform.forward;
            var flat = new Vector3(forward.x, 0, forward.z);
            if (flat.sqrMagnitude < 1e-4f) return;
            var rotation = Quaternion.LookRotation(flat.normalized);
            _characterTag.transform.rotation = rotation;
            _playerTag.transform.rotation = rotation;
            _card.rotation = rotation;
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            _phase += Time.unscaledDeltaTime;
            float t = _moveLength > 0 ? Mathf.Clamp01((now - _moveStart) / _moveLength) : 1f;
            var scale = Vector3.one;
            var offset = Vector3.zero;
            float tilt = 0f;
            float spin = 0f;

            // Idle: a slow breath and a small sway, each guest a little out of step with the others.
            float breath = Mathf.Sin(_phase * 1.6f + transform.position.x) * 0.012f;
            scale.y += breath;
            tilt += Mathf.Sin(_phase * 0.7f + transform.position.x * 1.7f) * 1.5f;
            if (_away) scale.y -= 0.06f;
            if (_lookingDown) tilt += 10f;

            switch (_move)
            {
                case Move.Enter:
                {
                    float p = Mathf.Clamp01((now - _moveStart) / _popSeconds);
                    float s = EaseOutBack(p);
                    scale *= Mathf.Max(0.001f, s);
                    offset.y += (1f - p) * 0.25f;
                    break;
                }
                case Move.Nod:
                    tilt += Mathf.Sin(t * Mathf.PI) * 14f;
                    break;
                case Move.Happy:
                    offset.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3f)) * 0.28f * (1f - t * 0.5f);
                    spin = t < 0.6f ? Mathf.SmoothStep(0, 360f, t / 0.6f) : 0f;
                    break;
                case Move.Sad:
                    scale.y *= 1f - Mathf.Sin(Mathf.Min(1f, t * 1.5f) * Mathf.PI * 0.5f) * 0.22f * (t < 0.8f ? 1f : (1f - t) * 5f);
                    scale.x *= 1f + (1f - scale.y) * 0.4f;
                    scale.z = scale.x;
                    tilt -= Mathf.Sin(t * Mathf.PI) * 10f;
                    break;
                case Move.Shrug:
                    offset.y += Mathf.Sin(t * Mathf.PI * 2f) * 0.05f;
                    tilt += Mathf.Sin(t * Mathf.PI * 4f) * 6f;
                    break;
                case Move.Wave:
                    offset.y += Mathf.Sin(t * Mathf.PI) * 0.35f;
                    tilt += Mathf.Sin(t * Mathf.PI * 6f) * 8f;
                    break;
                case Move.Flinch:
                    offset.x += Mathf.Sin(t * Mathf.PI * 8f) * 0.06f * (1f - t);
                    break;
                case Move.Leave:
                    scale *= 1f - EaseInBack(t);
                    offset.y += t * 0.2f;
                    if (t >= 1f) Gone = true;
                    break;
            }

            if (_move != Move.None && t >= 1f && _move != Move.Leave) _move = Move.None;

            if (_applauding && _move == Move.None)
            {
                tilt += Mathf.Sin(_phase * 14f) * 3f;
                offset.y += Mathf.Abs(Mathf.Sin(_phase * 7f)) * 0.03f;
            }

            if (_celebrating && _move == Move.None)
            {
                offset.y += Mathf.Abs(Mathf.Sin(_phase * 5f)) * 0.25f;
                spin = Mathf.Repeat(_phase * 180f, 360f);
            }

            _root.localScale = scale;
            _root.localPosition = offset;
            _root.localRotation = Quaternion.Euler(-tilt * 0.5f, spin, tilt * 0.4f);

            if (_cardUp)
            {
                float p = Mathf.Clamp01((now - _cardUpSince) / _cardUpSeconds);
                float y = Mathf.LerpUnclamped(0.45f, 1.26f, EaseOutBack(p)) + Mathf.Sin(_phase * 2.2f) * 0.01f;
                _card.position = transform.position + transform.forward * 0.22f + Vector3.up * y + _root.localPosition * 0.6f;
                _card.localScale = new Vector3(0.62f, 0.46f, 0.02f) * Mathf.Clamp01(p * 3f);
            }

            if (Gone) gameObject.SetActive(false);
        }

        private void ApplyColor()
        {
            if (_bodyMaterial == null) return;
            _bodyMaterial.SetColor("_BaseColor", _away ? Color.Lerp(_color, AwayColor, 0.85f) : _color);
        }

        private void Build()
        {
            _root = new GameObject("Root").transform;
            _root.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            Object.Destroy(body.GetComponent<Collider>());
            _body = body.transform;
            _body.SetParent(_root, false);
            _body.localPosition = new Vector3(0, 0.5f, 0);
            _body.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            _bodyMaterial = NewMaterial(Color.white, 0.35f);
            body.GetComponent<Renderer>().sharedMaterial = _bodyMaterial;

            var white = NewMaterial(Color.white, 0.7f);
            var dark = NewMaterial(new Color(0.06f, 0.05f, 0.05f), 0.8f);
            foreach (float side in new[] { -0.1f, 0.1f })
            {
                var eye = Sphere("Eye", _body, new Vector3(side / 0.6f, 0.42f, 0.42f), new Vector3(0.13f / 0.6f, 0.13f / 0.5f, 0.07f / 0.6f), white);
                Sphere("Pupil", eye.transform, new Vector3(0, -0.05f, 0.45f), new Vector3(0.5f, 0.5f, 0.4f), dark);
            }

            _characterTag = Tag("CharacterTag", new Vector3(0, -0.02f, 0.62f), 1.3f, false);
            _playerTag = Tag("PlayerTag", new Vector3(0, -0.16f, 0.62f), 1.9f, true);

            var card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.name = "Card";
            Object.Destroy(card.GetComponent<Collider>());
            _card = card.transform;
            _card.SetParent(transform, false);
            card.GetComponent<Renderer>().sharedMaterial = NewMaterial(CardColor, 0.2f);
            _cardText = new GameObject("Text").AddComponent<TextMeshPro>();
            _cardText.transform.SetParent(_card, false);
            _cardText.transform.localPosition = new Vector3(0, 0, -0.6f);
            _cardText.transform.localScale = new Vector3(1f / 0.62f, 1f / 0.46f, 1f) * 0.1f;
            _cardText.rectTransform.sizeDelta = new Vector2(5.6f, 4.2f);
            _cardText.fontSize = 32;
            _cardText.enableAutoSizing = true;
            _cardText.fontSizeMin = 8;
            _cardText.fontSizeMax = 32;
            _cardText.fontStyle = FontStyles.Bold;
            _cardText.alignment = TextAlignmentOptions.Center;
            _cardText.textWrappingMode = TextWrappingModes.NoWrap;
            card.SetActive(false);
        }

        private TextMeshPro Tag(string name, Vector3 local, float size, bool bold)
        {
            var text = new GameObject(name).AddComponent<TextMeshPro>();
            text.transform.SetParent(transform, false);
            text.transform.localPosition = local;
            text.rectTransform.sizeDelta = new Vector2(0.68f, 0.16f);
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * 0.5f;
            text.fontSizeMax = size;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.color = TvColors.Text;
            text.outlineWidth = 0.22f;
            text.outlineColor = new Color32(0, 0, 0, 220);
            return text;
        }

        private static GameObject Sphere(string name, Transform parent, Vector3 local, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        public static Material NewMaterial(Color color, float smoothness)
        {
            var m = new Material(Shader.Find(LitShader));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseInBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }
    }
}
