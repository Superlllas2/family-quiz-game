using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using FamilyQuiz.Presentation;
using NUnit.Framework;

namespace FamilyQuiz.Tests.TvParts
{
    /// <summary>Plain C# parts of the TV changes after the first playtest (decisions/0026: D141, D142, D145).</summary>
    public sealed class D0026TvPartsTests
    {
        private static PlayerState Player(GameState state, string id, string name)
        {
            var p = new PlayerState(id, name, "en", 0, "t" + id, 1);
            p.Connected = true;
            state.MutablePlayers.Add(p);
            return p;
        }

        private static void Answered(PlayerState p, int? choice, bool correct, int points)
        {
            p.Turn = new PlayerTurn { Locked = choice.HasValue, Choice = choice, Correct = correct, Points = points };
        }

        // D141's empty tiles and larger card are replaced by D150 (D0027TvPartsTests).

        // ------------------------------------------------------------ D142: who picked which tile

        [Test]
        public void EachLockedChoiceSitsOnItsTileWithNameAndPoints()
        {
            var s = new GameState();
            Answered(Player(s, "a", "Аня"), 2, true, 840);
            Answered(Player(s, "b", "Соня"), 0, false, 0);
            Answered(Player(s, "c", "Папа"), null, false, 0);
            Answered(Player(s, "d", "Мама"), 2, true, 420);
            var tiles = RevealPicks.ByTile(s, 4);
            Assert.That(tiles.Length, Is.EqualTo(4));
            Assert.That(tiles[2].Select(p => p.Name), Is.EqualTo(new[] { "Аня", "Мама" }), "seat order");
            Assert.That(tiles[2].Select(p => p.Points), Is.EqualTo(new[] { 840, 420 }));
            Assert.That(tiles[2].All(p => p.Correct), Is.True);
            Assert.That(tiles[0].Single().Name, Is.EqualTo("Соня"));
            Assert.That(tiles[0].Single().Correct, Is.False);
            Assert.That(tiles[1], Is.Empty);
            Assert.That(tiles.SelectMany(t => t).Any(p => p.PlayerId == "c"), Is.False, "no answer: on no tile");
        }

        [Test]
        public void APickOutsideTheTilesOrWithoutATurnIsLeftOut()
        {
            var s = new GameState();
            Answered(Player(s, "a", "A"), 3, false, 0);
            Player(s, "b", "B");
            var tiles = RevealPicks.ByTile(s, 3);
            Assert.That(tiles.SelectMany(t => t), Is.Empty);
        }

        // ------------------------------------------------------------ D145: hits and draws

        [Test]
        public void InAnswerEveryHitThatLandedShowsAttackerPowerUpAndTarget()
        {
            var s = new GameState();
            var target = Player(s, "s", "Соня");
            target.Turn = new PlayerTurn();
            target.Turn.AddEffect(PowerUpTypes.Slime, "Аня");
            target.Turn.AddEffect(PowerUpTypes.Freeze, "Папа");
            var hits = PowerUpHit.All(target, GamePhase.Answer);
            Assert.That(hits.Select(h => h.AttackerName), Is.EqualTo(new[] { "Аня", "Папа" }));
            Assert.That(hits.Select(h => h.PowerUp), Is.EqualTo(new[] { PowerUpTypes.Slime, PowerUpTypes.Freeze }));
            Assert.That(hits.All(h => h.TargetName == "Соня"), Is.True);
            Assert.That(PowerUpHit.All(target, GamePhase.QuestionShow), Is.Empty);
        }

        [Test]
        public void InRevealAndScoresEveryHitWaitingForTheNextAnswerShows()
        {
            var s = new GameState();
            var target = Player(s, "s", "Соня");
            target.Turn = new PlayerTurn();
            target.Turn.AddEffect(PowerUpTypes.Fog, "Old"); // last question's: not shown any more
            Assert.That(PowerUpHit.All(target, GamePhase.Reveal), Is.Empty);
            target.MutablePendingEffects.Add(new PendingEffect(PowerUpTypes.Slime, "a", "Аня"));
            target.MutablePendingEffects.Add(new PendingEffect(PowerUpTypes.Fog, "m", "Мама"));
            Assert.That(PowerUpHit.All(target, GamePhase.Reveal).Select(h => h.AttackerName), Is.EqualTo(new[] { "Аня", "Мама" }));
            Assert.That(PowerUpHit.All(target, GamePhase.Scores).Count, Is.EqualTo(2));
            Assert.That(PowerUpHit.All(target, GamePhase.RoundEnd), Is.Empty);
            Assert.That(PowerUpHit.All(null, GamePhase.Scores), Is.Empty);
        }

        [Test]
        public void ADrawSaysWhyTheRightAnswerAndTheBetterChanceBehindTheLeader()
        {
            Assert.That(PowerUpDrawReason.Key(1), Is.EqualTo("tv.powerUps.drawLeader"));
            Assert.That(PowerUpDrawReason.Key(2), Is.EqualTo("tv.powerUps.drawBehind"));
            Assert.That(PowerUpDrawReason.Key(6), Is.EqualTo("tv.powerUps.drawBehind"));
        }
    }
}
