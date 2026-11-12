using System;
using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Commands;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Core.State;
using NUnit.Framework;

namespace FamilyQuiz.Tests
{
    /// <summary>
    /// Power ups (docs/03_rules.md, "Power ups"; docs/19_acceptance.md, Day 4): draw chances by shared rank, free and
    /// aimUp targeting, caps and immunity, the pick window over Reveal and Scores on one phaseId, effects landing at the
    /// next Answer, the ack reasons, nothing on the last question of the last round, and the TV settings.
    /// </summary>
    public sealed class PowerUpTests
    {
        private static GameContent _alwaysDraw;

        /// <summary>The real content with powerUps.drawChanceLeader 1: every correct answer draws.</summary>
        private static GameContent AlwaysDraw =>
            _alwaysDraw ?? (_alwaysDraw = TestContent.WithConfig(c => c["powerUps"]["drawChanceLeader"] = 1.0));

        private static LobbyFixture Drawing()
        {
            return new LobbyFixture(content: AlwaysDraw);
        }

        // ---------------------------------------------------------------- the draw chance

        [TestCase(1, 6, 0.30)]
        [TestCase(2, 6, 0.44)]
        [TestCase(4, 6, 0.72)]
        [TestCase(6, 6, 1.00)]
        [TestCase(1, 2, 0.30)]
        [TestCase(2, 2, 1.00)]
        [TestCase(1, 1, 0.00)]
        public void TheChanceFollowsTheFormula(int rank, int players, double expected)
        {
            Assert.That(PowerUpDealer.Chance(rank, players, 0.3), Is.EqualTo(expected).Within(1e-9));
        }

        [Test]
        public void TenThousandDrawsPerRankMatchTheFormulaWithinTwoPercent()
        {
            var config = TestContent.Config().PowerUps;
            const int players = 6;
            const int draws = 10000;
            var random = new SeededRandom(2026);
            for (int rank = 1; rank <= players; rank++)
            {
                double chance = PowerUpDealer.Chance(rank, players, config.DrawChanceLeader);
                var counts = PowerUpTypes.All.ToDictionary(t => t, t => 0);
                int hits = 0;
                for (int i = 0; i < draws; i++)
                {
                    var drawn = PowerUpDealer.Draw(random, chance, config.Types);
                    if (drawn == null) continue;
                    hits++;
                    counts[drawn]++;
                }

                double expected = config.DrawChanceLeader + (1 - config.DrawChanceLeader) * (rank - 1) / (players - 1);
                Assert.That((double)hits / draws, Is.EqualTo(expected).Within(0.02), "rank " + rank);
                if (hits > 1000)
                    foreach (var t in PowerUpTypes.All)
                        Assert.That((double)counts[t] / hits, Is.EqualTo(1.0 / 3).Within(0.02), "type " + t + " at rank " + rank);
            }
        }

        [Test]
        public void NoTypeOrNoChanceDrawsNothingAndRollsNothing()
        {
            var random = new CountingRandom();
            Assert.That(PowerUpDealer.Draw(random, 0, PowerUpTypes.All), Is.Null);
            Assert.That(PowerUpDealer.Draw(random, 1, new string[0]), Is.Null);
            Assert.That(random.Calls, Is.EqualTo(0));
            Assert.That(PowerUpDealer.Draw(random, 1, new[] { PowerUpTypes.Fog }), Is.EqualTo(PowerUpTypes.Fog));
        }

        private sealed class CountingRandom : IRandom
        {
            public int Calls;

            public int Next(int maxExclusive)
            {
                Calls++;
                return 0;
            }
        }

        // ---------------------------------------------------------------- the draw in a game

        [Test]
        public void ACorrectAnswerDrawsAtRevealWithTheRankBeforeTheQuestion()
        {
            var f = Drawing();
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 1000);
            f.Choice("c2", f.Wrong, 1000);
            var events = f.Choice("c3", f.Right, 2000);

