using System;
using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// A broken config or content file. The message names the file, the item id and the field (docs/15_schemas.md),
    /// one line per error, for example "questions.json geo_009: difficulty must be easy, medium or hard".
    /// </summary>
    public sealed class ContentException : Exception
    {
        public ContentException(string file, string message, Exception inner = null)
            : base(file + ": " + message, inner)
        {
            File = file;
            Errors = new[] { Message };
        }

        public ContentException(string file, IReadOnlyList<string> errors)
            : base(string.Join("\n", errors))
        {
            File = file;
            Errors = errors;
        }

        public string File { get; }

        /// <summary>Every error found in the file, each naming the file, the item id and the field.</summary>
        public IReadOnlyList<string> Errors { get; }
    }
}
