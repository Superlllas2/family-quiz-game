namespace FamilyQuiz.Core.Events
{
    /// <summary>
    /// A host line trigger fired (docs/15_schemas.md, "Triggers for game night 1"). Dialogue decides whether a line plays
    /// (priority, one line per question cycle, host, gap, bags); Core only reports what happened. When several players
    /// fit {name}, Core already picked one with the seeded random generator.
    /// </summary>
    public sealed class HostLineRequested : IGameEvent
    {
        public HostLineRequested(string trigger, string name = null, string target = null, int cycle = 0)
        {
            Trigger = trigger;
            Name = name;
            Target = target;
            Cycle = cycle;
        }

        public string Trigger { get; }

        /// <summary>The player name for {name}, or null.</summary>
        public string Name { get; }

        /// <summary>The target name for {target} (powerUpUsed), or null.</summary>
        public string Target { get; }

        /// <summary>The question number (1 based in the game) for the question cycle triggers; 0 for the others.</summary>
        public int Cycle { get; }
    }
}
