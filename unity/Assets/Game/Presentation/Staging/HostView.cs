using UnityEngine;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// A host for game night 1 (D90): a cube at the desk with eyes, a small idle animation and a speaking animation
    /// while their bubble shows. While speaking the host turns a little towards the guests (docs/21_staging.md,
    /// camera rule 5). The realistic hosts come after game night 1 (D63).
    /// </summary>
    public sealed class HostView : MonoBehaviour
    {
        private const float Size = 0.62f;

        private Transform _cube;
        private float _speakUntil;
        private float _phase;
        private float _turn;
        private float _turnTowardGuests = 22f;

        public static HostView Create(Transform parent, Transform place, string host, Color color)
        {
            var go = new GameObject("Host " + host);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(place.position, place.rotation);
            var view = go.AddComponent<HostView>();
            view.Build(color);
            return view;
        }

        /// <summary>Speaks for this many seconds of real time (the bubble's remaining time).</summary>
        public void Speak(float seconds)
        {
            _speakUntil = Time.unscaledTime + Mathf.Max(0f, seconds);
        }

        public bool Speaking => Time.unscaledTime < _speakUntil;

        private void Update()
        {
            _phase += Time.unscaledDeltaTime;
            bool speaking = Speaking;
            float target = speaking ? _turnTowardGuests : 0f;
            _turn = Mathf.Lerp(_turn, target, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 6f));
            float breath = Mathf.Sin(_phase * 1.4f) * 0.012f;
            float bounce = speaking ? Mathf.Abs(Mathf.Sin(_phase * 9f)) * 0.035f : 0f;
            float wobble = speaking ? Mathf.Sin(_phase * 4.5f) * 4f : Mathf.Sin(_phase * 0.6f) * 1f;
            _cube.localPosition = new Vector3(0, Size * 0.5f + 0.32f + bounce, 0);
            _cube.localScale = new Vector3(Size, Size * (1f + breath + bounce * 0.6f), Size);
            _cube.localRotation = Quaternion.Euler(0, _turn, wobble);
        }

        private void Build(Color color)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Cube";
            Object.Destroy(cube.GetComponent<Collider>());
            _cube = cube.transform;
            _cube.SetParent(transform, false);
            cube.GetComponent<Renderer>().sharedMaterial = GuestView.NewMaterial(color, 0.4f);
            var white = GuestView.NewMaterial(Color.white, 0.7f);
            var dark = GuestView.NewMaterial(new Color(0.06f, 0.05f, 0.05f), 0.8f);
            foreach (float side in new[] { -0.15f, 0.15f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(eye.GetComponent<Collider>());
                eye.transform.SetParent(_cube, false);
                eye.transform.localPosition = new Vector3(side, 0.14f, 0.5f);
                eye.transform.localScale = new Vector3(0.17f, 0.17f, 0.06f);
                eye.GetComponent<Renderer>().sharedMaterial = white;
                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(pupil.GetComponent<Collider>());
                pupil.transform.SetParent(eye.transform, false);
                pupil.transform.localPosition = new Vector3(0, -0.05f, 0.45f);
                pupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.4f);
                pupil.GetComponent<Renderer>().sharedMaterial = dark;
            }

            _turnTowardGuests = transform.position.x > 1.2f ? 26f : 18f;
        }
    }
}
