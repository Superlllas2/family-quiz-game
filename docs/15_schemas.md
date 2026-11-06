# 15 Data schemas and sample content

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`; `playerJoined` trigger and `stagingExtraMs` from `decisions/0004_staging.md`; characters and `startCountdown` from `decisions/0005_staging_answers.md`).

All content is JSON, UTF 8, in `content/`. The TV app parses it with Newtonsoft Json.NET. It reads it at startup from `unity/Assets/StreamingAssets/content/`, where an editor script copies `content/` before Play mode and before every build (`13_architecture.md`; the copy is not in git). It validates everything; any error stops the game with a clear message naming the file, the item id and the field.

Every player facing string is an object with both languages: `{"ru": "...", "en": "..."}`. Both are required.

Check the content with `node tools/validate_content.mjs` after every edit. The TV app runs the same checks at startup.

## Files

| File | Holds |
| --- | --- |
| `content/config.json` | Every number (`03_rules.md`) |
| `content/topics.json` | Topic list |
| `content/questions.json` | Round questions (multiple choice, closest value) |
| `content/race.json` | True or false statements for the final race |
| `content/facts.json` | Interesting facts, the winner's reward on Results (D87) |
| `content/host_lines.json` | Host text lines and their triggers |
| `content/show_scripts.json` | The segments of each mode |
| `content/poll.json` | Poll questions |
| `content/characters.json` | The roster of guest characters players pick from |
| `content/ui_strings.json` | The TV app's own texts: TV labels and texts it sends to phones (D65) |
| `unity/Assets/StreamingAssets/local.json` | Machine settings: `relayHost`. Not in git (`local.example.json` is) |

## topics.json

```json
[
  { "id": "geography", "title": { "ru": "География", "en": "Geography" } },
  { "id": "mixed", "title": { "ru": "Обо всём", "en": "A bit of everything" }, "mixed": true }
]
```

The topic with `"mixed": true` (id in `topics.mixedTopicId`) is the fourth door and draws from every topic.

## questions.json

A list of question objects. Fields common to all:

| Field | Type | Rule |
| --- | --- | --- |
| `id` | string | Unique, `topic_number`, for example `geo_007` |
| `topic` | string | A topic id |
| `format` | string | `multipleChoice` or `closestValue` |
| `difficulty` | string | `easy`, `medium` or `hard` |
| `text` | localized | The question, at most 120 characters per language |
| `fact` | localized, optional | One sentence, at most 160 characters: the back of the question card after the reveal and in Scores (D104) |
| `source` | string | Where the fact was checked: a Wikidata id or a URL |
| `kids` | bool, optional | true if suitable for young children; default true |

Multiple choice adds:

| Field | Type | Rule |
| --- | --- | --- |
| `options` | list of 4 localized | At most 40 characters each |
| `correct` | int | Index of the right option in `options` (0 to 3). Display order is shuffled at runtime |

Closest value adds:

| Field | Type | Rule |
| --- | --- | --- |
| `answer` | number | The true value |
| `range` | [min, max] | Slider limits; the answer must lie inside, not at either end |
| `step` | number | Slider step |
| `tolerance` | number | Distance at which points reach zero |
| `unit` | localized | For example `{"ru": "год", "en": "year"}`; may be empty strings |

Example:

```json
[
  {
    "id": "geo_001",
    "topic": "geography",
    "format": "multipleChoice",
    "difficulty": "easy",
    "text": { "ru": "Какая река самая длинная в Европе?", "en": "Which is the longest river in Europe?" },
    "options": [
      { "ru": "Волга", "en": "Volga" },
      { "ru": "Дунай", "en": "Danube" },
      { "ru": "Рейн", "en": "Rhine" },
      { "ru": "Днепр", "en": "Dnieper" }
    ],
    "correct": 0,
    "source": "https://www.wikidata.org/wiki/Q626"
  },
  {
    "id": "geo_005",
    "topic": "geography",
    "format": "closestValue",
    "difficulty": "hard",
    "text": { "ru": "Какова высота Эвереста в метрах?", "en": "How tall is Mount Everest in metres?" },
    "answer": 8849,
    "range": [6000, 11000],
    "step": 10,
    "tolerance": 1500,
    "unit": { "ru": "м", "en": "m" },
    "source": "https://www.wikidata.org/wiki/Q513"
  }
]
```

## race.json

```json
[
  {
    "id": "race_001",
    "statement": { "ru": "У осьминога три сердца.", "en": "An octopus has three hearts." },
    "answer": true,
    "source": "https://en.wikipedia.org/wiki/Octopus"
  }
]
```

## facts.json

The winner's reward on Results (D87): one fact, shown on the winner's phone only. At least one fact; `text` at most 160 characters per language; `source` names where the fact can be checked. The fact ids given to winners are kept in the TV app's seen file (`<persistentDataPath>/seen.json`, `"facts"` next to `"questions"` and `"race"`), so a fact repeats only once every fact was given; "Reset seen questions" clears them too.

```json
[
  {
    "id": "f001",
    "text": { "ru": "У осьминога три сердца и голубая кровь.", "en": "An octopus has three hearts and blue blood." },
    "source": "Britannica, Octopus"
  }
]
```

## characters.json

The guest characters players pick from (`21_staging.md`): personas, not people, with no real names, nationality or age (D60). The roster is a draft until Ilia approves it.

```json
[
  {
    "id": 0,
    "name": {
      "ru": "Робот",
      "en": "The Robot"
    },
    "personality": {
      "ru": "Работает на фактах. Глючит, когда ошибается.",
      "en": "Runs on facts. Glitches when it's wrong."
    },
    "look": "Boxy retro robot with a matte painted metal shell and rounded corners, a screen for a face that shows simple expressions, an antenna with a bulb, dials on the chest, stubby legs and long arms",
    "color": "#5B9BD5",
    "entrance": "Rolls in on hidden wheels, unfolds into the seat with whirs and clicks",
    "idlePose": "Screen face blinks, the antenna bulb pulses, small whirring calculations",
    "happyReaction": "Antenna lights up, a stiff robot dance with both arms",
    "sadReaction": "Screen shows an error, a few sparks, powers down and slumps, then reboots",
    "cardHold": "The card pops out of a slot in its chest; it holds it up on a telescoping arm",
    "victory": "Spins its head all the way round and fires a confetti cannon from its chest"
  }
]
```

| Field | Rule |
| --- | --- |
| `id` | Integer; the ids are 0, 1, 2, ... in file order. At least `players.max` characters |
| `name` | Localized, at most 16 characters per language, unique |
| `personality` | Localized, one line, at most 60 characters; shown on the `pickCharacter` card |
| `look` | English, for artists: age, build, clothes, props |
| `color` | Signature color, `#RRGGBB`, unique, with a contrast ratio of at least 4.5 against `#14161C` (the chip text color) |
| `entrance`, `idlePose`, `happyReaction`, `sadReaction`, `cardHold`, `victory` | English, for animators: the six signature animations, each played the way the persona would |

