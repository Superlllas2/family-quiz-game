using System;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The answer countdown the TV draws in Answer and RaceQuestion, read from the players' personal deadlines
    /// (docs/03_rules.md, "Timer multiplier and personal deadlines"). Plain C#, so it is tested without the engine.
    /// </summary>
    public static class AnswerClock
    {
        /// <summary>From the answers opening to the latest personal deadline (last call only moves deadlines earlier).</summary>
        public static long Span(GameState state)
        {
            var question = state.Question;
            long start = question != null ? question.AnswerStartedAtMs : state.PhaseStartedAtMs;
            long latest = long.MinValue;
            foreach (var p in state.Players)
            {
                if (p.Turn != null) latest = Math.Max(latest, p.Turn.DeadlineMs);
            }

            long span = latest == long.MinValue ? 0 : latest - start;
            if (span <= 0 && question != null) span = question.BaseWindowMs;
            return Math.Max(1, span);
        }

        /// <summary>The latest deadline among players the phase still waits for; now when nobody is left.</summary>
        public static long LatestOpenDeadline(GameState state, long now)
        {
            long latest = now;
            foreach (var p in state.Players)
            {
                if (p.Connected && p.Turn != null && !p.Turn.Locked) latest = Math.Max(latest, p.Turn.DeadlineMs);
            }

            return latest;
        }

        /// <summary>
        /// Answered of those the phase waits for: connected players, plus anyone who locked in before leaving.
        /// </summary>
        public static void Count(GameState state, out int locked, out int total)
        {
            locked = 0;
            total = 0;
            foreach (var p in state.Players)
            {
                if (p.Turn == null) continue;
                if (p.Turn.Locked) locked++;
                if (p.Turn.Locked || p.Connected) total++;
            }
        }
    }
}
