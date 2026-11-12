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
    /// docs/06_phone_screens.md. Every text is in the player's language, from content/ui_strings.json or content. The
    /// phone is an input device (D88): a screen with a decision, or "watch" the rest of the time.
    /// </summary>
    public sealed class ScreenBuilder
    {
        public const string LobbyScreenId = ScreenIds.Lobby;

        private const string CountdownTextKey = "screen.countdown.text";
        private const string WaitingLockedKey = "screen.waiting.locked";
        private const string WaitingValueKey = "screen.waiting.value";
        private const string TrueKey = "screen.trueFalse.true";
        private const string FalseKey = "screen.trueFalse.false";
        private const string PassKey = "screen.trueFalse.pass";
        private const string WaitingPowerUpKey = "screen.waiting.powerUp";
        private const string PowerUpNameKeyPrefix = "screen.powerUps.";
        private const string PausedTextKey = "screen.paused.text";
        private const string RateTitleKey = "screen.rate.title";
        private const string RateCloseKey = "screen.rate.close";
        private const string RateKeyPrefix = "screen.rate.";

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
            if (state.Paused)
            {
                // The pause menu is open on the laptop (D105): every phone shows paused, whatever the phase.
                return new ScreenDto
                {
                    PhaseId = state.PhaseId, Id = ScreenIds.Paused, EndsInMs = _game.EndsInMs(player),
                    Data = new PausedScreenData { Text = Text(PausedTextKey, player.Lang) }
                };
            }

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
                case GamePhase.Attacks:
                    // The power up pick (one phaseId over Reveal and Scores): "result" for a holder with someone to aim
                    // at, "waiting" once they used it, "watch" for everyone else (D88).
                    // Each of them carries the rating card while question feedback is on (D109).
                    var rate = Rate(player);
                    if (player.UsedPowerUpInPhaseId == state.PhaseId && player.UsedPowerUp != null)
                    {
                        id = ScreenIds.Waiting;
                        data = new WaitingScreenData { Locked = UsedPowerUpText(player), Rate = rate };
                    }
                    else if (player.HeldPowerUp != null && _game.PickWindowOpen && _game.PowerUpTargets(player).Count > 0)
                    {
                        id = ScreenIds.Result;
                        var result = Result(player);
                        result.Rate = rate;
                        data = result;
                    }
                    else
                    {
                        id = ScreenIds.Watch;
                        data = new WatchScreenData { Rate = rate };
                    }

                    break;
                case GamePhase.Poll:
                    if (state.SubmittedPoll(player.PlayerId))
                    {
                        id = ScreenIds.PollDone;
                        data = new WatchScreenData();
                    }
                    else
                    {
                        id = ScreenIds.Poll;
                        data = Poll(player);
                    }

                    break;
                case GamePhase.Intro:
                case GamePhase.QuestionShow:
                case GamePhase.RoundEnd:
                case GamePhase.FinalIntro:
                case GamePhase.RaceReveal:
                case GamePhase.TopicTie:
                    id = ScreenIds.Watch;
                    data = new WatchScreenData();
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
                        data = new AnswerTrueFalseScreenData { WindowMs = player.Turn?.WindowMs ?? 0 };
                    }

                    break;
                case GamePhase.Results:
                    // The winner gets their fact (D87); everyone else looks at the TV.
                    if (player.PlayerId == state.WinnerId && state.WinnerFact != null)
                    {
                        id = ScreenIds.Results;
                        data = new ResultsScreenData { Fact = state.WinnerFact.Text.Get(player.Lang) };
                    }
                    else
                    {
                        id = ScreenIds.Watch;
                        data = new WatchScreenData();
                    }

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

        /// <summary>The rating card for this player (D109, D110), or null when no card is up.</summary>
        private RateDto Rate(PlayerState player)
        {
            if (!_game.RatingOpen) return null;
            var question = _game.State.Question;
            var feedback = _game.Config.Feedback;
            var dto = new RateDto
            {
                QuestionId = question.Def.Id,
                Title = Text(RateTitleKey, player.Lang),
                Close = Text(RateCloseKey, player.Lang),
                Stars = feedback.Stars
            };
            foreach (var item in feedback.Items)
            {
                dto.Items.Add(new RateItemDto
                {
                    Id = item,
                    Text = Text(RateKeyPrefix + item + ".text", player.Lang),
                    Low = Text(RateKeyPrefix + item + ".low", player.Lang),
                    High = Text(RateKeyPrefix + item + ".high", player.Lang),
                    Given = question.GivenRating(player.PlayerId, item)
                });
            }

            return dto;
        }

        private ResultScreenData Result(PlayerState player)
        {
            var data = new ResultScreenData { PowerUp = player.HeldPowerUp };
            foreach (var t in _game.PowerUpTargets(player))
                data.Targets.Add(new PowerUpTargetDto { PlayerId = t.Target.PlayerId, Name = t.Target.Name, Available = t.Available });
            return data;
        }

        /// <summary>"Заморозка → Мама" after a power up was used.</summary>
        private string UsedPowerUpText(PlayerState player)
        {
            var strings = _game.Content.Strings;
            if (strings == null) return player.UsedPowerUp;
            var target = _game.State.FindById(player.UsedPowerUpTarget);
            var values = new Dictionary<string, string>
            {
                { "powerUp", strings.Get(PowerUpNameKeyPrefix + player.UsedPowerUp, player.Lang) },
                { "target", target?.Name ?? "" }
            };
            return strings.Format(WaitingPowerUpKey, player.Lang, values);
        }

        private PollScreenData Poll(PlayerState player)
        {
            var data = new PollScreenData();
            foreach (var q in _game.Content.Poll)
            {
                var dto = new PollQuestionDto { Id = q.Id, Text = q.Text.Get(player.Lang), Kind = q.Kind };
                if (q.IsChoice)
                {
                    dto.Options = new List<string>();
                    foreach (var o in q.Options) dto.Options.Add(o.Get(player.Lang));
                }

                data.Questions.Add(dto);
            }

            return data;
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
                Min = def.Range[0],
                Max = def.Range[1],
                Step = def.Step ?? 1,
                Unit = def.Unit?.Get(player.Lang) ?? "",
                WindowMs = player.Turn?.WindowMs ?? question.BaseWindowMs
            };
        }

        private WaitingScreenData Waiting(PlayerState player)
        {
            return new WaitingScreenData { Locked = LockedText(player) };
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
                    if (player.Turn != null && player.Turn.Passed) return Text(PassKey, player.Lang);
                    return player.Turn?.TrueFalse == true ? Text(TrueKey, player.Lang) : Text(FalseKey, player.Lang);
                default:
                    if (state.Question != null && state.Question.IsClosestValue)
                        return ValueText(WaitingValueKey, player.Turn?.Value ?? 0, player.Lang);
                    return OptionLabel(WaitingLockedKey, player.Turn?.Choice ?? -1, player.Lang);
            }
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
