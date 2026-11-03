using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// The host line triggers that fire at a Reveal (docs/15_schemas.md, "Triggers for game night 1"), read from the
    /// scored turns, in priority order (HostTriggers.CyclePriority). "Correct" on a closest value question means within
    /// the tolerance (D83). When several players fit {name}, one is picked with the seeded random generator.
    /// </summary>
    public static class RevealTriggers
    {
        public static List<HostLineRequested> Fired(GameState state, GameConfig config, IRandom random)
        {
            var fired = new List<HostLineRequested>();
            var question = state.Question;
            if (question == null) return fired;
            int cycle = question.Number;
            var withTurn = state.Players.Where(p => p.Turn != null && p.Turn.Revealed).ToList();
            if (withTurn.Count == 0) return fired;
            var correct = withTurn.Where(p => p.Turn.Correct).ToList();

            // Last place before the question: the highest shared rank then. While every player shares one rank (the
            // start of a game) nobody is in last place.
            int lastRank = withTurn.Max(p => p.Turn.RankBefore);
            if (lastRank > 1)
                Add(fired, HostTriggers.LastPlaceCorrect, correct.Where(p => p.Turn.RankBefore == lastRank), cycle, random);

            Add(fired, HostTriggers.NewLeader,
                withTurn.Where(p => Ranking.SharedRank(state, p) == 1 && p.Turn.RankBefore != 1), cycle, random);
            Add(fired, HostTriggers.Streak3, correct.Where(p => p.Streak == config.Hosts.StreakTrigger), cycle, random);
            if (question.IsClosestValue && question.Def.Answer.HasValue)
                Add(fired, HostTriggers.ClosestExact,
                    withTurn.Where(p => p.Turn.Value.HasValue && p.Turn.Value.Value == question.Def.Answer.Value), cycle, random);

            var connected = withTurn.Where(p => p.Connected).ToList();
            if (connected.Count > 0 && connected.All(p => p.Turn.Correct))
                fired.Add(new HostLineRequested(HostTriggers.EveryoneCorrect, cycle: cycle));
            if (correct.Count == 0) fired.Add(new HostLineRequested(HostTriggers.NobodyCorrect, cycle: cycle));

            // The single fastest correct player, below scoring.fastAnswerMs; a tie for fastest means no single one.
            var timed = correct.Where(p => p.Turn.Ms.HasValue).ToList();
            if (timed.Count > 0)
            {
                long fastest = timed.Min(p => p.Turn.Ms.Value);
                var fastestPlayers = timed.Where(p => p.Turn.Ms.Value == fastest).ToList();
                if (fastestPlayers.Count == 1 && fastest < config.Scoring.FastAnswerMs)
                    fired.Add(new HostLineRequested(HostTriggers.FastAnswer, fastestPlayers[0].Name, cycle: cycle));
            }

            return fired;
        }

        private static void Add(List<HostLineRequested> fired, string trigger, IEnumerable<PlayerState> fits, int cycle, IRandom random)
        {
            var list = fits.ToList();
            if (list.Count == 0) return;
            var who = list.Count == 1 ? list[0] : list[random.Next(list.Count)];
            fired.Add(new HostLineRequested(trigger, who.Name, cycle: cycle));
        }
    }
}
