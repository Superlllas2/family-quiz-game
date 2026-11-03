using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
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
        private const string IntroTextKey = "screen.getReady.intro";
        private const string FinalIntroTextKey = "screen.getReady.finalIntro";
        private const string WaitingLockedKey = "screen.waiting.locked";
        private const string WaitingValueKey = "screen.waiting.value";
        private const string ResultCorrectTextKey = "screen.result.correctText";
        private const string ResultCorrectValueKey = "screen.result.correctValue";
        private const string TrueKey = "screen.trueFalse.true";
        private const string FalseKey = "screen.trueFalse.false";
        private const string AwardMostCorrectKey = "screen.awards.mostCorrect";
        private const string AwardBestComebackKey = "screen.awards.bestComeback";

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
                        data = Lobby(player);
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
                case GamePhase.Intro:
                    id = ScreenIds.GetReady;
                    data = new GetReadyScreenData { Text = Text(IntroTextKey, player.Lang), Prediction = Prediction() };
                    break;
                case GamePhase.TopicVote:
                    if (state.Round != null && state.Round.Votes.ContainsKey(player.PlayerId))
                    {
                        id = ScreenIds.Waiting;
                        data = Waiting(player);
                    }
                    else
                    {
                        id = ScreenIds.TopicVote;
                        data = TopicVote(player);
                    }

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
                    else if (state.Question != null && state.Question.IsClosestValue)
                    {
                        id = ScreenIds.AnswerSlider;
                        data = AnswerSlider(player);
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
                case GamePhase.RoundEnd:
                    id = ScreenIds.RoundEnd;
                    data = RoundEnd(player);
                    break;
                case GamePhase.FinalIntro:
                    id = ScreenIds.GetReady;
                    data = new GetReadyScreenData { Text = Text(FinalIntroTextKey, player.Lang) };
                    break;
                case GamePhase.RaceQuestion:
                    if (player.Turn != null && player.Turn.Locked)
                    {
                        id = ScreenIds.Waiting;
                        data = Waiting(player);
                    }
                    else
                    {
                        id = ScreenIds.AnswerTrueFalse;
                        data = new AnswerTrueFalseScreenData
                        {
                            Statement = state.Race?.Statement?.Statement?.Get(player.Lang) ?? "",
                            WindowMs = player.Turn?.WindowMs ?? 0
                        };
                    }

                    break;
                case GamePhase.RaceReveal:
                    id = ScreenIds.RaceResult;
                    data = new RaceResultScreenData
                    {
                        Correct = player.Turn != null && player.Turn.Correct,
                        Position = state.Race?.PositionOf(player.PlayerId) ?? 0,
                        Finish = _game.Config.Race.FinishSteps
                    };
                    break;
                case GamePhase.Results:
                    id = ScreenIds.Results;
                    data = Results(player);
                    break;
                default:
                    id = ScreenIds.Lobby;
                    data = Lobby(player);
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
        /// The connected players still to send their input, in seat order: in TopicVote those who have not voted; in
        /// Answer and RaceQuestion those not locked in and before their deadline plus net.inputGraceMs (D73).
        /// </summary>
        public List<PlayerState> PendingPlayers()
        {
            var state = _game.State;
            var pending = new List<PlayerState>();
            var now = _game.NowMs;
            foreach (var p in state.Players)
            {
                if (!p.Connected) continue;
                switch (state.Phase)
                {
                    case GamePhase.TopicVote:
                        if (state.Round != null && !state.Round.Votes.ContainsKey(p.PlayerId)) pending.Add(p);
                        break;
                    case GamePhase.Answer:
                    case GamePhase.RaceQuestion:
                        if (p.Turn != null && !p.Turn.Locked && now < p.Turn.DeadlineMs + _game.Config.Net.InputGraceMs) pending.Add(p);
                        break;
                }
            }

            return pending;
        }

        /// <summary>True when this player sees the waiting screen now (sent their vote or locked their answer).</summary>
        public bool IsWaiting(PlayerState player)
        {
            var state = _game.State;
            switch (state.Phase)
            {
                case GamePhase.TopicVote:
                    return state.Round != null && state.Round.Votes.ContainsKey(player.PlayerId);
                case GamePhase.Answer:
                case GamePhase.RaceQuestion:
                    return player.Turn != null && player.Turn.Locked;
                default:
                    return false;
            }
        }

        private LobbyScreenData Lobby(PlayerState player)
        {
            var players = new List<LobbyPlayerDto>(_game.State.Players.Count);
            foreach (var p in _game.State.Players)
            {
                players.Add(new LobbyPlayerDto { PlayerId = p.PlayerId, Name = p.Name, CharacterId = p.CharacterId, Ready = p.Ready });
            }

            return new LobbyScreenData { Players = players, Ready = player.Ready, EnoughPlayers = _game.EnoughPlayers };
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

        private List<PredictionPlayerDto> Prediction()
        {
            var list = new List<PredictionPlayerDto>();
            foreach (var p in _game.State.Players)
            {
                list.Add(new PredictionPlayerDto { PlayerId = p.PlayerId, Name = p.Name, CharacterId = p.CharacterId });
            }

            return list;
        }

        private TopicVoteScreenData TopicVote(PlayerState player)
        {
            var data = new TopicVoteScreenData();
            var round = _game.State.Round;
            if (round == null) return data;
            foreach (var door in round.Doors) data.Doors.Add(new DoorDto { Title = door.Title.Get(player.Lang) });
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

        private AnswerSliderScreenData AnswerSlider(PlayerState player)
        {
            var question = _game.State.Question;
            var def = question.Def;
            return new AnswerSliderScreenData
            {
                Question = def.Text?.Get(player.Lang) ?? "",
                Min = def.Range[0],
                Max = def.Range[1],
                Step = def.Step ?? 1,
                Unit = def.Unit?.Get(player.Lang) ?? "",
                WindowMs = player.Turn?.WindowMs ?? question.BaseWindowMs
            };
        }

        private WaitingScreenData Waiting(PlayerState player)
        {
            var data = new WaitingScreenData { Locked = LockedText(player) };
            foreach (var p in PendingPlayers())
            {
                data.Pending.Add(new PendingPlayerDto { Name = p.Name, CharacterId = p.CharacterId });
            }

            return data;
        }

        /// <summary>What the player sent: the door title, "B · Danube", "1889 m" or "True".</summary>
        private string LockedText(PlayerState player)
        {
            var state = _game.State;
            switch (state.Phase)
            {
                case GamePhase.TopicVote:
                    return state.Round.Votes.TryGetValue(player.PlayerId, out var door) ? state.Round.Doors[door].Title.Get(player.Lang) : "";
                case GamePhase.RaceQuestion:
                    return player.Turn?.TrueFalse == true ? Text(TrueKey, player.Lang) : Text(FalseKey, player.Lang);
                default:
                    if (state.Question != null && state.Question.IsClosestValue)
                        return ValueText(WaitingValueKey, player.Turn?.Value ?? 0, player.Lang);
                    return OptionLabel(WaitingLockedKey, player.Turn?.Choice ?? -1, player.Lang);
            }
        }

        private ResultScreenData Result(PlayerState player)
        {
            var question = _game.State.Question;
            string correctText = "";
            if (question != null)
            {
                correctText = question.IsClosestValue
                    ? ValueText(ResultCorrectValueKey, question.Def.Answer ?? 0, player.Lang)
                    : OptionLabel(ResultCorrectTextKey, question.CorrectDisplayIndex, player.Lang);
            }

            return new ResultScreenData
            {
                Correct = player.Turn != null && player.Turn.Correct,
                Points = player.Turn?.Points ?? 0,
                CorrectText = correctText,
                Streak = player.Streak,
                Rank = Ranking.SharedRank(_game.State, player),
                PowerUp = null
            };
        }

        private RoundEndScreenData RoundEnd(PlayerState player)
        {
            var state = _game.State;
            int next = (state.Round?.Index ?? 0) + 2;
            return new RoundEndScreenData
            {
                Rank = Ranking.SharedRank(state, player),
                Score = player.Score,
                NextRound = next <= state.RoundCount ? next : (int?)null
            };
        }

        private ResultsScreenData Results(PlayerState player)
        {
            var state = _game.State;
            int place = 0;
            for (int i = 0; i < state.Placement.Count; i++)
            {
                if (state.Placement[i] == player.PlayerId) place = i + 1;
            }

            var data = new ResultsScreenData
            {
                Place = place > 0 ? place : Ranking.SharedRank(state, player),
                CorrectCount = player.CorrectCount
            };
            if (Contains(state.MostCorrectIds, player.PlayerId)) data.Awards.Add(Text(AwardMostCorrectKey, player.Lang));
            if (Contains(state.BestComebackIds, player.PlayerId)) data.Awards.Add(Text(AwardBestComebackKey, player.Lang));
            return data;
        }

        private static bool Contains(IReadOnlyList<string> ids, string id)
        {
            foreach (var x in ids)
            {
                if (x == id) return true;
            }

            return false;
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

        /// <summary>A text with {value} and {unit} of the current closest value question.</summary>
        private string ValueText(string key, double value, string lang)
        {
            var strings = _game.Content.Strings;
            if (strings == null) return key;
            var unit = _game.State.Question?.Def.Unit?.Get(lang) ?? "";
            var values = new Dictionary<string, string> { { "value", NumberText.Format(value) }, { "unit", unit } };
            return NumberText.WithUnit(strings.Format(key, lang, values));
        }

        private string Text(string key, string lang)
        {
            var strings = _game.Content.Strings;
            return strings == null ? key : strings.Get(key, lang);
        }
    }
}
