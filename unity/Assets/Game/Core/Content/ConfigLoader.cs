using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FamilyQuiz.Core.Content
{
    /// <summary>
    /// Parses and checks content/config.json (docs/03_rules.md "Config keys", tools/validate_content.mjs). Errors name
    /// the key, for example "config.json timers.reveal: must be more than 0".
    /// </summary>
    public static class ConfigLoader
    {
        public const string FileName = "config.json";

        /// <summary>Every key of the table in docs/03_rules.md (the same list as tools/validate_content.mjs).</summary>
        private static readonly string[] RequiredKeys =
        {
            "configVersion", "timers.intro", "timers.topicVote", "timers.questionShow", "timers.answer.multipleChoice",
            "timers.answer.closestValue", "timers.lastCall", "timers.reveal", "timers.scores", "timers.roundEnd",
            "timers.finalIntro", "timers.results", "rounds.count", "rounds.questionsPerRound", "rounds.multipliers",
            "rounds.difficultyOrder", "rounds.formatOrder", "topics.doors", "topics.minUnseenToOffer", "topics.mixedTopicId",
            "scoring.maxPoints", "scoring.speedPenalty", "scoring.roundTo", "scoring.fastAnswerMs", "closestValue.closestBonus",
            "closestValue.minPoints", "closestValue.exactShare",
            "powerUps.enabled", "powerUps.types", "powerUps.targeting", "powerUps.drawChanceLeader", "powerUps.maxHeld",
            "powerUps.maxHitsPerQuestion", "powerUps.immunityQuestions", "powerUps.onPhone", "powerUps.freeze.taps",
            "powerUps.slime.blobs", "powerUps.fog.seconds", "race.maxHeadStart", "race.finishSteps", "race.maxQuestions",
            "race.stepsPerCorrect", "race.answer", "race.reveal", "players.min", "players.max", "players.timerMultiplierOptions",
            "players.seatHoldSeconds", "lobby.startCountdownSeconds", "net.inputGraceMs", "net.tvReconnectSeconds",
            "hosts.minGapSeconds", "hosts.bubbleSeconds", "hosts.streakTrigger", "poll.enabled", "poll.maxSeconds",
            "staging.extraSeconds.questionShow", "staging.extraSeconds.reveal", "staging.extraSeconds.scores",
            "staging.extraSeconds.roundEnd", "staging.lobby.seatPopSeconds", "staging.intro.establishSeconds",
            "staging.questionShow.curtainOpenSeconds", "staging.questionShow.questionInSeconds",
            "staging.answer.suspenseRiseSeconds", "staging.reveal.cardsUpSeconds", "staging.reveal.reactionSeconds",
            "staging.scores.overlayInSeconds", "staging.results.winnerWalkSeconds"
        };

        public static GameConfig Parse(string json)
        {
            var root = JsonItems.ParseObject(FileName, json);
            var errors = new ContentErrors(FileName);
            foreach (var key in RequiredKeys)
            {
                if (root.SelectToken(key) == null) errors.Add(key, "missing key");
            }

            GameConfig config;
            try
            {
                config = root.ToObject<GameConfig>();
            }
            catch (JsonException e)
            {
                errors.Add("-", e.Message);
                errors.ThrowIfAny();
                throw;
            }

            errors.ThrowIfAny();
            if (config == null) throw new ContentException(FileName, "file is empty");
            Check(config, root, errors);
            errors.ThrowIfAny();
            return config;
        }

        private static void Check(GameConfig c, JObject root, ContentErrors e)
        {
            var t = c.Timers;
            Positive(e, "timers.intro", t.Intro);
            Positive(e, "timers.topicVote", t.TopicVote);
            Positive(e, "timers.questionShow", t.QuestionShow);
            Positive(e, "timers.answer.multipleChoice", t.Answer.MultipleChoice);
            Positive(e, "timers.answer.closestValue", t.Answer.ClosestValue);
            Positive(e, "timers.lastCall", t.LastCall);
            Positive(e, "timers.reveal", t.Reveal);
            Positive(e, "timers.scores", t.Scores);
            Positive(e, "timers.roundEnd", t.RoundEnd);
            Positive(e, "timers.finalIntro", t.FinalIntro);
            Positive(e, "timers.results", t.Results);

            var r = c.Rounds;
            e.Check(r.Count >= 1, "rounds.count", "must be 1 or more");
            e.Check(r.QuestionsPerRound >= 1, "rounds.questionsPerRound", "must be 1 or more");
            if (r.Multipliers == null || r.Multipliers.Length != r.Count)
                e.Add("rounds.multipliers", "must have one multiplier per round (rounds.count)");
            else
                foreach (var m in r.Multipliers) e.Check(m > 0, "rounds.multipliers", "every multiplier must be more than 0");
            if (r.DifficultyOrder == null || r.DifficultyOrder.Length != r.QuestionsPerRound)
                e.Add("rounds.difficultyOrder", "must have one entry per question slot (rounds.questionsPerRound)");
            else
                foreach (var d in r.DifficultyOrder)
                    e.Check(Difficulties.IsValid(d), "rounds.difficultyOrder", "\"" + d + "\" must be easy, medium or hard");
            if (r.FormatOrder == null || r.FormatOrder.Length != r.QuestionsPerRound)
                e.Add("rounds.formatOrder", "must have one entry per question slot (rounds.questionsPerRound)");
            else
                foreach (var f in r.FormatOrder)
                    e.Check(f == QuestionDef.MultipleChoice || f == QuestionDef.ClosestValue, "rounds.formatOrder",
                        "\"" + f + "\" must be multipleChoice or closestValue");

            var s = c.Scoring;
            e.Check(s.MaxPoints > 0, "scoring.maxPoints", "must be more than 0");
            e.Check(s.SpeedPenalty >= 0 && s.SpeedPenalty <= 1, "scoring.speedPenalty", "must be 0 to 1");
            e.Check(s.RoundTo > 0, "scoring.roundTo", "must be more than 0");
            e.Check(s.FastAnswerMs >= 0, "scoring.fastAnswerMs", "must be 0 or more");

            var p = c.Players;
            e.Check(p.Min >= 1, "players.min", "must be 1 or more");
            e.Check(p.Max >= p.Min, "players.max", "must be at least players.min");
            e.Check(p.SeatHoldSeconds > 0, "players.seatHoldSeconds", "must be more than 0");
            if (p.TimerMultiplierOptions == null || p.TimerMultiplierOptions.Length == 0)
                e.Add("players.timerMultiplierOptions", "must not be empty");
            else
                foreach (var m in p.TimerMultiplierOptions)
                    e.Check(m > 0, "players.timerMultiplierOptions", "every option must be more than 0");

            Positive(e, "lobby.startCountdownSeconds", c.Lobby.StartCountdownSeconds);
            e.Check(c.Net.InputGraceMs >= 0, "net.inputGraceMs", "must be 0 or more");
            Positive(e, "net.tvReconnectSeconds", c.Net.TvReconnectSeconds);

            // Staging (docs/21_staging.md): extras are 0 or more, and every staging animation fits inside its phase.
            var x = c.Staging.ExtraSeconds;
            e.Check(x.QuestionShow >= 0, "staging.extraSeconds.questionShow", "must be 0 or more");
            e.Check(x.Reveal >= 0, "staging.extraSeconds.reveal", "must be 0 or more");
            e.Check(x.Scores >= 0, "staging.extraSeconds.scores", "must be 0 or more");
            e.Check(x.RoundEnd >= 0, "staging.extraSeconds.roundEnd", "must be 0 or more");
            e.Check(Num(root, "staging.questionShow.curtainOpenSeconds") <= t.QuestionShow + x.QuestionShow,
                "staging.questionShow.curtainOpenSeconds", "must fit inside QuestionShow");
            e.Check(Num(root, "staging.questionShow.questionInSeconds") < t.QuestionShow + x.QuestionShow,
                "staging.questionShow.questionInSeconds", "must be shorter than QuestionShow");
            e.Check(Num(root, "staging.reveal.cardsUpSeconds") + Num(root, "staging.reveal.reactionSeconds") <= t.Reveal + x.Reveal,
                "staging.reveal", "cards up plus reactions must fit inside Reveal");
            e.Check(Num(root, "staging.scores.overlayInSeconds") < t.Scores + x.Scores,
                "staging.scores.overlayInSeconds", "must be shorter than Scores");
            e.Check(Num(root, "staging.answer.suspenseRiseSeconds") <= Math.Min(t.Answer.MultipleChoice, t.Answer.ClosestValue),
                "staging.answer.suspenseRiseSeconds", "must fit inside the shortest answer window");
            e.Check(Num(root, "staging.intro.establishSeconds") < t.Intro, "staging.intro.establishSeconds",
                "must be shorter than the Intro");
            e.Check(Num(root, "staging.results.winnerWalkSeconds") < t.Results, "staging.results.winnerWalkSeconds",
                "must be shorter than Results");

            e.Check(c.ClosestValue.ClosestBonus >= 0, "closestValue.closestBonus", "must be 0 or more");
            e.Check(c.ClosestValue.MinPoints >= 0 && c.ClosestValue.MinPoints <= s.MaxPoints, "closestValue.minPoints",
                "must be 0 to scoring.maxPoints");
            e.Check(c.ClosestValue.ExactShare >= 0 && c.ClosestValue.ExactShare < 1, "closestValue.exactShare",
                "must be 0 or more and below 1");
            var race = c.Race;
            e.Check(race.MaxHeadStart >= 0, "race.maxHeadStart", "must be 0 or more");
            e.Check(race.FinishSteps > race.MaxHeadStart, "race.finishSteps", "must be more than race.maxHeadStart");
            e.Check(race.MaxQuestions >= 1, "race.maxQuestions", "must be 1 or more");
            e.Check(race.StepsPerCorrect >= 1, "race.stepsPerCorrect", "must be 1 or more");
            Positive(e, "race.answer", race.Answer);
            Positive(e, "race.reveal", race.Reveal);

            var pu = c.PowerUps;
            if (pu.Types == null)
                e.Add("powerUps.types", "must be a list");
            else
                foreach (var type in pu.Types)
                    e.Check(PowerUpTypes.IsValid(type), "powerUps.types", "\"" + type + "\" must be freeze, slime or fog");
            e.Check(TargetingModes.IsValid(pu.Targeting), "powerUps.targeting", "must be free or aimUp");
            e.Check(pu.DrawChanceLeader >= 0 && pu.DrawChanceLeader <= 1, "powerUps.drawChanceLeader", "must be 0 to 1");
            // The player object carries one heldPowerUp (PROTOCOL.md section 4), so one is all a player can hold.
            e.Check(pu.MaxHeld == 1, "powerUps.maxHeld", "must be 1 (the phone shows one held power up)");
            e.Check(pu.MaxHitsPerQuestion >= 1, "powerUps.maxHitsPerQuestion", "must be 1 or more");
            e.Check(pu.ImmunityQuestions >= 0, "powerUps.immunityQuestions", "must be 0 or more");
            e.Check(pu.Freeze.Taps >= 1, "powerUps.freeze.taps", "must be 1 or more");
            e.Check(pu.Slime.Blobs >= 1, "powerUps.slime.blobs", "must be 1 or more");
            Positive(e, "powerUps.fog.seconds", pu.Fog.Seconds);

            e.Check(c.Hosts.MinGapSeconds >= 0, "hosts.minGapSeconds", "must be 0 or more");
            Positive(e, "hosts.bubbleSeconds", c.Hosts.BubbleSeconds);
            e.Check(c.Hosts.StreakTrigger >= 1, "hosts.streakTrigger", "must be 1 or more");
            Positive(e, "poll.maxSeconds", c.Poll.MaxSeconds);

            // lastPlaceCanWin (docs/02_show_flow.md) with the largest head start gap: leader maxHeadStart, last place 0.
            double maxHeadStart = Num(root, "race.maxHeadStart");
            double maxQuestions = Num(root, "race.maxQuestions");
            e.Check(maxHeadStart < maxQuestions, "race", "last place could not win: maxHeadStart must be less than maxQuestions");
            e.Check(Num(root, "race.finishSteps") <= maxQuestions, "race",
                "last place could not win: finishSteps must be at most maxQuestions");
        }

        private static void Positive(ContentErrors e, string key, double value)
        {
            e.Check(value > 0, key, "must be more than 0");
        }

        /// <summary>A number at a key path; NaN (failing every comparison) when it is missing or not a number.</summary>
        private static double Num(JObject root, string path)
        {
            var token = root.SelectToken(path);
            if (token == null || (token.Type != JTokenType.Integer && token.Type != JTokenType.Float)) return double.NaN;
            return token.Value<double>();
        }
    }
}
