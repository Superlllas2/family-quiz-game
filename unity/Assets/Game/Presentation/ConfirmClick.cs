namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// A button that needs a second click within a window to act (Back to lobby in the TV settings panel, which abandons
    /// a running game). Times are real seconds (unscaled), not the core clock, which stops while the relay is down.
    /// Plain C#, so it is tested without the engine.
    /// </summary>
    public sealed class ConfirmClick
    {
        private readonly double _windowSeconds;
        private double? _armedAt;

        public ConfirmClick(double windowSeconds)
        {
            _windowSeconds = windowSeconds;
        }

        /// <summary>True while a first click waits for its confirming second click.</summary>
        public bool Armed(double nowSeconds)
        {
            return _armedAt.HasValue && nowSeconds - _armedAt.Value < _windowSeconds;
        }

        /// <summary>A click: true when it confirms a first click inside the window; otherwise it becomes the first click.</summary>
        public bool Press(double nowSeconds)
        {
            if (Armed(nowSeconds))
            {
                _armedAt = null;
                return true;
            }

            _armedAt = nowSeconds;
            return false;
        }

        public void Cancel()
        {
            _armedAt = null;
        }
    }
}
