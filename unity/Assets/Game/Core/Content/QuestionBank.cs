using System.Collections.Generic;
using System.Linq;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// The game's question bank (D130, content/questions_refined.json): its questions and the topics a vote can offer from
    /// it (the topics that have questions in it, in topics.json order, and the mixed topic).
    /// </summary>
    public sealed class QuestionBank
    {
        public QuestionBank(IReadOnlyList<QuestionDef> questions, IReadOnlyList<TopicDef> allTopics)
        {
            Questions = questions;
            var used = new HashSet<string>(questions.Select(q => q.Topic));
            Topics = allTopics.Where(t => t.Mixed || used.Contains(t.Id)).ToList();
        }

        public IReadOnlyList<QuestionDef> Questions { get; }
        public IReadOnlyList<TopicDef> Topics { get; }
    }
}
