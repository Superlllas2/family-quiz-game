using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// The awards on Results (docs/03_rules.md, "Winner and ties") and the seen lists across games and app restarts
    /// (docs/02_show_flow.md, "Seen questions").
    /// </summary>
    public sealed class AwardsAndSeenTests
    {
        [Test]
        public void BestComebackIsTheBiggestRankGainFromRound2ToTheEndOfTheRace()
        {
            var after2 = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 } };
            // Final order: d, a, c, b. Gains: d 4-1=3, a 1-2=-1, c 3-3=0, b 2-4=-2.
            Assert.That(Awards.BestComeback(after2, new[] { "d", "a", "c", "b" }), Is.EqualTo(new[] { "d" }));
        }

        [Test]
        public void BestComebackTiesAreAllShown()
        {
            var after2 = new Dictionary<string, int> { { "a", 1 }, { "b", 3 }, { "c", 3 }, { "d", 4 } };
            // Final order: b, d, a, c. Gains: b 2, d 2, a -2, c -1.
            Assert.That(Awards.BestComeback(after2, new[] { "b", "d", "a", "c" }), Is.EquivalentTo(new[] { "b", "d" }));
        }

        [Test]
        public void NoGainNoComeback()
        {
            var after2 = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            Assert.That(Awards.BestComeback(after2, new[] { "a", "b" }), Is.Empty);
        }

        [Test]
        public void ResultsHoldTheRaceOrderAndBothAwards()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(3);
            // Round 1: P1 and P2 right, P3 wrong. P3 is last after round 2.
            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Right, 5000);
            f.Choice("c3", f.Wrong, 0);
            f.SkipTo(GamePhase.FinalIntro);
            Assert.That(f.State.RanksAfterRound2[seats[2].PlayerId], Is.EqualTo(3));
            f.Advance(f.FinalIntroMs);
            // The race: only P3 answers, always right, and wins from step 0.
            while (f.State.Phase == GamePhase.RaceQuestion)
            {
                f.TrueFalse("c3", f.State.Race.Statement.Answer, 500);
                f.SkipTo(GamePhase.RaceReveal);
                f.Advance(f.RaceRevealMs);
            }

            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Results));
            Assert.That(f.State.WinnerId, Is.EqualTo(seats[2].PlayerId));
            Assert.That(f.State.Placement, Is.EqualTo(new[] { seats[2].PlayerId, seats[0].PlayerId, seats[1].PlayerId }));
            Assert.That(f.State.MostCorrectIds, Is.EquivalentTo(new[] { seats[0].PlayerId, seats[1].PlayerId }));
            Assert.That(f.State.BestComebackIds, Is.EqualTo(new[] { seats[2].PlayerId }));

            var ended = f.Advance(f.ResultsMs).OfType<GameEnded>().Single();
            Assert.That(ended.WinnerId, Is.EqualTo(seats[2].PlayerId), "the game winner is the race winner");
        }

        // ---------------------------------------------------------------- seen lists

        [Test]
        public void EveryShownQuestionIsMarkedSeenWhenShown()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(2);
            var events = f.SkipTo(GamePhase.QuestionShow);
            Assert.That(events.OfType<SeenChanged>().Count(), Is.EqualTo(1));
            Assert.That(f.State.SeenQuestionIds, Is.EquivalentTo(new[] { f.State.Question.Def.Id }));
        }

        [Test]
        public void SeenListsFromAnEarlierSessionAreKept()
        {
            var first = new LobbyFixture(11);
            first.SeatPlayers(2);
            first.ReadyAll();
            first.RunToLobby();
            var questions = first.State.SeenQuestionIds.ToList();
            var race = first.State.SeenRaceIds.ToList();
            Assert.That(questions.Count, Is.EqualTo(first.Config.Rounds.Count * first.Config.Rounds.QuestionsPerRound));

            // An app restart: a new Game gets the saved lists and avoids them.
            var second = new LobbyFixture(12, seenQuestions: questions, seenRace: race);
            Assert.That(second.State.SeenQuestionIds, Is.EquivalentTo(questions));
            second.SeatPlayers(2);
            second.ReadyAll();
            second.RunToLobby();
            var newQuestions = second.State.SeenQuestionIds.Except(questions).ToList();
            var newRace = second.State.SeenRaceIds.Except(race).ToList();
            Assert.That(newQuestions.Count, Is.EqualTo(questions.Count), "the second game repeats no question");
            Assert.That(newRace.Count, Is.GreaterThan(0));
            Assert.That(second.State.SeenRaceIds.Count, Is.EqualTo(race.Count + newRace.Count));
        }

        [Test]
        public void ResetSeenClearsBothListsAtOnce()
        {
            var f = new LobbyFixture(seenQuestions: new[] { "geo_001" }, seenRace: new[] { "race_001" });
            var events = f.Send(new ResetSeen());
            Assert.That(f.State.SeenQuestionIds, Is.Empty);
            Assert.That(f.State.SeenRaceIds, Is.Empty);
            Assert.That(events.OfType<SeenChanged>().Single().Reset, Is.True);
            var setting = events.OfType<SettingsChanged>().Single();
            Assert.That(setting.Source, Is.EqualTo(SettingsChanged.SourceTv));
            Assert.That(setting.Setting, Is.EqualTo(SettingsChanged.SettingResetSeen));
        }
    }
}
