using System;

namespace FamilyQuiz.Core.Content
{
    /// <summary>A broken config or content file. The message names the file and the field (docs/15_schemas.md).</summary>
    public sealed class ContentException : Exception
    {
        public ContentException(string file, string message, Exception inner = null)
            : base(file + ": " + message, inner)
        {
            File = file;
        }

        public string File { get; }
    }
}
