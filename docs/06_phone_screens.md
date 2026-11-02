# 06 Phone screens and user flow

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`).

The phone page draws exactly one screen at a time, the one named in the last `screen` message from the TV app (`PROTOCOL.md`). It holds no game rules. Every screen has one decision at most. No phone has special rights: every player sees the same buttons.

## General rules for every screen

- Portrait only. Full height, no scrolling during play.
- Touch feedback on `pointerdown`, never on click: the pressed button changes at once (scale 0.95, brighter), with no wait for the network.
- After an input, show "Отправлено / Sent" with a small spinner, then a check mark when the `ack` arrives. No ack within 1 s: resend once; still nothing after 3 s: show "Нет связи / No connection".
- Top bar on every screen during a game: player name, avatar, score, and a gear icon for settings.
- Placeholder look: dark background `#14161C`, white text, answer colors A `#E74C3C` (triangle), B `#3498DB` (diamond), C `#F1C40F` (circle), D `#2ECC71` (square). These are placeholders until the visual style guide exists.
- Avatars: the 12 presets in `content/avatars.json`, each drawn as its colored shape with the player's first letter, uppercase, in `#14161C`. No emoji anywhere. The TV draws them the same way.
- Every visible text comes from the phone's locale file (`phone/src/locales/ru.json`, `en.json`) or from content in the player's chosen language.
- Request a Screen Wake Lock after the first tap, and again whenever the page becomes visible.
- No sounds on the phone.
- The answer clock (`ms`) starts when the options appear and keeps running under a sabotage overlay. A new `screen` message with the same phaseId (last call) updates the countdown but never restarts the clock.

## Screens

| Screen id | Shown during | Content | Decision | Next |
| --- | --- | --- | --- | --- |
| `join` | Opening the link or QR | Room code field (filled from the URL), name field (1 to 16 characters: letters, Latin or Cyrillic, digits and spaces), language toggle RU or EN (default: Russian), Join button | Join | `pickAvatar` |
| `pickAvatar` | After joining, in the Lobby | The 12 preset avatars from `avatars.json` with the player's first letter; taken ones greyed; the one the TV assigned is selected | Pick one (ack; `avatarTaken` if someone just took it, then the screen refreshes) | `lobby` |
| `lobby` | Lobby | "Ты в игре! / You're in!", list of players, Start button on every phone (enabled at 2 or more players) | Start | `countdown` |
| `countdown` | StartCountdown, after Start or Play again | "Игра начинается / Starting" and a big 3, 2, 1, Cancel button | Cancel | `lobby` or `pollDone` if cancelled, else the next screen sent |
| `getReady` | Intro, FinalIntro, QuestionShow | A short text sent by the TV app (for QuestionShow: the question). In Intro: prediction "Кто победит? / Who wins tonight?" with player buttons | Prediction tap (Intro only) | Next screen sent |
| `topicVote` | TopicVote | Four topic buttons | Vote | `waiting` |
| `answerChoice` | Answer, multiple choice | Four buttons 2x2, each a quarter of the space below the top bar | Tap one | `waiting` |
| `answerSlider` | Answer, closest value | Slider, big value, plus and minus, Lock | Lock | `waiting` |
| `answerTrueFalse` | RaceQuestion | Two buttons, top and bottom halves | Tap one | `waiting` |
| `sabotage` | Overlay on any answer screen when hit | Freeze: ice layer with a tap counter. Slime: 3 blobs to swipe off. Fog: blur layer that clears in 3 s or faster by rubbing. Text: "{name} заморозил тебя! / {name} froze you!" | Clear it | The answer screen below |
| `waiting` | After an input, until the phase ends | "Ответ принят / Locked: B" (or the value), avatars of who still has to answer | None | Next screen sent |
| `result` | Reveal and Scores (one phaseId) | "Верно! +850 / Correct! +850" or "Мимо / Wrong", streak, rank. If the player holds a power up: its card, tap it, then a row of target buttons (greyed if hit or immune) | Use power up (optional) | Next screen sent |
| `roundEnd` | RoundEnd | Own rank and score, "Раунд 3 / Round 3 next" | None | Next screen sent |
| `raceResult` | Race reveal | "Верно, +1 шаг / Right, +1 step" or "Мимо / Missed", own position on a small track | None | Next screen sent |
| `results` | Results | Own place, correct answers, awards won | None | `poll` |
| `poll` | Poll | One question per page, seven pages (`20_playtest.md` lists them); Next and Submit | Answer | `pollDone` |
| `pollDone` | After submitting, and in PlayAgain | "Спасибо! / Thanks!". Once the poll phase is over (everyone submitted or 90 s passed), a Play again button on every phone (enabled at 2 or more players) | Play again | `countdown` |
| `settings` | Gear icon, any time | Language RU or EN, "More time" 1x, 1.5x or 2x, Leave game (sends `leave`; the player is out for the rest of the game) | Change | Back to the screen before, or `join` after Leave |
| `reconnecting` | Connection lost, or `tvAway` from the relay | "Переподключение… / Reconnecting…" and a spinner. Retries every 2 s | None | `snapshot` restores the right screen |
| `error` | Room not found, game full, name taken, bad name, game already started, old page version, opened in another tab | Message and a Back button | Back | `join` |

## Flow

```
join -> pickAvatar -> lobby -> countdown -> getReady (intro)
  -> [ topicVote -> waiting
       -> 4 x ( getReady -> answerChoice | answerSlider -> waiting -> result ) -> roundEnd ] x 4
  -> getReady (final intro) -> up to 12 x ( answerTrueFalse -> waiting -> raceResult )
  -> results -> poll -> pollDone -> (Play again: countdown -> topicVote)
countdown -> Cancel -> back to lobby or pollDone.
Back to lobby (TV settings panel) sends every phone to lobby from any screen.
settings and reconnecting can appear at any point and return to the current screen.
```
