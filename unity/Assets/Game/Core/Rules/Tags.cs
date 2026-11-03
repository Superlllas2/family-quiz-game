using System.Collections.Generic;
using System.Linq;
using FamilyQuiz.Core.State;

namespace FamilyQuiz.Core.Rules
{
    /// <summary>
    /// The funny tags on Results (D86): every player except the winner gets exactly one, no tag twice in a game. A stat
    /// tag goes only to a player who really is the most (or least) of all players, the winner included, so a tag is
    /// never untrue; ties among untagged players are broken with the seeded random. The stat tags are tried in a seeded
    /// order for variety; players left without one get a kind fallback tag. Texts are tv.tags.(id) in ui_strings.json.
    /// </summary>
    public static class Tags
    {
        public const string MostMisses = "mostMisses";
        public const string Slowest = "slowest";
        public const string Hasty = "hasty";
        public const string WildGuess = "wildGuess";
        public const string LastOne = "lastOne";
        public const string Daydreamer = "daydreamer";
        public const string Outvoted = "outvoted";
        public const string RaceTortoise = "raceTortoise";

        public const string DarkHorse = "darkHorse";
        public const string GoodSport = "goodSport";
        public const string Steady = "steady";
        public const string Mystery = "mystery";
        public const string Calm = "calm";
        public const string NextTime = "nextTime";
        public const string StarOfTheEvening = "starOfTheEvening";

        /// <summary>Stat tags; each scores a player, higher fits better, and 0 or less never earns the tag.</summary>
        public static readonly IReadOnlyList<string> StatTags = new[]
        {
            MostMisses, Slowest, Hasty, WildGuess, LastOne, Daydreamer, Outvoted, RaceTortoise
        };

        /// <summary>
        /// Tags that need no stat, handed out in a seeded order after the stat tags. Seven of them, so up to seven
        /// non-winners (players.max 8) never share one even when no stat tag applies.
        /// </summary>
        public static readonly IReadOnlyList<string> FallbackTags = new[]
        {
            DarkHorse, GoodSport, Steady, Mystery, Calm, NextTime, StarOfTheEvening
        };

        /// <summary>Every tag id (localization keys tv.tags.(id)).</summary>
        public static IEnumerable<string> All => StatTags.Concat(FallbackTags);

        /// <summary>How well a player fits a stat tag; above 0 to earn it.</summary>
        public static double Fit(string tag, PlayerState player, IReadOnlyList<PlayerState> players)
        {
            var s = player.Stats;
            switch (tag)
            {
                case MostMisses: return s.Wrong;
                case Slowest: return s.AverageWindowShare;
                case Hasty: return s.HastyWrong;
                case WildGuess: return s.WorstMiss > 1 ? s.WorstMiss : 0;
                case LastOne: return s.LastCalls;
                case Daydreamer: return s.Unanswered;
                case Outvoted: return s.LostVotes;
                case RaceTortoise:
                {
                    // The fewest steps, and only when somebody moved more.
                    int most = players.Max(p => p.Stats.RaceSteps);
                    return most - s.RaceSteps;
                }
                default: return 0;
            }
        }

        /// <summary>playerId to tag id for every player but the winner.</summary>
        public static Dictionary<string, string> Assign(IReadOnlyList<PlayerState> players, string winnerId, IRandom random)
        {
            var result = new Dictionary<string, string>();
            var untagged = players.Where(p => p.PlayerId != winnerId).ToList();
            if (untagged.Count == 0) return result;

            var order = OptionShuffle.DisplayOrder(StatTags.Count, random).Select(i => StatTags[i]).ToList();
            foreach (var tag in order)
            {
                if (untagged.Count == 0) break;
                double best = players.Max(p => Fit(tag, p, players));
                if (best <= 0) continue;
                var tied = untagged.Where(p => Fit(tag, p, players) == best).ToList();
                if (tied.Count == 0) continue; // the most is the winner or a player already tagged
                var pick = tied.Count == 1 ? tied[0] : tied[random.Next(tied.Count)];
                result[pick.PlayerId] = tag;
                untagged.Remove(pick);
            }

            var fallbacks = OptionShuffle.DisplayOrder(FallbackTags.Count, random).Select(i => FallbackTags[i]).ToList();
            for (int i = 0; i < untagged.Count; i++) result[untagged[i].PlayerId] = fallbacks[i % fallbacks.Count];

            return result;
        }
    }
}
