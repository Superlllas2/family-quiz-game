# 06 Phone screens and user flow

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; characters from `decisions/0005_staging_answers.md`; input only since `decisions/0013_day3_answers.md`, D88).

The phone page draws exactly one screen at a time, the one named in the last `screen` message from the TV app (`PROTOCOL.md`). It holds no game rules. Every screen has one decision at most. No phone has special rights: every player sees the same buttons.

The phone is an input device (D88): it shows a screen only when the player has something to do (join, pick a character, Ready, vote, answer, use a power up, the poll, settings). The rest of the time it shows `watch`. Questions, statements, results, ranks, scores, the race track and the Results are on the TV only. The one exception is the winner's fact on Results (D87).

## General rules for every screen

- Portrait only. Full height, no scrolling during play.
- Touch feedback on `pointerdown`, never on click: the pressed button changes at once (scale 0.95, brighter), with no wait for the network.
- After an input, show "Отправлено / Sent" with a small spinner, then a check mark when the `ack` arrives. No ack within 1 s: resend once; still nothing after 3 s: show "Нет связи / No connection".
- Top bar on every screen during a game: player name with their character chip and a gear icon for settings. No score or rank (D88).
- Placeholder look: dark background `#14161C`, white text, answer colors A `#E74C3C` (triangle), B `#3498DB` (diamond), C `#F1C40F` (circle), D `#2ECC71` (square). These are placeholders until the visual style guide exists.
- Characters: the roster in `content/characters.json` (`21_staging.md`). Until the 3D characters exist, a player's character is drawn as a chip: a rounded card in the character's signature color with the player's name in `#14161C`. No emoji anywhere. The TV draws chips the same way.
- Every visible text comes from the phone's locale file (`phone/src/locales/ru.json`, `en.json`) or from content in the player's chosen language. Texts the TV app sends in screen data (`countdown.text`, `waiting.locked`, `results.fact`, `error.text`) come from `content/ui_strings.json` (D65). The phone page uses the system font for now.
- Request a Screen Wake Lock after the first tap, and again whenever the page becomes visible.
- No sounds on the phone.
- The answer clock (`ms`) starts when the options appear and keeps running under a sabotage overlay. A new `screen` message with the same phaseId (last call) updates the countdown but never restarts the clock.

## Screens

| Screen id | Shown during | Content | Decision | Next |
| --- | --- | --- | --- | --- |
| `join` | Opening the link or QR | Room code field (filled from the URL), name field (1 to 16 characters: letters, Latin or Cyrillic, digits and spaces), language toggle RU or EN (default: Russian), Join button | Join | `pickCharacter` |
| `pickCharacter` | After joining, in the Lobby | A grid of character cards from `characters.json`, each in its signature color with the character's name and personality line; taken ones greyed; the one the TV assigned starts selected; a Confirm button below the grid (D77) | Tap a card to select it (phone only), Confirm to pick it (ack; `characterTaken` if someone just took it, then the screen refreshes; a selected card someone else takes falls back to the assigned one) | `lobby` |
| `lobby` | Lobby | "Ты в игре! / You're in!", list of players with a ready mark each, a Ready button that becomes Not ready once tapped; "Ждём игроков / Waiting for more players" while fewer than 2 are connected, else "Ждём остальных / Waiting for everyone" once ready | Ready or Not ready | `countdown` when everyone is ready |
| `countdown` | StartCountdown, when everyone is ready | "Игра начинается / Starting" and a big 3, 2, 1, Not ready button | Not ready | `lobby` if stopped, else the next screen sent |
| `watch` | Intro, QuestionShow, Reveal and Scores (no power up held), RoundEnd, FinalIntro, RaceReveal, Results (not the winner) | The player's character chip and "Смотри на экран / Eyes on the TV" | None | Next screen sent |
| `topicVote` | TopicVote | Four topic buttons | Vote | `waiting` |
| `answerChoice` | Answer, multiple choice | Four buttons 2x2 with the option texts, each a quarter of the space below the top bar; the countdown. No question text: it is on the TV | Tap one | `waiting` |
| `answerSlider` | Answer, closest value | Slider, big value with the unit, plus and minus, Lock, the countdown. No question text | Lock | `waiting` |
| `answerTrueFalse` | RaceQuestion | Two buttons, top and bottom halves, "Правда / True" and "Ложь / False"; the countdown. No statement text | Tap one | `waiting` |
| `sabotage` | Overlay on any answer screen when hit | Freeze: ice layer with a tap counter. Slime: 3 blobs to swipe off. Fog: blur layer that clears in 3 s or faster by rubbing. Text: "{name} заморозил тебя! / {name} froze you!" | Clear it | The answer screen below |
| `waiting` | After an input, until the phase ends | "Ответ принят / Locked: B" (or the value, the door, True or False) with the ack check mark | None | Next screen sent |
| `result` | Reveal and Scores (one phaseId), only for a player holding a power up (Day 4) | The power up card, tap it, then a row of target buttons (greyed if hit or immune) | Use power up (optional) | Next screen sent |
| `results` | Results, the winner only | "Победа! / You won!", "Твоя награда — интересный факт / Your prize: a fun fact" and the fact (D87) | None | `poll`, or `lobby` with the poll off |
| `poll` | Poll | One question per page, seven pages (`20_playtest.md` lists them); Next and Submit | Answer | `pollDone` |
| `pollDone` | After submitting, until the poll phase is over | "Спасибо! / Thanks!" | None | `lobby` once the poll phase is over (everyone submitted or 90 s passed) |
| `settings` | Gear icon, any time | Language RU or EN, "More time" 1x, 1.5x or 2x, Leave game (sends `leave`; the player is out for the rest of the game) | Change | Back to the screen before, or `join` after Leave |
| `reconnecting` | Connection lost, or `tvAway` from the relay | "Переподключение… / Reconnecting…" and a spinner. Retries every 2 s | None | `snapshot` restores the right screen |
| `error` | Room not found, game full, name taken, bad name, game already started, old page version, opened in another tab | Message and a Back button | Back | `join` |

## Flow

```
join -> pickCharacter -> lobby -> countdown -> watch (intro)
  -> [ topicVote -> waiting
       -> 4 x ( watch -> answerChoice | answerSlider -> waiting -> watch or result ) -> watch ] x 4
  -> watch (final intro) -> up to 12 x ( answerTrueFalse -> waiting -> watch )
  -> results (winner) or watch -> poll -> pollDone -> lobby (everyone Ready again: countdown -> topicVote)
countdown -> Not ready -> back to lobby.
Back to lobby (TV settings panel) sends every phone to lobby from any screen.
settings and reconnecting can appear at any point and return to the current screen.
```