Until the 3D characters exist, the TV and the phone draw a player's character as a chip: a rounded card in `color` with the player's name in `#14161C`. The `pickCharacter` card also shows the character's name and personality line. In the grey box (`21_staging.md`) a character is a capsule in `color` with its name above it. No emoji. The colors are placeholders until the visual style guide exists.

## ui_strings.json

Every text the TV app shows or sends to a phone that is not question, host line or other content (D65). One object from key to a localized string:

```json
{
  "tv.lobby.roomCode": { "ru": "Код комнаты", "en": "Room code" },
  "screen.countdown.text": { "ru": "Игра начинается", "en": "Starting" },
  "tv.results.winner": { "ru": "Победитель: {name}", "en": "Winner: {name}" }
}
```

| Rule | |
| --- | --- |
| Key | Dotted lower camel case. `tv.*` for text drawn on the TV (in the game language), `screen.*` for text sent to a phone (in that player's language) |
| Value | Localized, both languages non empty |
| Placeholders | `{name}` style, the same set in `ru` and `en` |
| Adding keys | In the same commit as the code that uses them. Code never contains a player facing text |

Texts the phone page draws on its own (buttons, reconnecting, the messages for `refused` reasons and close code 4002) live in the phone locale files (`06_phone_screens.md`).

## host_lines.json

```json
[
  {
    "id": "k_allwrong_01",
    "host": "konstantin",
    "trigger": "nobodyCorrect",
    "text": { "ru": "Никто? Серьёзно? Я в вас верил.", "en": "Nobody? Really? I believed in you." },
    "weight": 1
  }
]
```

| Field | Rule |
| --- | --- |
| `host` | `konstantin` or `ilja` |
| `trigger` | One of the triggers below |
| `text` | Max 90 characters per language. May contain `{name}` (a player name) and `{target}` |
| `weight` | Relative chance among the lines still in the bag; default 1 |

### Triggers for game night 1

"Correct" on a closest value question means it scored above 0. When several players fit `{name}`, one is picked with the seeded random generator.

| Trigger | Fires when | Fills |
| --- | --- | --- |
| `playerJoined` | Lobby: a new player joins. Not for a rejoin, and not for players kept after a game or Back to lobby | `{name}` |
| `startCountdown` | StartCountdown begins, when every connected player is ready | |
| `gameStart` | Intro starts | |
| `roundStart` | TopicVote starts | |
| `everyoneCorrect` | Reveal: every connected player answered correctly | |
| `nobodyCorrect` | Reveal: nobody answered correctly (including nobody answering) | |
| `fastAnswer` | Reveal: the single fastest correct player, if their `ms` is below `scoring.fastAnswerMs` (2000) | `{name}` |
| `newLeader` | Reveal: a player has rank 1 who did not have it before the question | `{name}` |
| `streak3` | Reveal: a player's streak reaches `hosts.streakTrigger` (3) | `{name}` |
| `powerUpUsed` | A player uses a power up (Reveal or Scores) | `{name}` attacker, `{target}` target |
| `closestExact` | Reveal: a closest value guess equals the answer | `{name}` |
| `lastPlaceCorrect` | Reveal: a player in last place before the question answered correctly | `{name}` |
| `roundEnd` | RoundEnd starts | |
| `finalStart` | FinalIntro starts | |
| `raceWinner` | Results starts | `{name}` |
| `lastCall` | The last call cut starts | |

### Selection

1. **One line per question cycle.** The triggers that fire at a Reveal compete in this priority: `lastPlaceCorrect`, `newLeader`, `streak3`, `closestExact`, `everyoneCorrect`, `nobodyCorrect`, `fastAnswer`, `powerUpUsed`. The highest one that fired plays; the others are dropped. A `powerUpUsed` during Reveal or Scores plays only if no line has played in that cycle yet.
2. **Other triggers** play one line each.
3. **Which host:** the host who has been silent longer speaks. The line comes from that host's lines for the trigger that have not been shown yet (shuffle bag per trigger and host, saved to disk across evenings), drawn by weight. If that host has none left but the other host has, the other host speaks. When both are used up, both bags refill.
4. **Gap:** a host speaks only after `hosts.minGapSeconds` of silence. If the chosen host is still inside the gap, the line is dropped, never delayed. A bubble stays for `hosts.bubbleSeconds`; at most one bubble per host at a time.
5. **Intro exception:** the Intro plays one `gameStart` line from each host, one after the other: the second starts when the first bubble ends.

`playerJoined` follows the normal gap rule, so when players join within a few seconds of each other some are not greeted. It shares its name with the `playerJoined` log event; the `hostLine` event's `trigger` field holds the trigger.

`startCountdown` plays one line when the countdown begins. The host who says it is still inside the gap when the Intro starts 3 s later, so the other host says the first `gameStart` line; the Intro rule is otherwise unchanged.

Lines show in the game language, with `{name}` and `{target}` filled in.

## show_scripts.json

```json
{
  "standard": ["intro", "round", "round", "round", "round", "finalIntro", "finalRace", "results", "poll"],
  "quick": ["intro", "round", "round", "finalIntro", "finalRace", "results", "poll"],
  "long": ["intro", "round", "round", "round", "round", "round", "round", "finalIntro", "finalRace", "results", "poll"]
}
```

Mini games are added later as a `"miniGame"` segment. The next game from the Lobby starts at the first `round` (no intro).

## poll.json

See `20_playtest.md` for the questions. Kinds: `choice` (one of `options`) and `text` (free text, max 200 characters). Answers arrive as an object from question id to a string (text) or an option index (choice), see `PROTOCOL.md`.

## Event log (telemetry)

One JSON object per line, written to `<Application.persistentDataPath>/playtests/<yyyy-mm-dd>/events_<session>.jsonl`.

| Field | Rule |
| --- | --- |
| `t` | Milliseconds since the session started, from the core clock |
| `session`, `build` | On every line |
| `seed` | On the `gameStarted` line of each game |
| `event` | One of the event types below |

Every event type, with its fields. `session` and `build` are shortened to `"..."` here; real lines carry them in full.

```json
{"t":0,"session":"2026-11-07T19-30-00","build":"0.1.0","event":"sessionStarted","room":"KXQT","configVersion":1}
{"t":5210,"session":"...","build":"...","event":"playerJoined","playerId":"p1","name":"Mama","lang":"ru","characterId":0}
{"t":9100,"session":"...","build":"...","event":"playerLeft","playerId":"p3","reason":"disconnected"}
{"t":12000,"session":"...","build":"...","event":"playerRejoined","playerId":"p3","awayMs":2900}
{"t":20000,"session":"...","build":"...","event":"lobbyAction","playerId":"p2","action":"ready","ok":true}
{"t":20500,"session":"...","build":"...","event":"settingsChanged","source":"tv","setting":"targeting","value":"aimUp"}
{"t":23000,"session":"...","build":"...","seed":918273,"event":"gameStarted","game":1,"mode":"standard","players":["p1","p2","p3"],"settings":{"gameLang":"ru","powerUps":["freeze","slime","fog"],"targeting":"free"}}
{"t":40000,"session":"...","build":"...","event":"phaseStarted","phase":"Answer","phaseId":16,"round":1,"slot":2,"questionId":"geo_003","budgetMs":15000,"stagingExtraMs":0}
{"t":49800,"session":"...","build":"...","event":"phaseEnded","phase":"Answer","phaseId":16,"reason":"allDone","durationMs":9800}
{"t":43410,"session":"...","build":"...","event":"input","playerId":"p2","kind":"choice","phaseId":16,"ms":3410,"correct":true,"points":890}
{"t":56100,"session":"...","build":"...","event":"inputRefused","playerId":"p4","kind":"choice","phaseId":16,"reason":"closed"}
{"t":50200,"session":"...","build":"...","event":"powerUpDrawn","playerId":"p4","powerUp":"slime","rank":4,"chance":1.0}
{"t":51000,"session":"...","build":"...","event":"powerUpUsed","playerId":"p4","powerUp":"slime","target":"p1","phaseId":17}
{"t":62650,"session":"...","build":"...","event":"effectCleared","playerId":"p1","effect":"slime","ms":2650}
{"t":49900,"session":"...","build":"...","event":"scoreChanged","playerId":"p2","delta":890,"score":2350,"rank":1}
{"t":49950,"session":"...","build":"...","event":"hostLine","lineId":"k_newLeader_01","host":"konstantin","trigger":"newLeader"}
{"t":520000,"session":"...","build":"...","event":"raceStarted","headStarts":{"p1":3,"p2":1,"p3":0},"lastPlaceCanWin":true}
{"t":640000,"session":"...","build":"...","event":"gameEnded","game":1,"reason":"finished","winner":"p2","durationMs":617000,"factId":"f014","tags":{"p1":"slowest","p3":"darkHorse"}}
{"t":700000,"session":"...","build":"...","event":"poll","answers":{"fun":"The freeze on Dad","waiting":1,"time":1,"again":0}}
{"t":700500,"session":"...","build":"...","event":"error","where":"content","message":"questions.json geo_009: bad difficulty"}
```

Allowed values:

| Field | Values |
| --- | --- |
| `playerLeft.reason` | `disconnected` (seat still held), `left` (tapped Leave), `seatExpired` |
| `lobbyAction.action` | `ready`, `notReady` |
| `settingsChanged.source` | `tv` (settings panel: `gameLang`, `powerUps`, `powerUpTypes`, `targeting`, `feedback`, `resetSeen`, `backToLobby`; pause menu: `newGame`; bank chooser: `bank`) or `phone` (with `playerId`: `lang`, `timerMultiplier`) |
| `phaseStarted.budgetMs`, `stagingExtraMs` | The phase budget in ms with any staging extra included, and that extra alone (`staging.extraSeconds`, `21_staging.md`); 0 for phases without one. Answer budgets are the base window, before timer multipliers |
| `phaseEnded.reason` | `timeout`, `allDone`, `cancelled` (countdown), `abandoned` (Back to lobby), `skip` (developer phase skip key only) |
| `gameEnded.factId`, `tags` | Only with reason `finished`: the winner's fact id (or null) and playerId to funny tag id for every other player (D86, D87) |
| `poll` | Never carries a player id |
| `questionRated` | One line per accepted tap on the rating card (decisions/0018, D111): `playerId`, `questionId`, `topic`, `format`, `difficulty`, `item` (`fun`, `guess`, `fact`), `stars` (1 to 5), `phaseId`, `correct` (true, false, or null when the player sent no answer). The latest line per player, question and item counts |
| `gameStarted.settings.bank`, `questionRated.bank` | The question bank of the game (decisions/0020): `full` or `curated` |
| `gamePaused`, `gameResumed` | The pause menu opened or closed (D105): `phase`, `phaseId`. `t` runs on the core clock, which stands still while paused |
| `gameEnded.reason` | `finished`, `backToLobby`, or `restart` (New game in the pause menu, D107) |

### Derived numbers

Computed by `tools/analyze_log`:

- **Idle:** a moment is idle when no connected phone can send an answer, vote, power up pick or poll input. Idle share = idle time divided by game time (from `gameStarted` to `gameEnded`). Reported only, no pass or fail in game night 1.
- **Longest idle stretch** outside Intro, FinalIntro and Results. Starting limit 11 s (`19_acceptance.md`); a suggested starting value like every timer (D56).
- Phase durations (real, from `phaseEnded.durationMs`, next to the budget and its staging extra), correct rate per question, power ups drawn versus used, who targets whom.
