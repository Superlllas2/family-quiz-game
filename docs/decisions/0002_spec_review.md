# Decision record 0002: spec review for game night 1

Date: Nov 2, 2026. Decided by Ilia after an AI review of every spec for days 1 to 4. The AI building the game never reopens one of these on its own; it asks first. D23 replaces the host phone part of D3 (the game language is now set in the TV settings panel). The avatar presets of D33 were later replaced by the character roster (D55 in `0005_staging_answers.md`).

## Decisions

| # | Question | Decision |
| --- | --- | --- |
| D21 | JSON library for the TV app | Newtonsoft Json.NET (`com.unity.nuget.newtonsoft-json`). Core may reference it; it is plain .NET |
| D22 | Rank when scores are tied | Shared rank (1, 1, 3). When exactly one player must be chosen among tied players, the seeded random generator picks |
| D23 | Host phone | Removed completely: no `host` message, no `isHost`, `canSkip`, `canContinue`, `canChoose`. Every phone shows Start in the Lobby (2 or more players) and Play again after the poll. Start runs a 3 s countdown on the TV and all phones; any phone can Cancel. Intro, FinalIntro and Results run on their timers with no skip. The poll ends when every connected phone submitted or after 90 s. New phone message `lobby` (start, cancelStart, playAgain) with an ack. Game settings move to a TV settings panel (Tab key, mouse): game language, power ups on or off and per type, targeting, reset seen questions, back to lobby. Phones keep language, more time and leave |
| D24 | Joining and seats | Join only in the Lobby; the roster is fixed during a game (`gameStarted`). A disconnected player can always rejoin their seat with their token while it is held. New `leave` message frees the seat at once; the player is out for the rest of the game. Play again keeps players and avatars and resets scores. Back to lobby keeps seats and avatars so others can join or leave |
| D25 | Seen questions | Every game marks its questions and race statements as seen. Reset in the TV settings panel |
| D26 | Relay lifecycle | TV control message `{"close": "<conn>", "code": 4002}`. Relay sends phones `tvAway` and `tvBack`; phones show reconnecting. Phone messages are dropped while the TV is away. Over the rate limit: drop, never close |
| D27 | Topic doors | Candidates are topics not used in this game. Prefer those with at least 4 unseen questions (any 4), choose 3 by seeded random. If fewer than 3 qualify, fill with unused topics with the most unseen questions, then with topics used earlier in this game. Mixed door is always door 4 |
| D28 | Timers | Personal deadline per player (window times multiplier); a later tap is `closed`. Last call sends a new screen with the same phaseId and a new `endsInMs`; the phone does not restart its `ms` clock. Reveal and Scores share one phaseId. The timer multiplier also applies to race questions |
| D29 | Vote tie break | Among the players who voted for one of the tied doors, the lowest ranked one decides; if tied in rank, seeded random. Same wording in 02, 03 and 18 |
| D30 | `ms` and sabotage | `ms` always starts when the options appear, sabotage or not. Sabotage costs time, which can cost some speed points (about 100), never points directly |
| D31 | Host line arbitration | At most one line per Reveal, by the host silent longer. Priority: lastPlaceCorrect, newLeader, streak3, closestExact, everyoneCorrect, nobodyCorrect, fastAnswer, powerUpUsed. fastAnswer only for the single fastest correct player. The Intro plays one gameStart line from each host |
| D32 | Bots | No bots in the game. Tests may run headless games with simulated players in test code only: fast 1 to 3 s, normal 3 to 8 s, slow 7 to 14 s (uniform), accuracy 0.9, 0.6 and 0.3, 2 of each. Testing alone uses several browser tabs on the laptop |
| D33 | Avatars and names | No emoji. 12 presets in `content/avatars.json` (id, color, shape), drawn as a colored shape with the first letter. Names allow Latin and Cyrillic letters, digits and spaces only (`badName` otherwise). Avatar picks get an ack; `avatarTaken` if just taken |
| D34 | Smaller defaults | Game language defaults to Russian. Poll answers: object from question id to a string (text) or option index (choice). An editor script copies `content/` into StreamingAssets before play and builds. TV UI: UGUI with TextMeshPro. Every event type has an example in 15 |
| D35 | Idle share | Not asserted for game night 1. Simulated games assert that no stretch without input lasts longer than 10 s outside Intro, FinalIntro and Results, and only report the idle share |
| D36 | Question fallback | An unseen question from any topic comes before a seen one from the same topic. The no repeat check runs over 2 games (the bank holds 2 games) |
| D37 | Power up toggles | Per type toggles live in the TV settings panel |
| D38 | Numbers in config | New keys: `scoring.fastAnswerMs` 2000, `net.inputGraceMs` 500, `net.tvReconnectSeconds` 2, `hosts.streakTrigger` 3, `lobby.startCountdownSeconds` 3, `poll.maxSeconds` 90. `configVersion` and `topics.mixedTopicId` added to the table in 03 |
| D39 | lastPlaceCanWin | True when (leader head start − last place head start) < `race.maxQuestions` and (`race.finishSteps` − last place head start) ≤ `race.maxQuestions` |
| D40 | Round count for other modes | Out of scope. Quick and Long need multipliers per round before use (note in 03) |
| D41 | Game length in 01 | Game night 1 games last about 10 to 12 minutes without mini games; 15 to 19 includes mini games later |

