# 15 Data schemas and sample content

Status: Done for game night 1 (Nov 2, 2026).

All content is JSON, UTF 8, in `content/`. The TV app reads it at startup from `unity/Assets/StreamingAssets/content/` (a build step copies `content/` there) and validates it; any error stops the game with a clear message naming the file, the item id and the field.

Every player facing string is an object with both languages: `{"ru": "...", "en": "..."}`. Both are required.

Check the content with `node tools/validate_content.mjs` after every edit. The TV app runs the same checks at startup.

## Files

| File | Holds |
| --- | --- |
| `content/config.json` | Every number (`03_rules.md`) |
| `content/topics.json` | Topic list |
| `content/questions.json` | Round questions (multiple choice, closest value) |
| `content/race.json` | True or false statements for the final race |
| `content/host_lines.json` | Host text lines and their triggers |
| `content/show_scripts.json` | The segments of each mode |
| `content/poll.json` | Poll questions |
| `unity/Assets/StreamingAssets/local.json` | Machine settings: `relayHost`. Not in git (`local.example.json` is) |

## topics.json

```json
[
  { "id": "geography", "title": { "ru": "География", "en": "Geography" } },
  { "id": "mixed", "title": { "ru": "Обо всём", "en": "A bit of everything" }, "mixed": true }
]
```

The topic with `"mixed": true` is the fourth door and draws from every topic.

## questions.json

A list of question objects. Fields common to all:

| Field | Type | Rule |
| --- | --- | --- |
| `id` | string | Unique, `topic_number`, for example `geo_007` |
| `topic` | string | A topic id |
| `format` | string | `multipleChoice` or `closestValue` |
| `difficulty` | string | `easy`, `medium` or `hard` |
| `text` | localized | The question, at most 120 characters per language |
| `fact` | localized, optional | One sentence shown or spoken after the reveal |
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
| `text` | Max 90 characters per language. May contain `{name}` (a player name) and `{leader}` |
| `weight` | Higher wins when several lines match; default 1 |

Triggers for game night 1: `gameStart`, `roundStart`, `everyoneCorrect`, `nobodyCorrect`, `fastAnswer` (correct in under 2 s, field `{name}`), `newLeader` (`{name}`), `streak3` (`{name}`), `powerUpUsed` (`{name}` is the attacker, `{target}` the target), `closestExact` (`{name}`), `lastPlaceCorrect` (`{name}`), `roundEnd`, `finalStart`, `raceWinner` (`{name}`), `lastCall`.

Selection: for a trigger, take the matching lines, pick by weight, and never repeat a line until all lines of that trigger were shown (shuffle bag per trigger, saved to disk across evenings). Respect `hosts.minGapSeconds` per host; drop the line rather than delay it.

## show_scripts.json

```json
{
  "standard": ["intro", "round", "round", "round", "round", "finalIntro", "finalRace", "results", "poll"],
  "quick": ["intro", "round", "round", "finalIntro", "finalRace", "results", "poll"],
  "long": ["intro", "round", "round", "round", "round", "round", "round", "finalIntro", "finalRace", "results", "poll"]
}
```

Mini games are added later as a `"miniGame"` segment.

## poll.json

See `20_playtest.md` for the questions. Kinds: `choice` (one of `options`), `players` (pick a player), `text` (free text, max 200 characters).

## Event log (telemetry)

One JSON object per line, written to `<Application.persistentDataPath>/playtests/<yyyy-mm-dd>/events_<session>.jsonl`.

```json
{"t":12345,"session":"2026-11-07T19-30-00","build":"0.1.0","seed":918273,"event":"phaseStarted","phase":"Answer","phaseId":16,"budgetMs":15000}
{"t":15800,"session":"2026-11-07T19-30-00","build":"0.1.0","event":"input","playerId":"p2","kind":"choice","phaseId":16,"ms":3410,"correct":true,"points":890}
```

| Field | Rule |
| --- | --- |
| `t` | Milliseconds since the session started, from the core clock |
| `session`, `build` | On every line |
| `seed` | On the first line of each game |
| `event` | One of: `sessionStarted`, `playerJoined`, `playerLeft`, `playerRejoined`, `gameStarted`, `phaseStarted`, `phaseEnded` (with `reason`: `timeout`, `allDone`, `skip`), `input`, `inputRefused`, `powerUpDrawn`, `powerUpUsed`, `effectCleared`, `scoreChanged`, `hostLine`, `raceStarted` (with `lastPlaceCanWin`), `gameEnded`, `poll`, `error` |

Derived numbers computed by `tools/analyze_log`: idle share (time when no phone accepts input divided by game time), longest idle stretch, phase durations, correct rate per question, power ups drawn versus used, who targets whom.
