using System.Collections.Generic;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/topics.json (docs/15_schemas.md, tools/validate_content.mjs).</summary>
    public static class TopicLoader
    {
        public const string FileName = "topics.json";

        public static IReadOnlyList<TopicDef> Parse(string json)
        {
            var errors = new ContentErrors(FileName);
            var items = JsonItems.Convert<TopicDef>(JsonItems.ParseArray(FileName, json), errors);
            var list = new List<TopicDef>();
            var ids = new HashSet<string>();
            int mixed = 0;
            foreach (var (topic, _, id) in items)
            {
                errors.Check(ids.Add(topic.Id), id, "id is used twice");
                errors.CheckText(id, "title", topic.Title, ContentLimits.TopicTitleMax);
                if (topic.Mixed)
                {
                    mixed++;
                }
                else
                {
                    errors.Check(TopicShelves.IsShelf(topic.Shelf), id, "shelf must be screen, knowledge or life");
                    errors.Check(!string.IsNullOrEmpty(topic.Category), id, "category must not be empty");
                    errors.Check(TopicShelves.IsLean(topic.Lean), id, "lean must be young, old or all");
                }
                list.Add(topic);
            }

            errors.Check(mixed == 1, "-", "exactly one topic must be mixed");
            errors.ThrowIfAny();
            return list;
        }
    }
}
