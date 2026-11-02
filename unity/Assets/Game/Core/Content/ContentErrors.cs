using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Collects the errors of one file, then throws them together as one <see cref="ContentException"/>.</summary>
    internal sealed class ContentErrors
    {
        private readonly string _file;
        private readonly List<string> _errors = new List<string>();

        public ContentErrors(string file)
        {
            _file = file;
        }

        public bool Any => _errors.Count > 0;

        /// <summary>Adds "file item: message"; the message starts with the field it is about.</summary>
        public void Add(string item, string message)
        {
            _errors.Add(_file + " " + (string.IsNullOrEmpty(item) ? "-" : item) + ": " + message);
        }

        public void Check(bool ok, string item, string message)
        {
            if (!ok) Add(item, message);
        }

        /// <summary>
        /// A localized field: both ru and en present, non empty unless allowEmpty, and at most max characters per
        /// language when max is above 0.
        /// </summary>
        public void CheckText(string item, string field, LocalizedText text, int max, bool allowEmpty = false)
        {
            if (text == null || text.Ru == null || text.En == null)
            {
                Add(item, field + " must have ru and en");
                return;
            }

            if (!allowEmpty && (text.Ru.Trim().Length == 0 || text.En.Trim().Length == 0))
                Add(item, field + " must have non empty ru and en");
            if (max > 0 && (text.Ru.Length > max || text.En.Length > max))
                Add(item, field + " must be at most " + max + " characters per language");
        }

        public void ThrowIfAny()
        {
            if (_errors.Count > 0) throw new ContentException(_file, _errors);
        }
    }
}
