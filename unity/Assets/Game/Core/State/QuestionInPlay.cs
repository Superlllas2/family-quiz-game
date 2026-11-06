using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.State
{
    /// <summary>The question of the current question cycle, with its shuffled option order (docs/04_formats.md).</summary>
    public sealed class QuestionInPlay
    {
        private readonly Dictionary<string, int> _ratings = new Dictionary<string, int>();

        internal QuestionInPlay(QuestionDef def, int number, int slot, int roundIndex, double multiplier, int[] displayOrder)
        {
            Def = def;
            Number = number;
            Slot = slot;
            RoundIndex = roundIndex;
            Multiplier = multiplier;
            DisplayOrder = displayOrder;
            CorrectDisplayIndex = -1;
            for (int i = 0; i < displayOrder.Length; i++)
            {
                if (displayOrder[i] == def.Correct) CorrectDisplayIndex = i;
            }
        }

        public QuestionDef Def { get; }

        /// <summary>1 based number of this question in the game.</summary>
        public int Number { get; }

        /// <summary>0 based slot inside the round (rounds.difficultyOrder, rounds.formatOrder).</summary>
        public int Slot { get; }

        /// <summary>0 based round index in the game.</summary>
        public int RoundIndex { get; }

        /// <summary>rounds.multipliers[RoundIndex].</summary>
        public double Multiplier { get; }

        /// <summary>
        /// Multiple choice: DisplayOrder[displayIndex] = index into Def.Options, the same order on the TV and every phone.
        /// Empty for closest value.
        /// </summary>
        public int[] DisplayOrder { get; }

        /// <summary>Display index (0 to 3, A to D) of the right option; -1 for closest value.</summary>
        public int CorrectDisplayIndex { get; }

        /// <summary>phaseId of this question's Answer phase, 0 until it starts.</summary>
        public long AnswerPhaseId { get; internal set; }

        /// <summary>Core clock time the Answer phase started (the options appeared).</summary>
        public long AnswerStartedAtMs { get; internal set; }

        /// <summary>True for a closest value question (slider), false for multiple choice.</summary>
        public bool IsClosestValue => Def.Format == QuestionDef.ClosestValue;

        /// <summary>
        /// Which group of docs/02_show_flow.md "Questions" the question came from: 1 unseen from the chosen topic, 2 unseen
        /// from any topic, 3 seen from the chosen topic, 4 seen from any topic.
        /// </summary>
        public int PickGroup { get; internal set; }

        /// <summary>The format's window in ms before timer multipliers (timers.answer.multipleChoice or closestValue x 1000).</summary>
        public long BaseWindowMs { get; internal set; }

        /// <summary>True once the Answer phase has ended; inputs for it are then "closed".</summary>
        public bool AnswerClosed { get; internal set; }

        /// <summary>The latest stars this player gave this item of the rating card (decisions/0018, D109), 0 if none.</summary>
        public int GivenRating(string playerId, string item)
        {
            return _ratings.TryGetValue(playerId + "\n" + item, out var stars) ? stars : 0;
        }

        internal void SetRating(string playerId, string item, int stars)
        {
            _ratings[playerId + "\n" + item] = stars;
        }

        public string OptionText(int displayIndex, string lang)
        {
            return Def.Options[DisplayOrder[displayIndex]].Get(lang);
        }
    }
}
