using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.State;
using FamilyQuiz.Core.Rules;
using FamilyQuiz.Net.Protocol;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// Builds the per phone screen and player objects from the game state (read only): PROTOCOL.md section 4 and
    /// docs/06_phone_screens.md. Every text is in the player's language, from content/ui_strings.json or content.
    /// </summary>
    public sealed class ScreenBuilder
    {
        public const string LobbyScreenId = ScreenIds.Lobby;

        private const string CountdownTextKey = "screen.countdown.text";
        private const string WaitingLockedKey = "screen.waiting.locked";
        private const string ResultCorrectTextKey = "screen.result.correctText";
        private const string AwardMostCorrectKey = "screen.awards.mostCorrect";

        private readonly Game _game;

        public ScreenBuilder(Game game)
        {
            _game = game;
        }

        /// <summary>The screen this player should see now.</summary>
        public ScreenDto CurrentScreen(PlayerState player)
        {
            var state = _game.State;
            string id;
            object data;
            switch (state.Phase)
            {
                case GamePhase.Lobby:
                    if (player.CharacterPicked)
                    {
                        id = ScreenIds.Lobby;
                        data = Lobby();
                    }
                    else
                    {
                        id = ScreenIds.PickCharacter;
                        data = PickCharacter(player);
                    }

                    break;
                case GamePhase.StartCountdown:
                    id = ScreenIds.Countdown;
                    data = new CountdownScreenData { Text = Text(CountdownTextKey, player.Lang) };
                    break;
                case GamePhase.QuestionShow:
                    id = ScreenIds.GetReady;
                    data = new GetReadyScreenData { Text = state.Question?.Def.Text?.Get(player.Lang) ?? "" };
                    break;
                case GamePhase.Answer:
                    if (player.Turn != null && player.Turn.Locked)
                    {
                        id = ScreenIds.Waiting;
                        data = Waiting(player);
                    }
                    else
                    {
                        id = ScreenIds.AnswerChoice;
                        data = AnswerChoice(player);
                    }

                    break;
                case GamePhase.Reveal:
                case GamePhase.Scores:
                    id = ScreenIds.Result;
                    data = Result(player);
                    break;
                case GamePhase.Results:
                    id = ScreenIds.Results;
                    data = Results(player);
                    break;
                case GamePhase.PlayAgain:
                    id = ScreenIds.PollDone;
                    data = new PollDoneScreenData { CanPlayAgain = _game.CanPlayAgain };
                    break;
                default:
                    id = ScreenIds.Lobby;
                    data = Lobby();
                    break;
            }

            return new ScreenDto { PhaseId = state.PhaseId, Id = id, EndsInMs = _game.EndsInMs(player), Data = data };
        }

        public PlayerDto Player(PlayerState player)
        {
            return new PlayerDto
            {
                PlayerId = player.PlayerId,
                Name = player.Name,
                CharacterId = player.CharacterId,
                Lang = player.Lang,
                Score = player.Score,
                Rank = Ranking.SharedRank(_game.State, player),
                Streak = player.Streak,
                HeldPowerUp = player.HeldPowerUp,
                TimerMultiplier = player.TimerMultiplier
            };
        }

        /// <summary>
        /// The connected players still answering in Answer: not locked in and before their deadline plus
        /// net.inputGraceMs (D73), in seat order.
        /// </summary>
        public List<PlayerState> PendingPlayers()
        {
            var state = _game.State;
            var pending = new List<PlayerState>();
            if (state.Phase != GamePhase.Answer) return pending;
            var now = _game.NowMs;
            foreach (var p in state.Players)
            {
                if (p.Connected && p.Turn != null && !p.Turn.Locked && now < p.Turn.DeadlineMs + _game.Config.Net.InputGraceMs)
                    pending.Add(p);
            }

            return pending;
        }

        private LobbyScreenData Lobby()
        {
            var players = new List<LobbyPlayerDto>(_game.State.Players.Count);
            foreach (var p in _game.State.Players)
            {
                players.Add(new LobbyPlayerDto { PlayerId = p.PlayerId, Name = p.Name, CharacterId = p.CharacterId });
            }

            return new LobbyScreenData { Players = players, CanStart = _game.CanStart };
        }

        private PickCharacterScreenData PickCharacter(PlayerState player)
        {
            var data = new PickCharacterScreenData { Current = player.CharacterId };
            foreach (var p in _game.State.Players)
            {
                if (p != player) data.Taken.Add(p.CharacterId);
            }

            return data;
        }

        private AnswerChoiceScreenData AnswerChoice(PlayerState player)
        {
            var question = _game.State.Question;
            var data = new AnswerChoiceScreenData
            {
                Question = question?.Def.Text?.Get(player.Lang) ?? "",
                WindowMs = player.Turn?.WindowMs ?? question?.BaseWindowMs ?? 0
            };
            if (question != null)
            {
                for (int i = 0; i < question.DisplayOrder.Length; i++) data.Options.Add(question.OptionText(i, player.Lang));
            }

            return data;
        }

        private WaitingScreenData Waiting(PlayerState player)
        {
            var data = new WaitingScreenData { Locked = OptionLabel(WaitingLockedKey, player.Turn.Choice ?? -1, player.Lang) };
            foreach (var p in PendingPlayers())
            {
                data.Pending.Add(new PendingPlayerDto { Name = p.Name, CharacterId = p.CharacterId });
            }

            return data;
        }

        private ResultScreenData Result(PlayerState player)
        {
            var question = _game.State.Question;
            return new ResultScreenData
            {
                Correct = player.Turn != null && player.Turn.Correct,
                Points = player.Turn?.Points ?? 0,
                CorrectText = question != null ? OptionLabel(ResultCorrectTextKey, question.CorrectDisplayIndex, player.Lang) : "",
                Streak = player.Streak,
                Rank = Ranking.SharedRank(_game.State, player),
                PowerUp = null
            };
        }

        private ResultsScreenData Results(PlayerState player)
        {
            var data = new ResultsScreenData
            {
                Place = Ranking.SharedRank(_game.State, player),
                CorrectCount = player.CorrectCount
            };
            foreach (var id in _game.State.MostCorrectIds)
            {
                if (id == player.PlayerId)
                {
                    data.Awards.Add(Text(AwardMostCorrectKey, player.Lang));
                    break;
                }
            }

            return data;
        }

        /// <summary>A text with {letter} and {text} of one answer position of the current question.</summary>
        private string OptionLabel(string key, int displayIndex, string lang)
        {
            var question = _game.State.Question;
            var optionText = question != null && displayIndex >= 0 && displayIndex < question.DisplayOrder.Length
                ? question.OptionText(displayIndex, lang)
                : "";
            var values = new Dictionary<string, string> { { "letter", AnswerLetters.For(displayIndex) }, { "text", optionText } };
            var strings = _game.Content.Strings;
            if (strings == null) return key;
            return strings.Format(key, lang, values);
        }

        private string Text(string key, string lang)
        {
            var strings = _game.Content.Strings;
            return strings == null ? key : strings.Get(key, lang);
        }
    }
}
