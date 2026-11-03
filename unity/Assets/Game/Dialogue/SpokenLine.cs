using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Dialogue
{
    /// <summary>
    /// A host line that plays: one text bubble from StartMs to EndMs (StartMs + hosts.bubbleSeconds) on the core clock.
    /// The TV draws it in the current game language, which can change while the bubble shows, so the text is filled on
    /// every call to <see cref="Text"/>.
    /// </summary>
    public sealed class SpokenLine
    {
        public const string NamePlaceholder = "{name}";
        public const string TargetPlaceholder = "{target}";

        public SpokenLine(HostLineDef def, string name, string target, int cycle, long startMs, long endMs)
        {
            Def = def;
            Name = name;
            Target = target;
            Cycle = cycle;
            StartMs = startMs;
            EndMs = endMs;
        }

        /// <summary>The line from host_lines.json.</summary>
        public HostLineDef Def { get; }

        public string LineId => Def.Id;
        public string Host => Def.Host;
        public string Trigger => Def.Trigger;

        /// <summary>The player name for {name}, or null.</summary>
        public string Name { get; }

        /// <summary>The target name for {target} (powerUpUsed), or null.</summary>
        public string Target { get; }

        /// <summary>The question number of the request for a question cycle trigger; 0 for the others.</summary>
        public int Cycle { get; }

        /// <summary>Core clock time the bubble appears.</summary>
        public long StartMs { get; }

        /// <summary>Core clock time the bubble goes away.</summary>
        public long EndMs { get; }

        /// <summary>The text in the language with {name} and {target} filled in (a missing name or target becomes empty).</summary>
        public string Text(string lang)
        {
            return Def.Text.Get(lang)
                .Replace(NamePlaceholder, Name ?? string.Empty)
                .Replace(TargetPlaceholder, Target ?? string.Empty);
        }
    }
}
