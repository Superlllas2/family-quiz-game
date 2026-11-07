using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// Parses and checks the game's question bank, content/questions_refined.json (D130; docs/15_schemas.md). The full
    /// bank content/questions.json is the source the refining works from and is not loaded by the game.
    /// </summary>
    public static class QuestionLoader
    {
        public const string FileName = "questions_refined.json";

        public static IReadOnlyList<QuestionDef> Parse(string json, IReadOnlyList<TopicDef> topics, string fileName = FileName)
        {
            var errors = new ContentErrors(fileName);
            var items = JsonItems.Convert<QuestionDef>(JsonItems.ParseArray(fileName, json), errors);
            var topicById = new Dictionary<string, TopicDef>();
            foreach (var t in topics) topicById[t.Id] = t;

            var list = new List<QuestionDef>();
            var ids = new HashSet<string>();
            foreach (var (q, _, id) in items)
            {
                errors.Check(ids.Add(q.Id), id, "id is used twice");
                errors.Check(topicById.TryGetValue(q.Topic, out var topic) && !topic.Mixed, id,
                    "topic \"" + q.Topic + "\" is unknown or the mixed topic");
                errors.Check(Difficulties.IsValid(q.Difficulty), id, "difficulty must be easy, medium or hard");
                errors.CheckText(id, "text", q.Text, ContentLimits.QuestionTextMax);
                if (q.Fact != null) errors.CheckText(id, "fact", q.Fact, 0);
                errors.Check(!string.IsNullOrWhiteSpace(q.Source), id, "source is missing");
                if (q.Format == QuestionDef.MultipleChoice) CheckMultipleChoice(q, id, errors);
                else if (q.Format == QuestionDef.ClosestValue) CheckClosestValue(q, id, errors);
                else errors.Add(id, "format must be multipleChoice or closestValue");
                list.Add(q);
            }

            errors.ThrowIfAny();
            return list;
        }

        private static void CheckMultipleChoice(QuestionDef q, string id, ContentErrors errors)
        {
            if (q.Options == null || q.Options.Count != ContentLimits.OptionCount)
            {
                errors.Add(id, "options must be a list of " + ContentLimits.OptionCount);
            }
            else
            {
                for (int i = 0; i < q.Options.Count; i++)
                    errors.CheckText(id, "options[" + i + "]", q.Options[i], ContentLimits.OptionTextMax);
            }

            errors.Check(q.Correct >= 0 && q.Correct < ContentLimits.OptionCount, id,
                "correct must be 0 to " + (ContentLimits.OptionCount - 1));
        }

        private static void CheckClosestValue(QuestionDef q, string id, ContentErrors errors)
        {
            bool rangeOk = q.Range != null && q.Range.Length == 2;
            errors.Check(rangeOk, id, "range must be [min, max]");
            errors.Check(q.Answer.HasValue, id, "answer is missing");
            if (rangeOk && q.Answer.HasValue)
                errors.Check(q.Range[0] < q.Answer.Value && q.Answer.Value < q.Range[1], id, "answer must lie strictly inside range");
            errors.Check(q.Step.HasValue && q.Step.Value > 0, id, "step must be more than 0");
            errors.Check(q.Tolerance.HasValue && q.Tolerance.Value > 0, id, "tolerance must be more than 0");
            errors.CheckText(id, "unit", q.Unit, 0, allowEmpty: true);
        }
    }
}
