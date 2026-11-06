using System.Collections.Generic;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// The pause menu (decisions/0018, D105): pausing and resuming flip GameState.Paused and report it. The core clock
    /// is injected, so the composition root stops and starts it on GamePaused and GameResumed (as it does while the
    /// relay is lost). A pause that is already on, or a resume with nothing paused, does nothing.
    /// </summary>
    public sealed class PauseRules
    {
        private readonly GameState _state;

        public PauseRules(GameState state)
        {
            _state = state;
        }

        public void Pause(List<IGameEvent> output)
        {
            if (_state.Paused) return;
            _state.Paused = true;
            output.Add(new GamePaused(_state.Phase, _state.PhaseId));
        }

        public void Resume(List<IGameEvent> output)
        {
            if (!_state.Paused) return;
            _state.Paused = false;
            output.Add(new GameResumed(_state.Phase, _state.PhaseId));
        }
    }
}
