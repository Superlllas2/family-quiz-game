using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// Which sound effect each game event plays (decisions/0020, D120). Plain C#, so it is tested without the engine. A
    /// listener only: it reads the events and the state after them and never changes anything. Every cue is a clip
    /// Resources/Sfx/sfx_&lt;cue&gt; (Kenney.nl, CC0; Assets/Audio/Sfx/LICENSES.md).
    /// </summary>
    public static class SfxCues
    {
        public const string Join = "join";
        public const string Countdown = "countdown";
        public const string Start = "start";
        public const string Vote = "vote";
        public const string Topic = "topic";
        public const string Card = "card";
        public const string Lock = "lock";
        public const string Right = "right";
        public const string Nobody = "nobody";
        public const string Scores = "scores";
        public const string RoundEnd = "round_end";
        public const string RaceStep = "race_step";
        public const string Winner = "winner";
        public const string Pause = "pause";
        public const string Resume = "resume";
        public const string Tick = "tick";

        /// <summary>Every cue that has a clip, for loading.</summary>
        public static readonly string[] All =
        {
            Join, Countdown, Start, Vote, Topic, Card, Lock, Right, Nobody, Scores, RoundEnd, RaceStep, Winner, Pause, Resume, Tick,
            PowerUpTypes.Freeze, PowerUpTypes.Slime, PowerUpTypes.Fog
        };

        /// <summary>The cues of one batch of events, in order, each at most once per batch.</summary>
        public static List<string> For(IReadOnlyList<IGameEvent> events, GameState state)
        {
            var cues = new List<string>();
            foreach (var e in events)
            {
                string cue = null;
                switch (e)
                {
                    case PlayerJoined _:
                        cue = Join;
                        break;
                    case GameStarted _:
                        cue = Start;
                        break;
                    case PhaseStarted started:
                        switch (started.Phase)
                        {
                            case GamePhase.StartCountdown: cue = Countdown; break;
                            case GamePhase.QuestionShow: cue = Card; break;
                            case GamePhase.Scores: cue = Scores; break;
                            case GamePhase.RoundEnd: cue = RoundEnd; break;
                            case GamePhase.Results: cue = Winner; break;
                        }

                        break;
                    case VoteCast _:
                        cue = Vote;
                        break;
                    case TopicChosen _:
                        cue = Topic;
                        break;
                    case AnswerLocked _:
                        cue = Lock;
                        break;
                    case Revealed _:
                        cue = AnyoneRight(state) ? Right : Nobody;
                        break;
                    case RaceRevealed _:
                        cue = RaceStep;
                        break;
                    case PowerUpApplied applied:
                        cue = applied.PowerUp;
                        break;
                    case GamePaused _:
                        cue = Pause;
                        break;
                    case GameResumed _:
                        cue = Resume;
                        break;
                }

                if (cue != null && !cues.Contains(cue)) cues.Add(cue);
            }

            return cues;
        }

        /// <summary>
        /// The answer clock's last seconds (D120): the whole seconds left until the latest deadline of the connected players
        /// still answering in Answer or RaceQuestion, or 0 when nobody is (or the game is paused). The player ticks on each
        /// new value from <see cref="TickFromSeconds"/> down to 1.
        /// </summary>
        public static int SecondsLeft(GameState state, long now)
        {
            if (state.Paused || (state.Phase != GamePhase.Answer && state.Phase != GamePhase.RaceQuestion)) return 0;
            long latest = long.MinValue;
            foreach (var p in state.Players)
            {
                if (p.Connected && p.Turn != null && !p.Turn.Locked) latest = System.Math.Max(latest, p.Turn.DeadlineMs);
            }

            if (latest == long.MinValue || latest <= now) return 0;
            return (int)((latest - now + 999) / 1000);
        }

        /// <summary>The clock ticks over the last this many seconds.</summary>
        public const int TickFromSeconds = 5;

        private static bool AnyoneRight(GameState state)
        {
            foreach (var p in state.Players)
            {
                if (p.Turn != null && p.Turn.Locked && p.Turn.Correct) return true;
            }

            return false;
        }
    }
}
