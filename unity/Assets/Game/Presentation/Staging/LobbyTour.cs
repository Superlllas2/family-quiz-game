namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// The timing of the Lobby tour (D160, docs/21_staging.md "LobbyTour"): the camera makes one slow move around the
    /// dim studio per cut of staging.lobby.cutSeconds, fading in over the first staging.lobby.fadeSeconds and out over
    /// the last, both inside the cut. The moves come in a fixed cycle, so the same move never plays twice in a row
    /// (with two or more moves). Plain C#: fed the time since the Lobby began, it says which move, how far through it
    /// and how bright. A listener only: the Lobby has no timer and nothing here holds a phase.
    /// </summary>
    public sealed class LobbyTour
    {
        private readonly long _cutMs;
        private readonly long _fadeMs;
        private readonly int _moves;

        /// <param name="cutMs">staging.lobby.cutSeconds in ms.</param>
        /// <param name="fadeMs">staging.lobby.fadeSeconds in ms; longer than half a cut, the fades meet in the middle.</param>
        /// <param name="moves">How many moves the tour cycles through.</param>
        public LobbyTour(long cutMs, long fadeMs, int moves)
        {
            _cutMs = cutMs;
            _fadeMs = fadeMs < 0 ? 0 : fadeMs;
            _moves = moves < 1 ? 1 : moves;
        }

        public LobbyTourFrame At(long elapsedMs)
        {
            if (elapsedMs < 0) elapsedMs = 0;
            if (_cutMs <= 0) return new LobbyTourFrame(0, 0, 0, 1);
            long cut = elapsedMs / _cutMs;
            long into = elapsedMs - cut * _cutMs;
            double progress = (double)into / _cutMs;
            double fade = _fadeMs * 2 > _cutMs ? _cutMs / 2.0 : _fadeMs;
            double alpha = 1;
            if (fade > 0)
            {
                long fromEdge = into < _cutMs - into ? into : _cutMs - into;
                if (fromEdge < fade) alpha = fromEdge / fade;
            }

            return new LobbyTourFrame(cut, (int)(cut % _moves), progress, alpha);
        }
    }
}