## Details added while applying these (not yet confirmed by Ilia)

Each fills a gap the decisions above left open. Change any of them by asking for it.

| Detail | Where |
| --- | --- |
| The countdown is its own phone screen `countdown` (data `text`, time in `endsInMs`); Play again uses the same countdown with Cancel | 02, 06, PROTOCOL |
| A PlayAgain phase follows the poll; `pollDone` carries `canPlayAgain`, and `lobby` carries `canStart` | 02, 06, PROTOCOL |
| `avatar` carries an `inputId` so it can be acked; on join the TV gives the first free avatar | 02, 06, PROTOCOL |
| Testing alone with tabs: a page opened with `&tab=<n>` stores its token under `fq.token.<room>.<n>`. Tabs in one browser share `localStorage`, so without this every tab would rejoin the same seat | PROTOCOL, 13, 18, prompts |
| A phone closed with 4002 shows "Game open in another tab" and does not reconnect; on `tvBack` a phone sends `rejoin`; after a TV reconnects the relay sends one `connected` per open phone socket; when a room closes the relay closes phones with 4004 | PROTOCOL |
| Name characters: A to Z, a to z, А to Я, а to я, Ё, ё, digits, space | PROTOCOL, 18 |
| A player out of the game is removed from the leaderboard, ranks and target lists | 02 |
| TV panel: language applies at once; power up and targeting changes from the next game; it never pauses the game; Back to lobby abandons a running game | 02 |
| Nobody voted: seeded random among the four doors. Slot fallback picks by format and difficulty. Race statements: unseen first, then seen | 02, 03, 18 |
| Race questions have no last call | 02, 03, 04 |
| Draw chance formula with shared ranks: `0.3 + 0.7 × (rank − 1) / (players − 1)`; head start uses `rankFromBottom = players − rank` | 03 |
| `powerUpUsed` plays only if no line played yet in that question cycle; bags are per trigger and host, falling back to the other host before refilling | 15 |
| Game length is measured from the end of the start countdown to the end of Results | 02, 19 |
| New log events `lobbyAction` and `settingsChanged`; `phaseEnded` reasons `cancelled`, `abandoned`, and `skip` only for the developer phase skip key | 15 |
| The `players` poll kind is removed (no poll question used it) | 15 |
| Protocol version stays 1: nothing was built yet when fields were removed | PROTOCOL |

## Open issue (resolved)

Resolved on Nov 2, 2026 by D42 in `0003_3d_and_idle_limit.md`: the limit is now 11 s.


D35's 10 s check fails with the current timers whenever nobody holds a power up. QuestionShow 4 s + Reveal 3.5 s + Scores 3.5 s = 11 s without input between two answer windows, and Reveal + Scores + RoundEnd = 11 s before each topic vote. Day 3 has no power ups at all, so its simulated game will fail this check. One timer must shrink by at least 1 s, or the threshold must rise to 11 s, before Day 3.
