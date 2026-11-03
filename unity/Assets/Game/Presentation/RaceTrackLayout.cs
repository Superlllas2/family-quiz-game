namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// The race track's cells (docs/02_show_flow.md, "The final race"): steps 0 to race.finishSteps side by side, the last
    /// cell past the finish line. Positions are fractions of the track width. Plain C#, so it is tested without the engine.
    /// </summary>
    public static class RaceTrackLayout
    {
        /// <summary>Number of cells: one per step from 0 to finishSteps.</summary>
        public static int Cells(int finishSteps)
        {
            return finishSteps < 0 ? 1 : finishSteps + 1;
        }

        /// <summary>Width of one cell as a fraction of the track.</summary>
        public static float CellWidth(int finishSteps)
        {
            return 1f / Cells(finishSteps);
        }

        /// <summary>The middle of a step's cell; steps outside 0..finishSteps are clamped.</summary>
        public static float CellCenter(int step, int finishSteps)
        {
            int last = Cells(finishSteps) - 1;
            int clamped = step < 0 ? 0 : step > last ? last : step;
            return (clamped + 0.5f) / Cells(finishSteps);
        }

        /// <summary>The finish line: the left edge of the finishSteps cell.</summary>
        public static float FinishLine(int finishSteps)
        {
            return (Cells(finishSteps) - 1f) / Cells(finishSteps);
        }
    }
}
