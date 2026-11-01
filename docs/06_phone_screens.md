# 06 Phone screens and user flow

Status: Done for game night 1 (Nov 2, 2026).

The phone page draws exactly one screen at a time, the one named in the last `screen` message from the TV app (`PROTOCOL.md`). It holds no game rules. Every screen has one decision at most.

## General rules for every screen

- Portrait only. Full height, no scrolling during play.
- Touch feedback on `pointerdown`, never on click: the pressed button changes at once (scale 0.95, brighter), with no wait for the network.
- After an input, show "Отправлено / Sent" with a small spinner, then a check mark when the `ack` arrives. No ack within 1 s: resend once; still nothing after 3 s: show "Нет связи / No connection".
- Top bar on every screen during a game: player name, avatar, score, and a gear icon for settings.
- Placeholder look: dark background `#14161C`, white text, answer colors A `#E74C3C` (triangle), B `#3498DB` (diamond), C `#F1C40F` (circle), D `#2ECC71` (square). These are placeholders until the visual style guide exists.
- Every visible text comes from the phone's locale file (`phone/src/locales/ru.json`, `en.json`) or from content in the player's chosen language.
- Request a Screen Wake Lock after the first tap, and again whenever the page becomes visible.
- No sounds on the phone.

## Screens

| Screen id | Shown during | Content | Decision | Next |
| --- | --- | --- | --- | --- |
| `join` | Opening the link or QR | Room code field (filled from the URL), name field (max 16 characters), language toggle RU or EN, Join button | Join | `pickAvatar` |
| `pickAvatar` | After joining | 12 preset avatars (emoji faces in colored circles for now); taken ones greyed | Pick one | `lobby` |
| `lobby` | Lobby | "Ты в игре! / You're in!", list of players. Host phone also sees: Start button (enabled at 2 or more players), game language RU or EN, power ups on or off, targeting free or aim up | Host: Start | `getReady` |
| `getReady` | Intro, FinalIntro, QuestionShow | A short text sent by the TV app (for QuestionShow: the question). In Intro: prediction "Кто победит? / Who wins tonight?" with player buttons. Host phone sees Skip in Intro and FinalIntro | Prediction tap, or host Skip | Next screen sent |
| `topicVote` | TopicVote | Four topic buttons | Vote | `waiting` |
| `answerChoice` | Answer, multiple choice | Four buttons 2x2, each a quarter of the space below the top bar | Tap one | `waiting` |
| `answerSlider` | Answer, closest value | Slider, big value, plus and minus, Lock | Lock | `waiting` |
| `answerTrueFalse` | RaceQuestion | Two buttons, top and bottom halves | Tap one | `waiting` |
| `sabotage` | Overlay on any answer screen when hit | Freeze: ice layer with a tap counter. Slime: 3 blobs to swipe off. Fog: blur layer that clears in 3 s or faster by rubbing. Text: "{name} заморозил тебя! / {name} froze you!" | Clear it | The answer screen below |
| `waiting` | After an input, until the phase ends | "Ответ принят / Locked: B" (or the value), avatars of who still has to answer | None | Next screen sent |
| `result` | Reveal and Scores | "Верно! +850 / Correct! +850" or "Мимо / Wrong", streak, rank. If the player holds a power up: its card, tap it, then a row of target buttons (greyed if hit or immune) | Use power up (optional) | Next screen sent |
| `roundEnd` | RoundEnd | Own rank and score, "Раунд 3 / Round 3 next" | None | Next screen sent |
| `raceResult` | Race reveal | "Верно, +1 шаг / Right, +1 step" or "Мимо / Missed", own position on a small track | None | Next screen sent |
| `results` | Results | Own place, correct answers, awards won. Host phone: Continue | Host: Continue | `poll` |
| `poll` | Poll | One question per page, seven pages (`20_playtest.md` lists them); Next and Submit | Answer | `pollDone` |
| `pollDone` | After submitting | "Спасибо! / Thanks!". Host phone: Play again, New players | Host choice | Next screen sent |
| `settings` | Gear icon, any time | Language RU or EN, "More time" 1x, 1.5x or 2x, Leave game | Change | Back to the screen before |
| `reconnecting` | Connection lost | "Переподключение… / Reconnecting…" and a spinner. Retries every 2 s | None | `snapshot` restores the right screen |
| `error` | Room not found, game full, name taken | Message and a Back button | Back | `join` |

## Flow

```
join -> pickAvatar -> lobby -> getReady (intro)
  -> [ topicVote -> waiting
       -> 4 x ( getReady -> answerChoice | answerSlider -> waiting -> result ) -> roundEnd ] x 4
  -> getReady (final intro) -> 12 x ( answerTrueFalse -> waiting -> raceResult )
  -> results -> poll -> pollDone -> (play again: topicVote | new players: lobby)
settings and reconnecting can appear at any point and return to the current screen.
```
