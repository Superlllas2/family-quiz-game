using System.Collections.Generic;

namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// The final race was set up (log event raceStarted): head starts by rank, and the lastPlaceCanWin check of
    /// docs/02_show_flow.md. Raised when the race's first segment starts (FinalIntro shows the head starts).
    /// </summary>
    public sealed class RaceStarted : IGameEvent
    {
        public RaceStarted(IReadOnlyDictionary<string, int> headStarts, bool lastPlaceCanWin)
        {
            HeadStarts = headStarts;
            LastPlaceCanWin = lastPlaceCanWin;
        }

        public IReadOnlyDictionary<string, int> HeadStarts { get; }
        public bool LastPlaceCanWin { get; }
    }
}