            var drawn = events.OfType<PowerUpDrawn>().ToList();
            Assert.That(drawn.Select(d => d.PlayerId), Is.EqualTo(new[] { seats[0].PlayerId, seats[2].PlayerId }));
            Assert.That(drawn.All(d => d.Rank == 1 && Math.Abs(d.Chance - 1) < 1e-9), Is.True);
            Assert.That(seats[0].HeldPowerUp, Is.Not.Null);
            Assert.That(seats[1].HeldPowerUp, Is.Null, "a wrong answer never draws");
            Assert.That(PowerUpTypes.All, Does.Contain(seats[0].HeldPowerUp));
            Assert.That(events.OfType<PlayerUpdated>().Select(u => u.PlayerId), Does.Contain(seats[0].PlayerId));
        }

        [Test]
        public void APlayerHoldsAtMostOne()
        {
            var f = Drawing();
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 1000);
            f.Choice("c2", f.Wrong, 1000);
            var held = seats[0].HeldPowerUp;
            Assert.That(held, Is.Not.Null);
            f.RunUntilPhase(GamePhase.Answer, 60000);
            f.Choice("c1", f.Right, 1000);
            var events = f.Choice("c2", f.Wrong, 1000);
            Assert.That(events.OfType<PowerUpDrawn>(), Is.Empty, "a new draw while holding one is skipped");
            Assert.That(seats[0].HeldPowerUp, Is.EqualTo(held));
        }

        [Test]
        public void WithOnePlayerThereAreNoDraws()
        {
            var f = Drawing();
            var seats = f.SeatPlayers(2);
            f.Send(new PlayerLeave("c2", f.NextInputId()));
            f.ReadyAll(); // one player is below players.min: no countdown
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Lobby));
            Assert.That(PowerUpDealer.Chance(1, 1, 1.0), Is.EqualTo(0));
            Assert.That(seats[0].HeldPowerUp, Is.Null);
        }

        // ---------------------------------------------------------------- using

        /// <summary>Q1 of a drawing game with three players: c1 and c3 right (c1 faster), c2 wrong. Reveal is open.</summary>
        private static (LobbyFixture F, PlayerState[] Seats) FirstPickWindow(string targeting = null)
        {
            var f = Drawing();
            if (targeting != null) f.SetPowerUps(true, targeting: targeting);
            var seats = f.ToFirstAnswer(3);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Wrong, 1000);
            f.Choice("c3", f.Right, 9000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            return (f, seats);
        }

        [Test]
        public void TheHolderUsesItOnATargetInReveal()
        {
            var (f, seats) = FirstPickWindow();
            long window = f.State.PhaseId;
            var powerUp = seats[0].HeldPowerUp;
            Assert.That(f.Game.PickWindowOpen, Is.True);
            Assert.That(f.Game.AnyoneCanAnswer, Is.True, "a holder with a target can still send an input");

            var events = f.UsePowerUp("c1", powerUp, seats[1].PlayerId);
            Assert.That(LobbyFixture.AckOf(events).Ok, Is.True);
            var used = events.OfType<PowerUpUsed>().Single();
            Assert.That(used.PlayerId, Is.EqualTo(seats[0].PlayerId));
            Assert.That(used.TargetId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(used.PowerUp, Is.EqualTo(powerUp));
            Assert.That(used.PhaseId, Is.EqualTo(window));
            Assert.That(seats[0].HeldPowerUp, Is.Null);
            Assert.That(seats[1].PendingEffects.Single().AttackerName, Is.EqualTo("P1"));
            var line = events.OfType<HostLineRequested>().Single();
            Assert.That(line.Trigger, Is.EqualTo(HostTriggers.PowerUpUsed));
            Assert.That(line.Name, Is.EqualTo("P1"));
            Assert.That(line.Target, Is.EqualTo("P2"));
            Assert.That(line.Cycle, Is.EqualTo(1));
        }

        [Test]
        public void ThePickWorksInScoresOnTheSamePhaseId()
        {
            var (f, seats) = FirstPickWindow();
            long window = f.State.PhaseId;
            f.Advance(f.RevealMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Scores));
            Assert.That(f.State.PhaseId, Is.EqualTo(window));
            var events = f.UsePowerUp("c3", seats[2].HeldPowerUp, seats[0].PlayerId);
            Assert.That(LobbyFixture.AckOf(events).Ok, Is.True);
        }

        [Test]
        public void AnAttackAddsTheAttacksPhaseOnTheSamePhaseIdAndThePickStillCountsThere()
        {
            // D149: a power up used in Reveal or Scores brings the Attacks phase after Scores.
            var (f, seats) = FirstPickWindow();
            long window = f.State.PhaseId;
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", seats[0].HeldPowerUp, seats[1].PlayerId)).Ok, Is.True);
            f.Advance(f.RevealMs);
            var events = f.Advance(f.ScoresMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Attacks));
            Assert.That(f.State.PhaseId, Is.EqualTo(window), "Attacks keeps the pick window's phaseId");
            Assert.That(events.OfType<PhaseStarted>().Single().BudgetMs, Is.EqualTo(f.AttacksMs));
            Assert.That(f.Game.PickWindowOpen, Is.True);
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c3", seats[2].HeldPowerUp, seats[0].PlayerId)).Ok, Is.True, "a late pick");
            f.Advance(f.AttacksMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            var late = f.Send(new UsePowerUp("c2", f.NextInputId(), window, PowerUpTypes.Fog, seats[0].PlayerId));
            Assert.That(LobbyFixture.AckOf(late).Reason, Is.EqualTo(AckReasons.Closed).Or.EqualTo(AckReasons.NoPowerUp));
        }

        [Test]
        public void AfterScoresThePickIsClosed()
        {
            var (f, seats) = FirstPickWindow();
            long window = f.State.PhaseId;
            var powerUp = seats[0].HeldPowerUp;
            f.Advance(f.RevealMs);
            f.Advance(f.ScoresMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.QuestionShow));
            var events = f.Send(new UsePowerUp("c1", f.NextInputId(), window, powerUp, seats[1].PlayerId));
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Closed));
            events = f.Send(new UsePowerUp("c1", f.NextInputId(), window - 1, powerUp, seats[1].PlayerId));
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.Stale));
            Assert.That(seats[0].HeldPowerUp, Is.EqualTo(powerUp), "kept for a later question");
        }

        [Test]
        public void NotHoldingItGivesNoPowerUp()
        {
            var (f, seats) = FirstPickWindow();
            var events = f.UsePowerUp("c2", PowerUpTypes.Freeze, seats[0].PlayerId);
            Assert.That(LobbyFixture.AckOf(events).Reason, Is.EqualTo(AckReasons.NoPowerUp));
            Assert.That(events.OfType<InputRefused>().Single().Kind, Is.EqualTo(InputKinds.PowerUp));
            var other = PowerUpTypes.All.First(t => t != seats[0].HeldPowerUp);
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", other, seats[1].PlayerId)).Reason, Is.EqualTo(AckReasons.NoPowerUp));
        }

        [Test]
        public void ABadTargetOrPhaseIsInvalid()
        {
            var (f, seats) = FirstPickWindow();
            var powerUp = seats[0].HeldPowerUp;
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", powerUp, seats[0].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid), "self");
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", powerUp, "p99")).Reason, Is.EqualTo(AckReasons.Invalid), "nobody");
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", "banana", seats[1].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid));
            f.RunUntilPhase(GamePhase.Answer, 60000);
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", powerUp, seats[1].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid),
                "only in Reveal and Scores");
        }

        [Test]
        public void AResentPickIsADuplicate()
        {
            var (f, seats) = FirstPickWindow();
            var powerUp = seats[0].HeldPowerUp;
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", powerUp, seats[1].PlayerId, "same")).Ok, Is.True);
            var again = f.UsePowerUp("c1", powerUp, seats[1].PlayerId, "same");
            Assert.That(LobbyFixture.AckOf(again).Reason, Is.EqualTo(AckReasons.Duplicate));
            Assert.That(again.OfType<PowerUpUsed>(), Is.Empty);
        }

        // ---------------------------------------------------------------- caps and immunity

        [Test]
        public void UpToMaxHitsPerTargetPerQuestion()
        {
            // D145: up to powerUps.maxHitsPerQuestion (3) hits on one target per question, so players can gang up.
            var (f, seats) = GangPickWindow();
            int max = f.Config.PowerUps.MaxHitsPerQuestion;
            Assert.That(max, Is.EqualTo(3));
            var target = seats[4];
            for (int i = 1; i <= max; i++)
            {
                Assert.That(f.Game.PowerUpTargets(seats[0]).Single(t => t.Target == target).Available, Is.True, "hit " + i + " still open");
                Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c" + (i + 1), seats[i].HeldPowerUp, target.PlayerId)).Ok, Is.True, "hit " + i);
            }

            Assert.That(f.Game.PowerUpTargets(seats[0]).Single(t => t.Target == target).Available, Is.False, "full: greyed");
            Assert.That(f.Game.PowerUpTargets(seats[0]).Single(t => t.Target == seats[1]).Available, Is.True);
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", seats[0].HeldPowerUp, target.PlayerId)).Reason, Is.EqualTo(AckReasons.Immune));
            Assert.That(target.PendingEffects.Count, Is.EqualTo(max));
        }

        [Test]
        public void EveryHitLandsAndTheyAreClearedOneAfterAnother()
        {
            // D145: the effects on one phone play one after another, in the order they were used.
            var (f, seats) = GangPickWindow();
            var target = seats[4];
            var used = new[] { seats[1].HeldPowerUp, seats[2].HeldPowerUp, seats[3].HeldPowerUp };
            for (int i = 1; i <= 3; i++) f.UsePowerUp("c" + (i + 1), seats[i].HeldPowerUp, target.PlayerId);
            f.RunUntilPhase(GamePhase.QuestionShow, 60000);
            var events = f.Advance(f.QuestionShowMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            var applied = events.OfType<PowerUpApplied>().ToList();
            Assert.That(applied.Select(a => a.PowerUp), Is.EqualTo(used));
            Assert.That(applied.Select(a => a.AttackerName), Is.EqualTo(new[] { "P2", "P3", "P4" }));
            Assert.That(target.Turn.Effects.Select(e => e.PowerUp), Is.EqualTo(used));
            Assert.That(target.Turn.Effect, Is.EqualTo(used[0]), "the first one shows first");
            Assert.That(target.Turn.EffectFrom, Is.EqualTo("P2"));

            long answer = f.State.PhaseId;
            for (int i = 0; i < 3; i++)
            {
                Assert.That(target.Turn.EffectCleared, Is.False, "not all cleared before " + i);
                var later = used.Skip(i + 1).FirstOrDefault(u => u != used[i]);
                if (later != null)
                    Assert.That(LobbyFixture.AckOf(f.Send(new ClearEffect("c5", f.NextInputId(), answer, later, 100))).Reason,
                        Is.EqualTo(AckReasons.Invalid), "a later one first");
                var cleared = f.Send(new ClearEffect("c5", f.NextInputId(), answer, used[i], 1000 + i));
                Assert.That(LobbyFixture.AckOf(cleared).Ok, Is.True, "clear " + i);
                Assert.That(cleared.OfType<EffectCleared>().Single().Effect, Is.EqualTo(used[i]));
            }

            Assert.That(target.Turn.EffectCleared, Is.True);
            Assert.That(target.Turn.Effects.All(e => e.Cleared), Is.True);
            Assert.That(LobbyFixture.AckOf(f.Send(new ClearEffect("c5", f.NextInputId(), answer, used[2], 10))).Reason,
                Is.EqualTo(AckReasons.Invalid), "nothing left");
        }

        /// <summary>Q1 of a drawing game with five players: c1 to c4 right (all draw), c5 wrong. Reveal is open.</summary>
        private static (LobbyFixture F, PlayerState[] Seats) GangPickWindow()
        {
            var f = Drawing();
            var seats = f.ToFirstAnswer(5);
            for (int i = 1; i <= 4; i++) f.Choice("c" + i, f.Right, 500 * i);
            f.Choice("c5", f.Wrong, 1000);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            Assert.That(seats.Take(4).All(p => p.HeldPowerUp != null), Is.True);
            return (f, seats);
        }

        [Test]
        public void TheEffectLandsWhenTheNextAnswerStarts()
        {
            var (f, seats) = FirstPickWindow();
            var powerUp = seats[0].HeldPowerUp;
            f.UsePowerUp("c1", powerUp, seats[1].PlayerId);
            var toShow = f.RunUntilPhase(GamePhase.QuestionShow, 60000);
            Assert.That(toShow.OfType<PowerUpApplied>(), Is.Empty, "not in QuestionShow");
            var events = f.Advance(f.QuestionShowMs);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Answer));
            var started = events.OfType<PhaseStarted>().Single();
            var applied = events.OfType<PowerUpApplied>().Single();
            Assert.That(events.ToList().IndexOf(applied), Is.GreaterThan(events.ToList().IndexOf(started)), "after the Answer screen");
            Assert.That(applied.TargetId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(applied.AttackerName, Is.EqualTo("P1"));
            Assert.That(applied.PowerUp, Is.EqualTo(powerUp));
            Assert.That(applied.PhaseId, Is.EqualTo(f.State.PhaseId));
            Assert.That(seats[1].Turn.Effect, Is.EqualTo(powerUp));
            Assert.That(seats[1].Turn.EffectFrom, Is.EqualTo("P1"));
            Assert.That(seats[1].LastHitQuestion, Is.EqualTo(2));
            Assert.That(seats[1].PendingEffects, Is.Empty);
            Assert.That(seats[0].Turn.Effect, Is.Null);
        }

        [Test]
        public void AfterBeingHitAPlayerCanBeHitAgainOnTheNextQuestion()
        {
            // D148: no immunity after a hit (powerUps.immunityQuestions 0).
            var (f, seats) = FirstPickWindow();
            Assert.That(f.Config.PowerUps.ImmunityQuestions, Is.EqualTo(0));
            f.UsePowerUp("c1", seats[0].HeldPowerUp, seats[1].PlayerId); // lands on question 2
            f.RunUntilPhase(GamePhase.Answer, 60000);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Right, 600);
            f.Choice("c3", f.Right, 700);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            Assert.That(f.Game.PowerUpTargets(seats[0]).Single(t => t.Target == seats[1]).Available, Is.True);
        }

        [Test]
        public void WithImmunityConfiguredAHitPlayerIsImmuneForThatManyQuestions()
        {
            Assert.That(Targeting.Immune(2, 3, 1), Is.True);
            Assert.That(Targeting.Immune(2, 4, 1), Is.False);
            Assert.That(Targeting.Immune(2, 3, 0), Is.False);
        }

        [Test]
        public void FreeTargetingAllowsAnyOtherPlayer()
        {
            var (f, seats) = FirstPickWindow(TargetingModes.Free);
            // Ranks now: P1 1, P3 2, P2 3.
            Assert.That(f.Game.PowerUpTargets(seats[0]).Select(t => t.Target), Is.EqualTo(new[] { seats[1], seats[2] }), "the leader may attack");
            Assert.That(f.Game.PowerUpTargets(seats[2]).Select(t => t.Target), Is.EqualTo(new[] { seats[0], seats[1] }), "aiming down is fine");
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c3", seats[2].HeldPowerUp, seats[1].PlayerId)).Ok, Is.True);
        }

        [Test]
        public void AimUpAllowsOnlyBetterRanks()
        {
            var (f, seats) = FirstPickWindow(TargetingModes.AimUp);
            Assert.That(f.State.Settings.Targeting, Is.EqualTo(TargetingModes.AimUp));
            // Ranks now: P1 1 (holds one), P3 2 (holds one), P2 3.
            Assert.That(f.Game.PowerUpTargets(seats[0]), Is.Empty, "the leader cannot attack and keeps it");
            Assert.That(f.Game.CanPickPowerUp(seats[0]), Is.False);
            Assert.That(f.Game.PowerUpTargets(seats[2]).Select(t => t.Target), Is.EqualTo(new[] { seats[0] }));
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", seats[0].HeldPowerUp, seats[2].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid));
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c3", seats[2].HeldPowerUp, seats[1].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid),
                "never down");
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c3", seats[2].HeldPowerUp, seats[0].PlayerId)).Ok, Is.True);
            Assert.That(seats[0].HeldPowerUp, Is.Not.Null, "the leader still holds theirs");
        }

        [TestCase(TargetingModes.AimUp, 2, 2, false)]
        [TestCase(TargetingModes.AimUp, 3, 1, true)]
        [TestCase(TargetingModes.AimUp, 1, 3, false)]
        [TestCase(TargetingModes.Free, 1, 3, true)]
        [TestCase(TargetingModes.Free, 2, 2, true)]
        public void AimUpUsesSharedRanks(string mode, int attacker, int target, bool allowed)
        {
            Assert.That(Targeting.AllowedByRank(mode, attacker, target), Is.EqualTo(allowed));
        }

        // ---------------------------------------------------------------- effectCleared

        [Test]
        public void TheTargetReportsTheEffectCleared()
        {
            var (f, seats) = FirstPickWindow();
            var powerUp = seats[0].HeldPowerUp;
            f.UsePowerUp("c1", powerUp, seats[1].PlayerId);
            f.RunUntilPhase(GamePhase.Answer, 60000);
            long answer = f.State.PhaseId;
            Assert.That(LobbyFixture.AckOf(f.Send(new ClearEffect("c1", f.NextInputId(), answer, powerUp, 100))).Reason,
                Is.EqualTo(AckReasons.Invalid), "not hit");
            var other = PowerUpTypes.All.First(t => t != powerUp);
            Assert.That(LobbyFixture.AckOf(f.Send(new ClearEffect("c2", f.NextInputId(), answer, other, 100))).Reason,
                Is.EqualTo(AckReasons.Invalid), "another effect");

            var events = f.Send(new ClearEffect("c2", f.NextInputId(), answer, powerUp, 2650));
            Assert.That(LobbyFixture.AckOf(events).Ok, Is.True);
            var cleared = events.OfType<EffectCleared>().Single();
            Assert.That(cleared.PlayerId, Is.EqualTo(seats[1].PlayerId));
            Assert.That(cleared.Effect, Is.EqualTo(powerUp));
            Assert.That(cleared.Ms, Is.EqualTo(2650));
            Assert.That(seats[1].Turn.Locked, Is.False, "clearing never locks an answer");
            Assert.That(LobbyFixture.AckOf(f.Send(new ClearEffect("c2", f.NextInputId(), answer, powerUp, 10))).Reason,
                Is.EqualTo(AckReasons.Invalid), "once");
        }

        // ---------------------------------------------------------------- the last question and the end of the rounds

        /// <summary>Steps the clock (nobody answering) until the Answer of game question number n is open.</summary>
        private static void RunToAnswer(LobbyFixture f, int number)
        {
            long waited = 0;
            while (!(f.State.Phase == GamePhase.Answer && f.State.Question.Number == number))
            {
                Assert.That(waited, Is.LessThan(3600000), "question " + number + " never came");
                f.Advance(50);
                waited += 50;
            }
        }

        [Test]
        public void NothingOnTheLastQuestionOfTheLastRoundAndUnusedOnesAreLost()
        {
            var f = Drawing();
            var seats = f.ToFirstAnswer(2);
            int last = f.Config.Rounds.Count * f.Config.Rounds.QuestionsPerRound;
            RunToAnswer(f, last - 1);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Wrong, 500);
            Assert.That(seats[0].HeldPowerUp, Is.Not.Null, "drawn on the question before the last");
            var held = seats[0].HeldPowerUp;

            RunToAnswer(f, last);
            Assert.That(f.State.Question.IsClosestValue, Is.True);
            var answer = f.Value("c2", f.State.Question.Def.Answer.Value, 500);
            var events = f.Value("c1", f.State.Question.Def.Answer.Value, 500);
            Assert.That(f.State.Phase, Is.EqualTo(GamePhase.Reveal));
            Assert.That(events.OfType<PowerUpDrawn>(), Is.Empty, "no draw on the last question");
            Assert.That(seats[1].HeldPowerUp, Is.Null);
            Assert.That(f.Game.PickWindowOpen, Is.False, "the pick window is hidden");
            Assert.That(f.Game.PowerUpTargets(seats[0]), Is.Empty);
            Assert.That(LobbyFixture.AckOf(f.UsePowerUp("c1", held, seats[1].PlayerId)).Reason, Is.EqualTo(AckReasons.Invalid));

            var toRoundEnd = f.RunUntilPhase(GamePhase.RoundEnd, 60000);
            Assert.That(seats[0].HeldPowerUp, Is.Null, "lost at the end of the last round");
            Assert.That(toRoundEnd.OfType<PlayerUpdated>().Select(u => u.PlayerId), Does.Contain(seats[0].PlayerId));
        }

        [Test]
        public void NoDrawsInTheRace()
        {
            var f = Drawing();
            f.ToFirstVote(2);
            f.RunUntilPhase(GamePhase.RaceQuestion, 3600000, 50);
            f.TrueFalse("c1", f.State.Race.Statement.Answer, 500);
            var events = f.TrueFalse("c2", f.State.Race.Statement.Answer, 500);
            Assert.That(events.OfType<PowerUpDrawn>(), Is.Empty);
        }

        // ---------------------------------------------------------------- the TV settings

        [Test]
        public void PowerUpsOffMeansNoDrawsFromTheNextGame()
        {
            var f = Drawing();
            f.SeatPlayers(2);
            var changed = f.SetPowerUps(false);
            var setting = changed.OfType<SettingsChanged>().Single();
            Assert.That(setting.Source, Is.EqualTo(SettingsChanged.SourceTv));
            Assert.That(setting.Setting, Is.EqualTo(SettingsChanged.SettingPowerUps));
            Assert.That(setting.Value, Is.EqualTo("false"));
            var startEvents = f.ReadyAll();
            startEvents.AddRange(f.Advance(f.CountdownMs));
            var started = startEvents.OfType<GameStarted>().Single();
            Assert.That(started.PowerUps, Is.Empty);
            f.Advance(f.IntroMs);
            f.EndVote();
            f.Advance(f.QuestionShowMs);
            f.Choice("c1", f.Right, 500);
            var events = f.Choice("c2", f.Right, 500);
            Assert.That(events.OfType<PowerUpDrawn>(), Is.Empty);
            Assert.That(f.Game.PickWindowOpen, Is.False);
        }

        [Test]
        public void SettingsChangedDuringAGameApplyFromTheNextGame()
        {
            var f = Drawing();
            var seats = f.ToFirstAnswer(2);
            f.SetPowerUps(false, targeting: TargetingModes.AimUp);
            Assert.That(f.State.PendingSettings.PowerUpsEnabled, Is.False);
            Assert.That(f.State.Settings.PowerUpsEnabled, Is.True, "the running game keeps its settings");
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Wrong, 500);
            Assert.That(seats[0].HeldPowerUp, Is.Not.Null);
            f.RunToLobby();
            f.StartNextGame();
            Assert.That(f.State.Settings.PowerUpsEnabled, Is.False);
            Assert.That(f.State.Settings.Targeting, Is.EqualTo(TargetingModes.AimUp));
        }

        [Test]
        public void OnlyTheEnabledTypesAreDrawn()
        {
            var f = Drawing();
            var changed = f.SetPowerUps(true, types: new[] { PowerUpTypes.Fog });
            Assert.That(changed.OfType<SettingsChanged>().Single().Value, Is.EqualTo("fog"));
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Right, 600);
            Assert.That(seats.Select(s => s.HeldPowerUp), Is.EqualTo(new[] { PowerUpTypes.Fog, PowerUpTypes.Fog }));
        }

        [Test]
        public void EveryTypeOffIsLikePowerUpsOff()
        {
            var f = Drawing();
            f.SetPowerUps(true, types: new string[0]);
            var seats = f.ToFirstAnswer(2);
            f.Choice("c1", f.Right, 500);
            f.Choice("c2", f.Right, 600);
            Assert.That(seats.All(s => s.HeldPowerUp == null), Is.True);
        }

        [Test]
        public void TheGameLanguageChangesAtOnce()
        {
            var f = new LobbyFixture();
            var events = f.Send(new ChangeTvSettings(gameLang: Languages.English));
            Assert.That(f.State.GameLanguage, Is.EqualTo(Languages.English));
            var e = events.OfType<SettingsChanged>().Single();
            Assert.That(e.Setting, Is.EqualTo(SettingsChanged.SettingGameLang));
            Assert.That(e.Value, Is.EqualTo(Languages.English));
            Assert.That(f.Send(new ChangeTvSettings(gameLang: "de")).OfType<SettingsChanged>(), Is.Empty, "unknown language");
            Assert.That(f.Send(new ChangeTvSettings(targeting: "sideways")).OfType<SettingsChanged>(), Is.Empty);
        }

        [Test]
        public void TheSettingsDefaultToTheConfig()
        {
            var f = new LobbyFixture();
            var cfg = f.Config.PowerUps;
            Assert.That(f.State.PendingSettings.PowerUpsEnabled, Is.EqualTo(cfg.Enabled));
            Assert.That(f.State.PendingSettings.PowerUpTypes, Is.EquivalentTo(cfg.Types));
            Assert.That(f.State.PendingSettings.Targeting, Is.EqualTo(cfg.Targeting));
        }
    }
}
