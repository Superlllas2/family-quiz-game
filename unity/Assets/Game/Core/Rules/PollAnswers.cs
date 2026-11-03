using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// Keeps the poll answers that fit content/poll.json (PROTOCOL.md, "poll answers"): a text question takes a string
    /// (trimmed, cut to 200 characters, left out when empty), a choice question takes a whole number option index in range.
    /// Unknown ids and answers of the wrong type are left out; the result is in poll.json order.
    /// </summary>
    public static class PollAnswers
    {
        public static List<KeyValuePair<string, object>> Clean(IReadOnlyList<PollQuestionDef> poll,
            IReadOnlyDictionary<string, object> answers)
        {
            var list = new List<KeyValuePair<string, object>>();
            if (poll == null || answers == null) return list;
            foreach (var q in poll)
            {
                if (!answers.TryGetValue(q.Id, out var value) || value == null) continue;
                if (q.IsChoice)
                {
                    if (TryIndex(value, out int index) && index >= 0 && q.Options != null && index < q.Options.Count)
                        list.Add(new KeyValuePair<string, object>(q.Id, index));
                }
                else if (value is string text)
                {
                    text = text.Trim();
                    if (text.Length > ContentLimits.PollTextAnswerMax) text = text.Substring(0, ContentLimits.PollTextAnswerMax);
                    if (text.Length > 0) list.Add(new KeyValuePair<string, object>(q.Id, text));
                }
            }

            return list;
        }

        private static bool TryIndex(object value, out int index)
        {
            index = -1;
            switch (value)
            {
                case int i:
                    index = i;
                    return true;
                case long l when l >= int.MinValue && l <= int.MaxValue:
                    index = (int)l;
                    return true;
                case double d when !double.IsNaN(d) && Math.Abs(d - Math.Round(d)) < 1e-9 && Math.Abs(d) < int.MaxValue:
                    index = (int)Math.Round(d);
                    return true;
                default:
                    return false;
            }
        }
    }
}
