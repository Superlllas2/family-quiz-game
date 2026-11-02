using Newtonsoft.Json;

namespace FamilyQuiz.Core.Content
{
    /// <summary>Parses and checks content/config.json.</summary>
    public static class ConfigLoader
    {
        public const string FileName = "config.json";

        public static GameConfig Parse(string json)
        {
            GameConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<GameConfig>(json);
            }
            catch (JsonException e)
            {
                throw new ContentException(FileName, e.Message, e);
            }

            if (config == null) throw new ContentException(FileName, "file is empty");
            Check(config.Players.Min >= 1, "players.min must be 1 or more");
            Check(config.Players.Max >= config.Players.Min, "players.max must be at least players.min");
            Check(config.Players.SeatHoldSeconds > 0, "players.seatHoldSeconds must be more than 0");
            Check(config.Players.TimerMultiplierOptions != null && config.Players.TimerMultiplierOptions.Length > 0,
                "players.timerMultiplierOptions must not be empty");
            Check(config.Net.TvReconnectSeconds > 0, "net.tvReconnectSeconds must be more than 0");
            Check(config.Net.InputGraceMs >= 0, "net.inputGraceMs must be 0 or more");
            return config;
        }

        private static void Check(bool ok, string message)
        {
            if (!ok) throw new ContentException(FileName, message);
        }
    }
}
