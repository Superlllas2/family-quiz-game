using FamilyQuiz.Core;

namespace FamilyQuiz.Tests
{
    /// <summary>A fake clock the test moves by hand.</summary>
    public sealed class ManualClock : IClock
    {
        public long NowMs { get; private set; }

        public void Advance(long ms)
        {
            NowMs += ms;
        }
    }
}
