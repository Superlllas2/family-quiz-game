using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>Character picks (PROTOCOL.md "character", docs/18_edge_cases.md) and the phone "settings" message.</summary>
    public sealed class CharacterAndSettingsTests
    {
        private static List<string> Names(IEnumerable<IGameEvent> events)
        {
            return LobbyFixture.FlowEvents(events).Select(LobbyFixture.Describe).ToList();
        }

        [Test]
        public void AJoinGetsTheFirstFreeCharacterNotYetPicked()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            Assert.That(a.CharacterId, Is.EqualTo(f.Characters[0].Id));
            Assert.That(a.CharacterPicked, Is.False);
        }

        [Test]
        public void PickingAFreeCharacter()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var events = f.Send(new PickCharacter("c1", "k1", 7));
            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(True)", "PlayerUpdated", "RosterChanged" }));
            Assert.That(a.CharacterId, Is.EqualTo(7));
            Assert.That(a.CharacterPicked, Is.True);
        }

        [Test]
        public void PickingYourOwnCharacterConfirmsIt()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c1", "k1", a.CharacterId))).Ok, Is.True);
            Assert.That(a.CharacterPicked, Is.True);
        }

        [Test]
        public void ACharacterJustTakenIsRefused()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var b = f.Seat("c2", "B");
            f.Send(new PickCharacter("c1", "k1", 4));
            var events = f.Send(new PickCharacter("c2", "k2", 4));
            Assert.That(Names(events), Is.EqualTo(new[] { "InputAcked(False, characterTaken)", "InputRefused" }));
            var refused = events.OfType<InputRefused>().Single();
            Assert.That(refused.PlayerId, Is.EqualTo(b.PlayerId));
            Assert.That(refused.Kind, Is.EqualTo(InputRefused.KindCharacter));
            Assert.That(refused.Reason, Is.EqualTo(AckReasons.CharacterTaken));
            Assert.That(b.CharacterId, Is.Not.EqualTo(4));
            Assert.That(b.CharacterPicked, Is.False);
            Assert.That(a.CharacterId, Is.EqualTo(4));
        }

        [Test]
        public void ACharacterFreedByLeaveCanBePicked()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            f.Seat("c2", "B");
            int taken = a.CharacterId;
            f.Send(new PlayerLeave("c1", "bye"));
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c2", "k1", taken))).Ok, Is.True);
        }

        [TestCase(-1)]
        [TestCase(999)]
        public void AnUnknownCharacterIsInvalid(int id)
        {
            var f = new LobbyFixture();
            f.Seat("c1", "A");
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c1", "k1", id))).Reason, Is.EqualTo(AckReasons.Invalid));
        }

        [Test]
        public void PicksOnlyInTheLobbyAlsoAfterAGame()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            Assert.That(f.State.Phase, Is.EqualTo(FamilyQuiz.Core.State.GamePhase.StartCountdown));
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c1", "k1", 9))).Reason, Is.EqualTo(AckReasons.Invalid), "countdown");
            f.Advance(f.CountdownMs);
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c1", "k2", 9))).Reason, Is.EqualTo(AckReasons.Invalid), "game");
            f.RunToLobby();
            Assert.That(LobbyFixture.AckOf(f.Send(new PickCharacter("c1", "k3", 9))).Ok, Is.True, "back in the Lobby");
            Assert.That(f.State.Players[0].CharacterId, Is.EqualTo(9));
        }

        [Test]
        public void APlayerWhoNeverPickedIsMarkedPickedOnceAGameStarts()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(seats.All(p => p.CharacterPicked), Is.True);
        }

        // ---------------------------------------------------------------- settings

        [Test]
        public void ChangingLanguageAndMultiplier()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            double option = f.Config.Players.TimerMultiplierOptions[1];
            var events = f.Send(new ChangeSettings("c1", "en", option));
            Assert.That(Names(events), Is.EqualTo(new[] { "SettingsChanged", "SettingsChanged", "PlayerUpdated" }));
            var changes = events.OfType<SettingsChanged>().ToList();
            Assert.That(changes.All(c => c.Source == SettingsChanged.SourcePhone && c.PlayerId == a.PlayerId), Is.True);
            Assert.That(changes[0].Setting, Is.EqualTo(SettingsChanged.SettingLang));
            Assert.That(changes[0].Value, Is.EqualTo("en"));
            Assert.That(changes[1].Setting, Is.EqualTo(SettingsChanged.SettingTimerMultiplier));
            Assert.That(changes[1].Value, Is.EqualTo(option.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            Assert.That(a.Lang, Is.EqualTo("en"));
            Assert.That(a.TimerMultiplier, Is.EqualTo(option));
        }

        [Test]
        public void OnlyChangedValuesAreReported()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var events = f.Send(new ChangeSettings("c1", a.Lang, a.TimerMultiplier));
            Assert.That(events, Is.Empty);
            events = f.Send(new ChangeSettings("c1", "en", a.TimerMultiplier));
            Assert.That(Names(events), Is.EqualTo(new[] { "SettingsChanged", "PlayerUpdated" }));
        }

        [TestCase("de", 1.75)]
        [TestCase(null, 0.0)]
        [TestCase("", -1.0)]
        public void InvalidValuesAreIgnored(string lang, double multiplier)
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            string before = a.Lang;
            double beforeMultiplier = a.TimerMultiplier;
            Assert.That(f.Send(new ChangeSettings("c1", lang, multiplier)), Is.Empty);
            Assert.That(a.Lang, Is.EqualTo(before));
            Assert.That(a.TimerMultiplier, Is.EqualTo(beforeMultiplier));
        }

        [Test]
        public void AnInvalidValueDoesNotStopAValidOne()
        {
            var f = new LobbyFixture();
            var a = f.Seat("c1", "A");
            var events = f.Send(new ChangeSettings("c1", "xx", f.Config.Players.TimerMultiplierOptions.Last()));
            Assert.That(events.OfType<SettingsChanged>().Single().Setting, Is.EqualTo(SettingsChanged.SettingTimerMultiplier));
            Assert.That(a.Lang, Is.EqualTo("ru"));
        }

        [Test]
        public void SettingsWithoutASeatAreIgnored()
        {
            var f = new LobbyFixture();
            Assert.That(f.Send(new ChangeSettings("nobody", "en", null)), Is.Empty);
        }
    }
}
