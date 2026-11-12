using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using FamilyQuiz.Presentation.Staging;
using NUnit.Framework;
using UnityEngine;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>Plain C# parts of the TV changes after the second playtest (decisions/0027: D149, D150, D151, D153, D157, D159).</summary>
    public sealed class D0027TvPartsTests
    {
        private static PlayerState Player(GameState state, string id, int score = 0)
        {
            var p = new PlayerState(id, id.ToUpperInvariant(), "en", 0, "t" + id, 1);
            p.Connected = true;
            p.Score = score;
            state.MutablePlayers.Add(p);
            return p;
        }

        private static SeatMap Seated(GameState state)
        {
            var seats = new SeatMap(8);
            seats.Sync(state.Players.Select(p => p.PlayerId));
            return seats;
        }

        // ------------------------------------------------------------ D150: no answer tiles in Answer

        [Test]
        public void TheAnswerCardTakesTheTilesRoomAndStaysUnderTheGuests()
        {
            var card = TvLayout.AnswerCardLarge;
            Assert.That(card.yMin, Is.LessThanOrEqualTo(TvLayout.Tiles.yMin), "down to where the tiles were");
            Assert.That(card.yMax, Is.EqualTo(TvLayout.AnswerCard.yMax));
            Assert.That(card.yMax / TvLayout.Height, Is.LessThan(0.38f));
            Assert.That(card.width, Is.GreaterThanOrEqualTo(TvLayout.Tiles.width));
            // The Answer set has the card and no tiles.
            Assert.That(TvLayout.PhaseSets().Any(set => set.Contains(card) && !set.Contains(TvLayout.Tiles)), Is.True);
        }

        [Test]
        public void TheAnswerCardHasRoomForThreeLinesOfVeryLargeType()
        {
            // Three lines of 84 units with line spacing -6 need about 240 under the ribbon.
            float room = TvLayout.AnswerCardLarge.height - 18f - QuestionCardLayout.QuestionTopInset();
            Assert.That(room, Is.GreaterThanOrEqualTo(240f));
            Assert.That(QuestionCard.AnswerMaxFontSize, Is.GreaterThanOrEqualTo(84f));
            Assert.That(QuestionCard.AnswerMinFontSize, Is.LessThan(QuestionCard.AnswerMaxFontSize));
        }

        // ------------------------------------------------------------ D151: no multiplier shows at 1

        [Test]
        public void NoMultiplierStickerAtOne()
        {
            Assert.That(MultiplierSticker.Shows(1.0), Is.False);
            Assert.That(MultiplierSticker.Shows(0.5), Is.False);
            Assert.That(MultiplierSticker.Shows(1.5), Is.True);
        }

        // ------------------------------------------------------------ D159: Scores in sofa order with a crown

        [Test]
        public void TheSofaFromLeftToRightIsOuterArmchairsToOuterArmchairs()
        {
            // Fill order: middle sofa pair, outer sofa pair, inner armchairs, outer armchairs (docs/21_staging.md, "Seats").
            Assert.That(SofaOrder.SeatsLeftToRight, Is.EqualTo(new[] { 6, 4, 2, 0, 1, 3, 5, 7 }));
            Assert.That(SofaOrder.SeatsLeftToRight.OrderBy(s => s), Is.EqualTo(Enumerable.Range(0, 8)));
        }

        [Test]
        public void ScoreCardsFollowTheSeatsNotTheRanks()
        {
            var s = new GameState();
            Player(s, "a", 100); // seat 0: middle left of the sofa
            Player(s, "b", 900); // seat 1: middle right
            Player(s, "c", 500); // seat 2: outer left
            Player(s, "d", 0);   // seat 3: outer right
            var seats = Seated(s);
            var order = SofaOrder.Players(s, seats.SeatOf).Select(p => p.PlayerId);
            Assert.That(order, Is.EqualTo(new[] { "c", "a", "b", "d" }));
        }

        [Test]
        public void APlayerKeepsTheirSeatWhenSomeoneLeavesAndANewPlayerTakesTheFreeOne()
        {
            var s = new GameState();
            Player(s, "a");
            var b = Player(s, "b");
            Player(s, "c");
            var seats = Seated(s);
            s.MutablePlayers.Remove(b);
            Player(s, "d");
            seats.Sync(s.Players.Select(p => p.PlayerId));
            // c stays on seat 2 (outer left); d takes b's free seat 1 (middle right).
            Assert.That(SofaOrder.Players(s, seats.SeatOf).Select(p => p.PlayerId), Is.EqualTo(new[] { "c", "a", "d" }));
        }

        [Test]
        public void PlayersWithoutASeatComeLastInJoinOrder()
        {
            var s = new GameState();
            Player(s, "a");
            Player(s, "b");
            Player(s, "c");
            var order = SofaOrder.Players(s, id => id == "b" ? 0 : -1).Select(p => p.PlayerId);
            Assert.That(order, Is.EqualTo(new[] { "b", "a", "c" }));
        }

        [Test]
        public void EveryPlayerTiedOnRankOneWearsTheCrown()
        {
            var s = new GameState();
            Player(s, "a", 900);
            Player(s, "b", 400);
            Player(s, "c", 900);
            Assert.That(Standings.Leaders(s).Select(p => p.PlayerId), Is.EqualTo(new[] { "a", "c" }));

            var one = new GameState();
            Player(one, "a", 100);
            Player(one, "b", 300);
            Assert.That(Standings.Leaders(one).Select(p => p.PlayerId), Is.EqualTo(new[] { "b" }));
        }

        // ------------------------------------------------------------ D153: the TopicTie spin

        [Test]
        public void TheSpinRunsBetweenTheTiedDoorsAndStopsOnTheChosenOne()
        {
            var tied = new[] { 0, 2, 3 };
            var hops = TopicTieSpin.Doors(tied, 2, TopicTieSpin.Hops);
            Assert.That(hops.Count, Is.EqualTo(TopicTieSpin.Hops + 1));
            Assert.That(hops.Last(), Is.EqualTo(2));
            Assert.That(hops.All(d => tied.Contains(d)), Is.True);
            for (int i = 1; i < hops.Count; i++) Assert.That(hops[i], Is.Not.EqualTo(hops[i - 1]), "it moves every hop");
            Assert.That(hops.Distinct().Count(), Is.EqualTo(3), "every tied door lights up");
        }

        [Test]
        public void TheSpinSlowsDownAndHoldsTheWinnerAtTheEnd()
        {
            var tied = new[] { 1, 3 };
            int hops = TopicTieSpin.Hops;
            var times = new List<float>();
            int last = -1;
            for (int ms = 0; ms <= 2500; ms++)
            {
                int hop = TopicTieSpin.HopAt(ms / 2500f);
                Assert.That(hop, Is.GreaterThanOrEqualTo(last), "never backwards");
                if (hop != last) times.Add(ms);
                last = hop;
            }

            Assert.That(last, Is.EqualTo(hops));
            Assert.That(times.Count, Is.EqualTo(hops + 1));
            var gaps = times.Skip(1).Zip(times, (b, a) => b - a).ToList();
            Assert.That(gaps.Last(), Is.GreaterThan(gaps.First() * 2), "slowing down");
            Assert.That(times.Last() / 2500f, Is.LessThanOrEqualTo(TopicTieSpin.SpinShare + 0.001f), "the winner then stands out");
            Assert.That(TopicTieSpin.DoorAt(tied, 3, 1f), Is.EqualTo(3));
            Assert.That(TopicTieSpin.DoorAt(tied, 3, 0.99f), Is.EqualTo(3));
        }

        [Test]
        public void ASpinWithoutATieShowsTheChosenDoor()
        {
            Assert.That(TopicTieSpin.DoorAt(new int[0], 2, 0.1f), Is.EqualTo(2));
            Assert.That(TopicTieSpin.DoorAt(new[] { 1, 3 }, 2, 0.1f), Is.EqualTo(2), "a chosen door outside the tie");
            Assert.That(TopicTieSpin.Doors(new[] { 3 }, 3, 5), Is.EqualTo(new[] { 3, 3, 3, 3, 3, 3 }));
        }

        [TestCase(GamePhase.TopicTie, Shot.Curtain)]
        [TestCase(GamePhase.Attacks, Shot.Wide)]
        public void TheNewPhasesKeepTheShotOfTheViewTheyKeep(GamePhase phase, Shot shot)
        {
            var plan = new ShotPlan(6000, 1500);
            plan.OnPhase(phase, 1000);
            Assert.That(plan.Current(1500).Shot, Is.EqualTo(shot));
        }

        // ------------------------------------------------------------ D149: attack arrows

        [Test]
        public void EveryPowerUpUsedIsOneArrowInTheOrderTheyWereUsed()
        {
            var s = new GameState();
            Player(s, "a");
            Player(s, "b");
            Player(s, "c");
            var used = new[]
            {
                new PowerUpUsed("a", PowerUpTypes.Slime, "c", 7),
                new PowerUpUsed("b", PowerUpTypes.Freeze, "c", 7),
                new PowerUpUsed("c", PowerUpTypes.Fog, "a", 7),
            };
            var arrows = AttackArrows.Build(used, s);
            Assert.That(arrows.Select(a => a.AttackerId + ">" + a.TargetId), Is.EqualTo(new[] { "a>c", "b>c", "c>a" }));
            Assert.That(arrows.Select(a => a.PowerUp), Is.EqualTo(new[] { PowerUpTypes.Slime, PowerUpTypes.Freeze, PowerUpTypes.Fog }));
            // a>c and c>a join the same two players: the second one takes the next lane so they never lie on each other.
            Assert.That(arrows.Select(a => a.Lane), Is.EqualTo(new[] { 0, 0, 1 }));
            // c has three arrow ends (two in, one out), a two, b one: they sit side by side in the order used.
            Assert.That(arrows.Select(a => a.TargetEnd + "/" + a.TargetEnds), Is.EqualTo(new[] { "0/3", "1/3", "1/2" }));
            Assert.That(arrows.Select(a => a.AttackerEnd + "/" + a.AttackerEnds), Is.EqualTo(new[] { "0/2", "0/1", "2/3" }));
        }

        [Test]
        public void AnArrowNeedsBothPlayersSeatedAndTwoDifferentPlayers()
        {
            var s = new GameState();
            Player(s, "a");
            Player(s, "b");
            var used = new[]
            {
                new PowerUpUsed("a", PowerUpTypes.Slime, "gone", 3),
                new PowerUpUsed("gone", PowerUpTypes.Slime, "b", 3),
                new PowerUpUsed("a", PowerUpTypes.Fog, "a", 3),
                null,
                new PowerUpUsed("b", PowerUpTypes.Fog, "a", 3),
            };
            var arrows = AttackArrows.Build(used, s);
            Assert.That(arrows.Select(a => a.AttackerId + ">" + a.TargetId), Is.EqualTo(new[] { "b>a" }));
        }

        [Test]
        public void AnArrowArcsFromTheAttackerToTheTarget()
        {
            var from = new Vector2(100f, 200f);
            var to = new Vector2(700f, 200f);
            var points = ArrowPath.Points(from, to, Vector2.up, 120f, 12);
            Assert.That(points.Count, Is.EqualTo(13));
            Assert.That(points[0], Is.EqualTo(from));
            Assert.That(points[12], Is.EqualTo(to));
            // A quadratic arc: half the bulge at the middle, on the bulge side.
            Assert.That(points[6].x, Is.EqualTo(400f).Within(0.01f));
            Assert.That(points[6].y, Is.EqualTo(260f).Within(0.01f));
            Assert.That(points.All(p => p.y >= 200f - 0.01f), Is.True);
        }

        [Test]
        public void ArrowEndsAtOnePieceSpreadAroundItsMiddle()
        {
            Assert.That(ArrowPath.Spread(0, 1), Is.EqualTo(0f));
            Assert.That(ArrowPath.Spread(0, 2), Is.EqualTo(-ArrowPath.Spread(1, 2)));
            Assert.That(ArrowPath.Spread(1, 3), Is.EqualTo(0f));
            Assert.That(ArrowPath.Spread(2, 3) - ArrowPath.Spread(1, 3), Is.EqualTo(ArrowPath.EndGap));
        }

        [Test]
        public void HigherLanesBulgeFurther()
        {
            Assert.That(ArrowPath.Bulge(600f, 1), Is.GreaterThan(ArrowPath.Bulge(600f, 0)));
            Assert.That(ArrowPath.Bulge(900f, 0), Is.GreaterThan(ArrowPath.Bulge(200f, 0)));
        }

        [Test]
        public void InAttacksEveryHitWaitingForTheNextAnswerShows()
        {
            var s = new GameState();
            var target = Player(s, "s");
            target.MutablePendingEffects.Add(new PendingEffect(PowerUpTypes.Slime, "a", "Аня"));
            target.MutablePendingEffects.Add(new PendingEffect(PowerUpTypes.Fog, "m", "Мама"));
            Assert.That(PowerUpHit.All(target, GamePhase.Attacks).Select(h => h.PowerUp),
                Is.EqualTo(new[] { PowerUpTypes.Slime, PowerUpTypes.Fog }));
        }

        // ------------------------------------------------------------ D157: the race moves back and Пас stays

        [Test]
        public void EachRaceTurnGetsItsMark()
        {
            Assert.That(RaceMark.For(null), Is.EqualTo(RaceMark.Kind.None));
            Assert.That(RaceMark.For(new PlayerTurn()), Is.EqualTo(RaceMark.Kind.None), "no answer: no move");
            Assert.That(RaceMark.For(new PlayerTurn { Locked = true, Passed = true }), Is.EqualTo(RaceMark.Kind.Pass));
            Assert.That(RaceMark.For(new PlayerTurn { Locked = true, TrueFalse = true, Correct = true, Revealed = true }),
                Is.EqualTo(RaceMark.Kind.Forward));
            Assert.That(RaceMark.For(new PlayerTurn { Locked = true, TrueFalse = false, Correct = false, Revealed = true }),
                Is.EqualTo(RaceMark.Kind.Back));
            Assert.That(RaceMark.For(new PlayerTurn { Locked = true, TrueFalse = false }), Is.EqualTo(RaceMark.Kind.None), "not revealed yet");
        }

        [Test]
        public void AStepBackSlidesLeftAndNeverBelowTheStart()
        {
            const int finish = 15;
            Assert.That(RaceTrackLayout.CellCenter(4, finish), Is.LessThan(RaceTrackLayout.CellCenter(5, finish)));
            Assert.That(RaceTrackLayout.CellCenter(-1, finish), Is.EqualTo(RaceTrackLayout.CellCenter(0, finish)));
            Assert.That(RaceTrackLayout.Cells(finish), Is.EqualTo(16));
        }
    }
}
