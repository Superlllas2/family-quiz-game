namespace FamilyQuiz.Core
{
    /// <summary>The core clock. Injected so tests can run on a fake clock (CLAUDE.md).</summary>
    public interface IClock
    {
        /// <summary>Milliseconds since an arbitrary start. Never goes backwards.</summary>
        long NowMs { get; }
    }
}
