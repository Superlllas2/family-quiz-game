using UnityEngine;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>One slow camera move of the Lobby tour (D160): from a start pose to an end pose, and what it looks at.</summary>
    public readonly struct TourMove
    {
        public TourMove(string name, Vector3 startPosition, Quaternion startRotation, Vector3 endPosition, Quaternion endRotation,
            float fieldOfView, Vector3 subject)
        {
            Name = name;
            StartPosition = startPosition;
            StartRotation = startRotation;
            EndPosition = endPosition;
            EndRotation = endRotation;
            FieldOfView = fieldOfView;
            Subject = subject;
        }

        public string Name { get; }
        public Vector3 StartPosition { get; }
        public Quaternion StartRotation { get; }
        public Vector3 EndPosition { get; }
        public Quaternion EndRotation { get; }
        public float FieldOfView { get; }

        /// <summary>What the lens focuses on.</summary>
        public Vector3 Subject { get; }

        /// <summary>The pose at t (0 start, 1 end): an even glide, like a dolly at walking pace.</summary>
        public void PoseAt(float t, out Vector3 position, out Quaternion rotation)
        {
            t = Mathf.Clamp01(t);
            position = Vector3.Lerp(StartPosition, EndPosition, t);
            rotation = Quaternion.Slerp(StartRotation, EndRotation, t);
        }
    }
}
