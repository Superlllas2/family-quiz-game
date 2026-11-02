using System;
using System.IO;
using System.Text;
using FamilyQuiz.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Telemetry
{
    /// <summary>
    /// The event log file (docs/15_schemas.md, "Event log"): one JSON object per line, UTF 8, flushed after every line.
    /// Every line starts with t (ms since the session started, from the core clock), session and build; seed, when
    /// given, comes before event. An I/O error never reaches the game: it is swallowed and the first one is kept.
    /// </summary>
    public sealed class EventLogWriter : IDisposable
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly string _path;
        private readonly string _session;
        private readonly string _build;
        private readonly IClock _clock;
        private readonly long _sessionStartMs;
        private readonly Action<string> _onFirstError;
        private StreamWriter _writer;
        private bool _disposed;

        /// <param name="path">The file, for example &lt;persistentDataPath&gt;/playtests/&lt;yyyy-mm-dd&gt;/events_&lt;session&gt;.jsonl. Folders are created.</param>
        /// <param name="session">The session id written on every line, for example 2026-11-07T19-30-00.</param>
        /// <param name="build">The build version written on every line.</param>
        /// <param name="clock">The core clock; t counts from its time when the writer is made.</param>
        /// <param name="onFirstError">Called once with a description of the first I/O error, for the TV log. May be null.</param>
        public EventLogWriter(string path, string session, string build, IClock clock, Action<string> onFirstError = null)
        {
            _path = path;
            _session = session;
            _build = build;
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _sessionStartMs = clock.NowMs;
            _onFirstError = onFirstError;
        }

        public string Path => _path;

        /// <summary>The first I/O error, or null. Later errors are not kept.</summary>
        public Exception FirstError { get; private set; }

        /// <summary>Lines written so far.</summary>
        public int LinesWritten { get; private set; }

        /// <summary>Milliseconds since the session started, from the core clock.</summary>
        public long SessionMs => _clock.NowMs - _sessionStartMs;

        /// <summary>Writes one line: t, session, build, seed (if given), event, then the fields in their order.</summary>
        public void Write(string eventName, JObject fields, int? seed = null)
        {
            if (_disposed) return;
            string line;
            try
            {
                var obj = new JObject
                {
                    ["t"] = SessionMs,
                    ["session"] = _session,
                    ["build"] = _build
                };
                if (seed.HasValue) obj["seed"] = seed.Value;
                obj["event"] = eventName;
                if (fields != null)
                {
                    foreach (var property in fields.Properties()) obj[property.Name] = property.Value.DeepClone();
                }

                line = obj.ToString(Formatting.None);
            }
            catch (Exception e)
            {
                Fail(e);
                return;
            }

            try
            {
                if (_writer == null) Open();
                _writer.Write(line);
                _writer.Write('\n');
                _writer.Flush();
                LinesWritten++;
            }
            catch (Exception e)
            {
                Fail(e);
                CloseQuietly();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            CloseQuietly();
        }

        private void Open()
        {
            var folder = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            var stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, Utf8NoBom);
        }

        private void CloseQuietly()
        {
            try
            {
                _writer?.Dispose();
            }
            catch (Exception)
            {
                // Closing a broken file must not throw either.
            }

            _writer = null;
        }

        private void Fail(Exception e)
        {
            if (FirstError != null) return;
            FirstError = e;
            try
            {
                _onFirstError?.Invoke("Event log " + _path + ": " + e.GetType().Name + ": " + e.Message);
            }
            catch (Exception)
            {
                // The log callback must not break the game either.
            }
        }
    }
}
