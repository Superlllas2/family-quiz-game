using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.State;
using FamilyQuiz.Net;

namespace FamilyQuiz.Tests.NetScreens
{
    /// <summary>
    /// A game whose state the test sets by hand (Core internals are visible to the tests), with the real config,
    /// characters and ui strings, one known question, a fake clock and a capturing relay sender.
    /// </summary>
    public sealed class NetTestGame
    {
        /// <summary>Display order used by StartQuestion: A Rhine, B Volga (the right one), C Dnieper, D Danube.</summary>
        public static readonly int[] DisplayOrder = { 2, 0, 3, 1 };

        public NetTestGame()
        {
            Config = TestContent.Config();
            Clock = new ManualClock();
            Clock.Advance(100000);
            Question = new QuestionDef
            {
                Id = "geo_001",
                Topic = "geography",
                Format = QuestionDef.MultipleChoice,
                Difficulty = "easy",
                Text = Text("Какая река самая длинная в Европе?", "Which is the longest river in Europe?"),
                Options = new List<LocalizedText>
                {
                    Text("Волга", "Volga"),
                    Text("Дунай", "Danube"),
                    Text("Рейн", "Rhine"),
                    Text("Днепр", "Dnieper")
                },
                Correct = 0,
                Source = "https://www.wikidata.org/wiki/Q626"
            };
            var strings = UiStrings.Parse(TestContent.Read(UiStrings.FileName));
            var content = new GameContent(Config, TestContent.Characters(Config), new List<QuestionDef> { Question }, strings);
            Game = new Game(content, Clock, new SeededRandom(1), () => 42);
            Game.Start();
            Sender = new CapturingRelaySender();
            Outbox = new PhoneOutbox(Game, Sender, () => "KXQT", null);
            Screens = new ScreenBuilder(Game);
        }

        public GameConfig Config { get; }
        public ManualClock Clock { get; }
        public QuestionDef Question { get; }
        public Game Game { get; }
        public GameState State => Game.State;
        public CapturingRelaySender Sender { get; }
        public PhoneOutbox Outbox { get; }
        public ScreenBuilder Screens { get; }

        public long BaseWindowMs => (long)(Config.Timers.Answer.MultipleChoice * 1000);

        public static LocalizedText Text(string ru, string en)
        {
            return new LocalizedText { Ru = ru, En = en };
        }

        /// <summary>Seats a player with a connected socket conn (or a disconnected one).</summary>
        public PlayerState AddPlayer(string playerId, string name, string lang, int characterId, string conn,
            bool connected = true, bool characterPicked = true, double timerMultiplier = 1)
        {
            var player = new PlayerState(playerId, name, lang, characterId, "token_" + playerId, timerMultiplier)
            {
                Conn = conn,
                Connected = connected,
                CharacterPicked = characterPicked
            };
            State.MutablePlayers.Add(player);
            if (connected && conn != null) State.OpenConns.Add(conn);
            return player;
        }

        /// <summary>The three players most tests use: Mama (ru), Papa (en), Kid (ru).</summary>
        public void AddFamily()
        {
            AddPlayer("p1", "Mama", "ru", 0, "c1");
            AddPlayer("p2", "Papa", "en", 1, "c2");
            AddPlayer("p3", "Kid", "ru", 2, "c3");
        }

        public void SetPhase(GamePhase phase, long phaseId, long? endsInMs)
        {
            State.Phase = phase;
            State.PhaseId = phaseId;
            State.PhaseStartedAtMs = Clock.NowMs;
            State.PhaseEndsAtMs = endsInMs.HasValue ? Clock.NowMs + endsInMs.Value : (long?)null;
        }

        /// <summary>Puts the known question in play (number 1, slot 0, round 0) with DisplayOrder.</summary>
        public QuestionInPlay StartQuestion(long answerPhaseId)
        {
            var question = new QuestionInPlay(Question, 1, 0, 0, Config.Rounds.Multipliers[0], DisplayOrder)
            {
                AnswerPhaseId = answerPhaseId,
                AnswerStartedAtMs = Clock.NowMs,
                BaseWindowMs = BaseWindowMs
            };
            State.Question = question;
            return question;
        }

        /// <summary>
        /// Enters Answer for every player: T = base window times the player's multiplier, deadline = now + T, and the
        /// phase end at the latest deadline plus net.inputGraceMs (D73).
        /// </summary>
        public void EnterAnswer(long phaseId)
        {
            StartQuestion(phaseId);
            long latest = Clock.NowMs;
            foreach (var p in State.Players)
            {
                var window = (long)(BaseWindowMs * p.TimerMultiplier);
                p.Turn = new PlayerTurn { WindowMs = window, DeadlineMs = Clock.NowMs + window };
                if (p.Turn.DeadlineMs > latest) latest = p.Turn.DeadlineMs;
            }

            State.Phase = GamePhase.Answer;
            State.PhaseId = phaseId;
            State.PhaseStartedAtMs = Clock.NowMs;
            State.PhaseEndsAtMs = latest + Config.Net.InputGraceMs;
        }

        public void Lock(PlayerState player, int displayIndex, long ms)
        {
            player.Turn.InputId = "in_" + player.PlayerId;
            player.Turn.Choice = displayIndex;
            player.Turn.RawMs = ms;
            player.Turn.Ms = ms;
            player.Turn.LockedAtMs = Clock.NowMs;
        }
    }
}
