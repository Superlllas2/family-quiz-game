using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>The ordered seen lists (docs/02_show_flow.md, "Seen questions"; D155).</summary>
    public sealed class SeenListTests
    {
        [Test]
        public void ItemsKeepTheOrderTheyWereSeenAndSeeingAgainMovesToTheEnd()
        {
            var list = new SeenList();
            foreach (var id in new[] { "b", "a", "c" }) list.Add(id);
            Assert.That(list.ToList(), Is.EqualTo(new[] { "b", "a", "c" }));
            list.Add("b");
            Assert.That(list.ToList(), Is.EqualTo(new[] { "a", "c", "b" }));
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list.Contains("a"), Is.True);
            Assert.That(list.Contains("x"), Is.False);
            Assert.That(list.LastSeen("a"), Is.LessThan(list.LastSeen("b")));
            Assert.That(list.LastSeen("x"), Is.Null);
        }

        [Test]
        public void ClearEmptiesTheList()
        {
            var list = new SeenList { "a", "b" };
            list.Clear();
            Assert.That(list, Is.Empty);
            list.Add("c");
            Assert.That(list.ToList(), Is.EqualTo(new[] { "c" }));
        }

        [Test]
        public void AnyOtherCollectionCountsEverySeenItemAsEquallyOld()
        {
            var set = new HashSet<string> { "a" };
            Assert.That(SeenList.LastSeenIn(set, "a"), Is.EqualTo(0));
            Assert.That(SeenList.LastSeenIn(set, "b"), Is.Null);
        }
    }
}
