using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/poll.json (docs/15_schemas.md, docs/20_playtest.md).</summary>
    public static class PollLoader
    {
        public const string FileName = "poll.json";

        public static IReadOnlyList<PollQuestionDef> Parse(string json)
        {
            var errors = new ContentErrors(FileName);
            var items = JsonItems.Convert<PollQuestionDef>(JsonItems.ParseArray(FileName, json), errors);
            var list = new List<PollQuestionDef>();
            var ids = new HashSet<string>();
            foreach (var (q, _, id) in items)
            {
                errors.Check(ids.Add(q.Id), id, "id is used twice");
                errors.CheckText(id, "text", q.Text, ContentLimits.PollQuestionTextMax);
                if (q.Kind == PollQuestionDef.KindChoice)
                {
                    if (q.Options == null || q.Options.Count < 2)
                        errors.Add(id, "options must list 2 or more choices");
                    else
                        for (int i = 0; i < q.Options.Count; i++)
                            errors.CheckText(id, "options[" + i + "]", q.Options[i], ContentLimits.PollOptionTextMax);
                }
                else if (q.Kind == PollQuestionDef.KindText)
                {
                    errors.Check(q.Options == null || q.Options.Count == 0, id, "options are only for choice questions");
                }
                else
                {
                    errors.Add(id, "kind must be choice or text");
                }

                list.Add(q);
            }

            errors.Check(list.Count >= 1, "-", "needs at least one question");
            errors.ThrowIfAny();
            return list;
        }
    }
}
