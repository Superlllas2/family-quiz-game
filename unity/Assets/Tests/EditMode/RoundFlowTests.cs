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

        [Test]
        public void APredictionInTheIntroIsAcceptedOnce()
        {
            var f = new LobbyFixture();
            var seats = f.SeatPlayers(2);
            f.ReadyAll();
            f.Advance(f.CountdownMs);
            Assert.That(f.Game.AnyoneCanAnswer, Is.True, "a prediction is an input");
            var ok = f.Send(new SubmitAnswer("c1", "p1", f.State.PhaseId, InputKinds.Prediction, null, null, playerId: seats[1].PlayerId));
            Assert.That(LobbyFixture.AckOf(ok).Ok, Is.True);
            Assert.That(ok.OfType<PredictionMade>().Single().PredictedPlayerId, Is.EqualTo(seats[1].PlayerId));
            var again = f.Send(new SubmitAnswer("c1", "p2", f.State.PhaseId, InputKinds.Prediction, null, null, playerId: seats[0].PlayerId));
            Assert.That(LobbyFixture.AckOf(again).Reason, Is.EqualTo(AckReasons.Invalid));
            var unknown = f.Send(new SubmitAnswer("c2", "p3", f.State.PhaseId, InputKinds.Prediction, null, null, playerId: "nobody"));
            Assert.That(LobbyFixture.AckOf(unknown).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Intro), "the Intro never ends early");
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
            Assert.That(f.State.PhaseEndsAtMs - f.State.PhaseStartedAtMs, Is.EqualTo(f.TopicVoteMs));
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
            f.Advance(f.TopicVoteMs);
            var late = f.Send(new SubmitAnswer("c1", "late", voteId, InputKinds.Vote, null, null, door: 0));
            Assert.That(LobbyFixture.AckOf(late).Reason, Is.EqualTo(AckReasons.Closed));
        }

        [Test]
        public void NobodyVotingRunsTheFullTimerAndASeededDoorWins()
        {
            var doors = new HashSet<int>();
            for (int seed = 1; seed <= 40; seed++)
            {
                var f = new LobbyFixture(seed);
                f.ToFirstVote(2);
                var events = f.Advance(f.TopicVoteMs);
                var chosen = events.OfType<TopicChosen>().Single();
                Assert.That(chosen.NobodyVoted, Is.True);
                Assert.That(events.OfType<PhaseEnded>().First().Reason, Is.EqualTo(PhaseEndReasons.Timeout));
                doors.Add(chosen.Door);
            }

            Assert.That(doors, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void ATiedVoteGoesToTheLowestRankedVoterInAGame()
        {
            var f = new LobbyFixture(3);
            var seats = f.ToFirstVote(3);
            // Round 1: P1 answers right, P2 and P3 do not: ranks 1, 2, 2.
            f.Vote("c1", 0);
            f.Vote("c2", 0);
            f.Vote("c3", 0);
            f.SkipTo(GamePhase.Answer);
            f.Choice("c1", f.Right, 0);
            f.Choice("c2", f.Wrong, 0);
            f.Choice("c3", f.Wrong, 0);
            f.SkipTo(GamePhase.TopicVote);
            Assert.That(Ranking.SharedRank(f.State, seats[0]), Is.EqualTo(1));
            Assert.That(Ranking.SharedRank(f.State, seats[2]), Is.EqualTo(2));

            // Round 2: P1 on door 0, P3 on door 2, P2 does not vote: a 1 to 1 tie, P3 is ranked lower and decides.
            f.Vote("c1", 0);
            f.Vote("c3", 2);
            var events = f.Advance(f.TopicVoteMs);
            var chosen = events.OfType<TopicChosen>().Single();
            Assert.That(chosen.ByTieBreak, Is.True);
            Assert.That(chosen.Door, Is.EqualTo(2));
            Assert.That(chosen.Round, Is.EqualTo(2));
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
