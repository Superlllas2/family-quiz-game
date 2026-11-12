using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>The Intro, the topic vote in a running game, RoundEnd and the next game without Intro (docs/02_show_flow.md).</summary>
    public sealed class RoundFlowTests
    {
        [Test]
        public void TheFirstGameHasAnIntroThatNeverEndsEarly()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Intro));
            Assert.That(f.Game.EndsInMs(f.State.Players[0]), Is.EqualTo(f.IntroMs));
            f.Advance(f.IntroMs - 1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Intro));
            f.Advance(1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
        }

        [Test]
        public void TheNextGameFromTheLobbySkipsTheIntro()
        {
            var f = new LobbyFixture();
            f.SeatPlayers(2);
            f.ReadyAll();
            f.RunToLobby();
            f.ReadyAll();
            var events = f.Advance(f.CountdownMs);
            Assert.That(events.OfType<PhaseStarted>().Select(p => p.Phase), Is.EqualTo(new[] { GamePhase.TopicVote }));
            Assert.That(f.State.GameNumber, Is.EqualTo(2));
        }

        // ---------------------------------------------------------------- the topic vote

        [Test]
        public void TheVoteOffersFourDoorsWithTheMixedDoorLast()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(2);
            var round = f.State.Round;
            Assert.That(round.Doors.Count, Is.EqualTo(f.Config.Topics.Doors));
            Assert.That(round.Doors.Last().Id, Is.EqualTo(f.Config.Topics.MixedTopicId));
            Assert.That(f.State.Round.VoteDeadlineMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.TopicVoteMs));
            Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.VoteEndMs), "plus the grace (D84)");
        }

        [Test]
        public void TheVoteEndsTheMomentEveryConnectedPlayerVoted()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(3);
            f.Send(new ConnectionClosed("c3"));
            f.Advance(1200);
            var first = f.Vote("c1", 2);
            Assert.That(first.OfType<VoteCast>().Single().Door, Is.EqualTo(2));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
            var last = f.Vote("c2", 2);
            var ended = last.OfType<PhaseEnded>().First();
            Assert.That(ended.Phase, Is.EqualTo(GamePhase.TopicVote));
            Assert.That(ended.Reason, Is.EqualTo(PhaseEndReasons.AllDone));
            Assert.That(ended.DurationMs, Is.EqualTo(1200));
            var chosen = last.OfType<TopicChosen>().Single();
            Assert.That(chosen.Door, Is.EqualTo(2));
            Assert.That(chosen.ByTieBreak, Is.False);
            Assert.That(f.State.Round.ChosenDoor, Is.EqualTo(2));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
        }

        [Test]
        public void OneVotePerPlayerAndOnlyRealDoors()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(3);
            Assert.That(LobbyFixture.AckOf(f.Vote("c1", 4)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Vote("c1", -1)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.Vote("c1", 1)).Ok, Is.True);
            Assert.That(LobbyFixture.AckOf(f.Vote("c1", 0)).Reason, Is.EqualTo(AckReasons.Invalid), "no changing a vote");
            Assert.That(LobbyFixture.AckOf(f.Choice("c2", 1, 100)).Reason, Is.EqualTo(AckReasons.Invalid), "a choice is no vote");
            Assert.That(f.State.Round.Votes.Count, Is.EqualTo(1));
        }

        [Test]
        public void AVoteAfterTheVoteEndedIsClosed()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(2);
            long voteId = f.State.PhaseId;
            f.Advance(f.VoteEndMs);
            var late = f.Send(new SubmitAnswer("c1", "late", voteId, InputKinds.Vote, null, null, door: 0));
            Assert.That(LobbyFixture.AckOf(late).Reason, Is.EqualTo(AckReasons.Closed));
        }

        [Test]
        public void AVoteInsideTheGraceAfterTheTimerCounts()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(2);
            f.Advance(f.TopicVoteMs + f.GraceMs - 1);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicVote));
            Assert.That(LobbyFixture.AckOf(f.Vote("c1", 1)).Ok, Is.True, "sent at the buzzer, arrived inside the grace");
            var events = f.Advance(1);
            Assert.That(events.OfType<TopicChosen>().Single().Door, Is.EqualTo(1));
        }

        [Test]
        public void ThePhoneCountdownShowsTheVoteTimerWithoutTheGrace()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(2);
            Assert.That(f.Game.EndsInMs(seats[0]), Is.EqualTo(f.TopicVoteMs));
        }

        [Test]
        public void NobodyVotingRunsTheFullTimerAndASeededDoorWins()
        {
            var doors = new HashSet<int>();
            for (int seed = 1; seed <= 40; seed++)
            {
                var f = new LobbyFixture(seed);
                f.ToFirstVote(2);
                var events = f.Advance(f.VoteEndMs);
                var chosen = events.OfType<TopicChosen>().Single();
                Assert.That(chosen.NobodyVoted, Is.True);
                Assert.That(events.OfType<PhaseEnded>().First().Reason, Is.EqualTo(PhaseEndReasons.Timeout));
                doors.Add(chosen.Door);
            }

            Assert.That(doors, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void ATiedVoteIsPickedAtRandomAmongTheTiedDoorsAndTheSpinShowsIt()
        {
            // D153: P1 on door 0, P3 on door 2, P2 does not vote: a 1 to 1 tie, the seeded random picks 0 or 2.
            var winners = new HashSet<int>();
            for (int seed = 1; seed <= 30; seed++)
            {
                var f = new LobbyFixture(seed);
                f.ToFirstVote(3);
                f.Vote("c1", 0);
                f.Vote("c3", 2);
                var events = f.Advance(f.VoteEndMs);
                var chosen = events.OfType<TopicChosen>().Single();
                Assert.That(chosen.ByTieBreak, Is.True);
                Assert.That(chosen.Door, Is.EqualTo(0).Or.EqualTo(2));
                winners.Add(chosen.Door);
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.TopicTie));
                Assert.That(f.State.Round.TiedDoors, Is.EqualTo(new[] { 0, 2 }));
                Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.TopicTieMs));
                f.Advance(f.TopicTieMs);
                Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            }

            Assert.That(winners, Is.EquivalentTo(new[] { 0, 2 }));
        }

        [Test]
        public void AClearVoteHasNoSpin()
        {
            var f = new LobbyFixture(3);
            f.ToFirstVote(3);
            f.Vote("c1", 1);
            f.Vote("c2", 1);
            f.Vote("c3", 2);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow), "everyone voted: the vote ends at once, no TopicTie");
            Assert.That(f.State.Round.TiedDoors, Is.Empty);
        }

        [Test]
        public void TopicsChosenEarlierAreNotOfferedAgainWhileOthersRemain()
        {
            var f = new LobbyFixture(5);
            f.ToFirstVote(2);
            var chosen = new List<string>();
            for (int r = 0; r < f.Config.Rounds.Count; r++)
            {
                var doors = f.State.Round.Doors.Take(f.Config.Topics.Doors - 1).Select(d => d.Id).ToList();
                Assert.That(doors.Intersect(chosen), Is.Empty, "round " + (r + 1));
                f.Vote("c1", 0);
                f.Vote("c2", 0);
                chosen.Add(f.State.Round.Topic.Id);
                if (r + 1 < f.Config.Rounds.Count) f.SkipTo(GamePhase.TopicVote);
            }
        }

        // ---------------------------------------------------------------- round end

        [Test]
        public void EveryRoundEndsWithRoundEndOnItsTimer()
        {
            var f = new LobbyFixture();
            f.ToFirstVote(2);
            f.SkipTo(GamePhase.RoundEnd);
            Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.RoundEndMs));
            Assert.That(f.State.Question, Is.Null);
            Assert.That(f.State.Round.Index, Is.EqualTo(0));
            var events = f.Advance(f.RoundEndMs);
            Assert.That(events.OfType<PhaseStarted>().Single().Phase, Is.EqualTo(GamePhase.TopicVote));
            Assert.That(f.State.Round.Index, Is.EqualTo(1));
        }

        [Test]
        public void TheRanksAtTheEndOfRound2AreKeptForBestComeback()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(2);
            f.SkipTo(GamePhase.Answer);
            f.Choice("c2", f.Right, 0);
            f.Choice("c1", f.Wrong, 0);
            f.SkipTo(GamePhase.TopicVote); // round 2
            Assert.That(f.State.RanksAfterRound2, Is.Empty);
            f.SkipTo(GamePhase.RoundEnd);
            Assert.That(f.State.RanksAfterRound2[seats[0].PlayerId], Is.EqualTo(2));
            Assert.That(f.State.RanksAfterRound2[seats[1].PlayerId], Is.EqualTo(1));
        }

        [Test]
        public void ThePointsOfARoundAreSummedPerPlayer()
        {
            var f = new LobbyFixture();
            var seats = f.ToFirstVote(2);
            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Wrong, 0);
            f.SkipTo(GamePhase.RoundEnd);
            Assert.That(f.State.Round.Points[seats[0].PlayerId], Is.EqualTo(seats[0].Score));
            Assert.That(f.State.Round.Points[seats[1].PlayerId], Is.EqualTo(0));
        }
    }
}
