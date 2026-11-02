namespace FamilyQuiz.Core.Rules
{
    /// <summary>The display order of a question's options, shuffled with the game's seeded random (docs/04_formats.md).</summary>
    public static class OptionShuffle
    {
        /// <summary>
        /// A Fisher-Yates shuffle of 0..count-1 over IRandom.Next. Result[displayIndex] = index into the question's options.
        /// The same seed gives the same order.
        /// </summary>
        public static int[] DisplayOrder(int count, IRandom random)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }

            return order;
        }
    }
}
