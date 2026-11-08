using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>Plain C# parts of the Day 4 TV views: host bubbles, the poll count, the settings panel, power up markers.</summary>
    public sealed class Day4TvPartsTests
    {
        private static PlayerState Player(GameState state, string id, bool connected)
        {
            var p = new PlayerState(id, id, "en", 0, "t" + id, 1);
            p.Connected = connected;
            state.MutablePlayers.Add(p);
            return p;
        }

        private static TvSettings Settings(bool enabled, string targeting, params string[] types)
        {
            var s = new TvSettings { PowerUpsEnabled = enabled, Targeting = targeting };
            s.SetTypes(types);
            return s;
        }

        // ------------------------------------------------------------ host bubbles

        [Test]
        public void ABubbleShowsUntilItsEndTimeOnTheCoreClock()
        {
            var slots = new HostBubbleSlots();
            Assert.IsTrue(slots.Show(HostIds.Konstantin, lang => "hi " + lang, 5000));
            Assert.IsTrue(slots.Visible(HostIds.Konstantin, 0));
            Assert.IsTrue(slots.Visible(HostIds.Konstantin, 4999));
            Assert.IsFalse(slots.Visible(HostIds.Konstantin, 5000));
            Assert.IsFalse(slots.Visible(HostIds.Ilja, 0));
        }

        [Test]
        public void ANewLineForTheSameHostReplacesTheBubbleAndTheOtherHostIsIndependent()
        {
            var slots = new HostBubbleSlots();
            slots.Show(HostIds.Konstantin, lang => "first", 5000);
            slots.Show(HostIds.Ilja, lang => "other", 3000);
            slots.Show(HostIds.Konstantin, lang => "second", 9000);
            Assert.AreEqual("second", slots.Text(HostIds.Konstantin, Languages.Russian));
            Assert.AreEqual(9000, slots.EndsAtMs(HostIds.Konstantin));
            Assert.IsTrue(slots.Visible(HostIds.Konstantin, 6000));
            Assert.AreEqual("other", slots.Text(HostIds.Ilja, Languages.Russian));
            Assert.IsFalse(slots.Visible(HostIds.Ilja, 6000));
        }

        [Test]
        public void TheTextFollowsTheLanguageAndUnknownHostsAreIgnored()
        {
            var slots = new HostBubbleSlots();
            slots.Show(HostIds.Ilja, lang => lang == Languages.English ? "Hello" : "Привет", 1000);
            Assert.AreEqual("Привет", slots.Text(HostIds.Ilja, Languages.Russian));
            Assert.AreEqual("Hello", slots.Text(HostIds.Ilja, Languages.English));
            Assert.IsFalse(slots.Show("narrator", lang => "x", 1000));
            Assert.IsFalse(slots.Show(HostIds.Ilja, null, 1000));
            Assert.IsNull(slots.Text("narrator", Languages.Russian));
            Assert.IsNull(slots.Text(HostIds.Konstantin, Languages.Russian));
        }

        // ------------------------------------------------------------ the poll count

        [Test]
        public void ThePollCountsSubmittedOfConnectedPlayers()
        {
            var s = new GameState();
            Player(s, "a", true);
            Player(s, "b", true);
            Player(s, "c", false); // gone without a poll: not waited for
            s.MutablePollSubmitted.Add("a");
            PollCount.Count(s, out int done, out int total);
            Assert.AreEqual(1, done);
            Assert.AreEqual(2, total);
        }

        [Test]
        public void APollSubmittedBeforeLosingTheConnectionStillCounts()
        {
            var s = new GameState();
            Player(s, "a", false);
            Player(s, "b", true);
            s.MutablePollSubmitted.Add("a");
            s.MutablePollSubmitted.Add("gone"); // a player who left the seat
            PollCount.Count(s, out int done, out int total);
            Assert.AreEqual(1, done);
            Assert.AreEqual(2, total);
        }

        [Test]
        public void NobodyConnectedCountsZeroOfZero()
        {
            var s = new GameState();
            Player(s, "a", false);
            PollCount.Count(s, out int done, out int total);
            Assert.AreEqual(0, done);
            Assert.AreEqual(0, total);
        }

        // ------------------------------------------------------------ the Results line

        [Test]
        public void TheResultsLineNamesThePollOnlyWhenItFollows()
        {
            Assert.AreEqual("tv.results.pollIn", ResultsCountdown.Key(true, 4));
            Assert.AreEqual("tv.results.lobbyIn", ResultsCountdown.Key(false, 4));
            Assert.AreEqual("tv.results.lobbyIn", ResultsCountdown.Key(true, 0));
        }

        // ------------------------------------------------------------ the settings panel

        [Test]
        public void BackToLobbyActsOnlyOnASecondClickInsideTheWindow()
        {
            var confirm = new ConfirmClick(3);
            Assert.IsFalse(confirm.Press(10));
            Assert.IsTrue(confirm.Armed(12.9));
            Assert.IsTrue(confirm.Press(12.9));
            Assert.IsFalse(confirm.Armed(12.9));

            Assert.IsFalse(confirm.Press(20));
            Assert.IsFalse(confirm.Armed(23));
            Assert.IsFalse(confirm.Press(23)); // too late: this is a new first click
            Assert.IsTrue(confirm.Press(24));

            Assert.IsFalse(confirm.Press(30));
            confirm.Cancel();
            Assert.IsFalse(confirm.Press(31));
        }

        [Test]
        public void SwitchingATypeSendsTheFullListInRuleOrder()
        {
            var current = new[] { PowerUpTypes.Fog, PowerUpTypes.Freeze };
            CollectionAssert.AreEqual(new[] { PowerUpTypes.Freeze, PowerUpTypes.Slime, PowerUpTypes.Fog },
                SettingsChoices.WithType(current, PowerUpTypes.Slime, true));
            CollectionAssert.AreEqual(new[] { PowerUpTypes.Freeze },
                SettingsChoices.WithType(current, PowerUpTypes.Fog, false));
            CollectionAssert.AreEqual(new[] { PowerUpTypes.Freeze, PowerUpTypes.Fog },
                SettingsChoices.WithType(current, PowerUpTypes.Fog, true));
            CollectionAssert.IsEmpty(SettingsChoices.WithType(new[] { PowerUpTypes.Slime }, PowerUpTypes.Slime, false));
        }

        [Test]
        public void TheNextGameNoteShowsOnlyForRowsThatDifferWhileAGameRuns()
        {
            var s = new GameState
            {
                Settings = Settings(true, TargetingModes.Free, PowerUpTypes.Freeze, PowerUpTypes.Slime, PowerUpTypes.Fog),
                PendingSettings = Settings(false, TargetingModes.AimUp, PowerUpTypes.Freeze, PowerUpTypes.Fog),
                GameRunning = true
            };
            Assert.IsTrue(SettingsChoices.PowerUpsDiffer(s));
            Assert.IsTrue(SettingsChoices.TargetingDiffers(s));
            Assert.IsFalse(SettingsChoices.TypeDiffers(s, PowerUpTypes.Freeze));
            Assert.IsTrue(SettingsChoices.TypeDiffers(s, PowerUpTypes.Slime));
            Assert.IsFalse(SettingsChoices.TypeDiffers(s, PowerUpTypes.Fog));

            s.GameRunning = false; // in the Lobby the pending values are simply what the next game takes
            Assert.IsFalse(SettingsChoices.PowerUpsDiffer(s));
            Assert.IsFalse(SettingsChoices.TargetingDiffers(s));
            Assert.IsFalse(SettingsChoices.TypeDiffers(s, PowerUpTypes.Slime));
        }

        // Power up markers: D0026TvPartsTests (every hit, attacker, power up and target).
    }
}
