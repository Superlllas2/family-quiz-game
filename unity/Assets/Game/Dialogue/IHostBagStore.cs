using System.Collections.Generic;

namespace FamilyQuiz.Dialogue
{
    /// <summary>
    /// Where the host line shuffle bags live between evenings (docs/15_schemas.md, "Selection" rule 3). The bags are kept
    /// as the set of line ids shown since their bag was last refilled; each id belongs to one (trigger, host) bag through
    /// its line in host_lines.json.
    /// </summary>
    public interface IHostBagStore
    {
        /// <summary>The shown line ids; empty when nothing was saved. Never throws.</summary>
        IReadOnlyCollection<string> Load();

        /// <summary>Saves every shown line id (the whole set, after every line). Never throws.</summary>
        void Save(IReadOnlyCollection<string> shownIds);
    }
}
