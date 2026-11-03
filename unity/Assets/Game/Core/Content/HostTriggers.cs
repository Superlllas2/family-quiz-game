using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>The host line triggers for game night 1 (docs/15_schemas.md, "Triggers for game night 1").</summary>
    public static class HostTriggers
    {
        public const string PlayerJoined = "playerJoined";
        public const string StartCountdown = "startCountdown";
        public const string GameStart = "gameStart";
        public const string RoundStart = "roundStart";
        public const string EveryoneCorrect = "everyoneCorrect";
        public const string NobodyCorrect = "nobodyCorrect";
        public const string FastAnswer = "fastAnswer";
        public const string NewLeader = "newLeader";
        public const string Streak3 = "streak3";
        public const string PowerUpUsed = "powerUpUsed";
        public const string ClosestExact = "closestExact";
        public const string LastPlaceCorrect = "lastPlaceCorrect";
        public const string RoundEnd = "roundEnd";
        public const string FinalStart = "finalStart";
        public const string RaceWinner = "raceWinner";
        public const string LastCall = "lastCall";

        public static readonly IReadOnlyList<string> All = new[]
        {
            PlayerJoined, StartCountdown, GameStart, RoundStart, EveryoneCorrect, NobodyCorrect, FastAnswer, NewLeader, Streak3,
            PowerUpUsed, ClosestExact, LastPlaceCorrect, RoundEnd, FinalStart, RaceWinner, LastCall
        };

        /// <summary>
        /// The triggers of one question cycle, highest priority first (docs/15_schemas.md, "Selection" rule 1): the
        /// Reveal triggers, then powerUpUsed (Reveal or Scores).
        /// </summary>
        public static readonly IReadOnlyList<string> CyclePriority = new[]
        {
            LastPlaceCorrect, NewLeader, Streak3, ClosestExact, EveryoneCorrect, NobodyCorrect, FastAnswer, PowerUpUsed
        };

        public static bool IsValid(string trigger)
        {
            foreach (var t in All)
            {
                if (t == trigger) return true;
            }

            return false;
        }

        /// <summary>True for a trigger that competes in the question cycle (rule 1); the others play one line each.</summary>
        public static bool IsCycleTrigger(string trigger)
        {
            foreach (var t in CyclePriority)
            {
                if (t == trigger) return true;
            }

            return false;
        }
    }
}
