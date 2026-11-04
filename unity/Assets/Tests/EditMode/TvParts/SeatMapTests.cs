using System.IO;
using FamilyQuiz.Presentation.Staging;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>
    /// Seats on the 3D set (docs/21_staging.md, "Seats"): a new player takes the first free seat in fill order, keeps
    /// it while in the roster (also through the return to the Lobby), and frees it on leaving.
    /// </summary>
    public sealed class SeatMapTests
    {
        [Test]
        public void PlayersFillTheSeatsInOrder()
        {
            var map = new SeatMap(8);
            map.Sync(new[] { "a", "b", "c" });
            Assert.That(map.SeatOf("a"), Is.EqualTo(0));
            Assert.That(map.SeatOf("b"), Is.EqualTo(1));
            Assert.That(map.SeatOf("c"), Is.EqualTo(2));
        }

        [Test]
        public void APlayerKeepsTheSeatWhenOthersLeave()
        {
            var map = new SeatMap(8);
            map.Sync(new[] { "a", "b", "c" });
            map.Sync(new[] { "a", "c" });
            Assert.That(map.SeatOf("c"), Is.EqualTo(2));
            Assert.That(map.SeatOf("b"), Is.EqualTo(-1));
        }

        [Test]
        public void ANewPlayerTakesTheFirstFreedSeat()
        {
            var map = new SeatMap(8);
            map.Sync(new[] { "a", "b", "c" });
            map.Sync(new[] { "a", "c" });
            map.Sync(new[] { "a", "c", "d" });
            Assert.That(map.SeatOf("d"), Is.EqualTo(1));
        }

        [Test]
        public void SyncReportsWhoSatDownAndWhoLeft()
        {
            var map = new SeatMap(8);
            var first = map.Sync(new[] { "a", "b" });
            Assert.That(first.Joined, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(first.Left, Is.Empty);
            var second = map.Sync(new[] { "b", "c" });
            Assert.That(second.Joined, Is.EqualTo(new[] { "c" }));
            Assert.That(second.Left, Is.EqualTo(new[] { "a" }));
        }

        [Test]
        public void NoSeatPastTheLast()
        {
            var map = new SeatMap(2);
            map.Sync(new[] { "a", "b", "c" });
            Assert.That(map.SeatOf("c"), Is.EqualTo(-1));
        }

        [Test]
        public void TheToggleIsOnUntilSwitchedOffAndRemembersIt()
        {
            string path = Path.Combine(Path.GetTempPath(), "studio_toggle_test_" + System.Guid.NewGuid() + ".json");
            try
            {
                var file = new StudioToggleFile(path);
                Assert.That(file.Load(), Is.True, "the 3D set is on by default");
                file.Save(false);
                Assert.That(new StudioToggleFile(path).Load(), Is.False);
                file.Save(true);
                Assert.That(new StudioToggleFile(path).Load(), Is.True);
                File.WriteAllText(path, "{broken");
                Assert.That(new StudioToggleFile(path).Load(), Is.True, "a broken file falls back to on");
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
