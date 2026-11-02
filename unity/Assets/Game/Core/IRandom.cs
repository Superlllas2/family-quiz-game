namespace FamilyQuiz.Core
{
    /// <summary>Random numbers, injected so seeds replay games exactly (CLAUDE.md).</summary>
    public interface IRandom
    {
        /// <summary>A number from 0 to maxExclusive - 1.</summary>
        int Next(int maxExclusive);
    }
}
