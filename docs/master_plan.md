# Family Quiz master plan

Snapshot from Oct 31, 2026 of the live doc at https://claude.ai/artifact/JYidxGoa7YUmN6fV3Cc5Wm. The live doc is the source of truth: decisions are tracked there, and it holds the two diagrams.

## How this plan works

We plan in five layers from top to bottom, and code starts only once the first three are written down.

1. **Why: the pillars.** Five rules from the research that every feature must pass.
2. **What: the show.** What players experience, with a time budget for every phase.
3. **How: the architecture.** Three programs, the layers inside the TV app, and strict rules for what may talk to what.
4. **When: the build order.** A walking skeleton first (the thinnest game that runs end to end), then one feature at a time, each with an exit test.
5. **Proof: measurement.** Logs and phone polls that show whether a change made the game more fun.

The research enters the game through one rule: **every finding becomes a number in a config file, a system that owns it, and a check that proves it.** For example, "end the question when everyone has answered" becomes the setting `endWhenAllAnswered`, lives in the Show Director, and a bot simulation fails if any question keeps waiting after the last answer. The research to code table further down does this for every finding.

## Five pillars every feature must pass

A feature that breaks a pillar gets redesigned or cut, however fun it sounds. The numbers come from the research report in this Project, where each is marked as sourced or as a first playtest setting.

| Pillar | What it means in the game | How we know it holds |
| --- | --- | --- |
| **Never wait** | Things happen at the same time: power ups land while people answer, hosts talk over the action, a question ends when the last player answers | Under 20% of game time with no phone accepting input; no gap over 10 s outside the intro and the final result |
| **Everyone can win** | Speed matters only a little, attacks only hit players ranked above you, kids get help on their own phone, last place starts the final race close behind | In every logged game, last place can still win when the final starts |
| **Every tap feels good** | The phone reacts on touch, celebrations grow with what the player actually did, big effects only for round wins and the final | Tap feedback within 0.1 s; nobody asks "did it register?" in playtests |
| **Hosts react, never block** | Lines of 1 to 4 s chosen after the outcome, shuffled so none repeats, cut the moment the game moves on | No phase ever extended by a host line; no repeated line in an evening |
| **Fun is measured** | Every build logs events, and every game night ends with an anonymous phone poll | "Play again" presses rise week over week; people ask for one more game unprompted |

## The show

A Standard game runs 15 to 19 minutes: an intro, two host parts of two rounds each, a mini game after every round but the last, and a final race. Each host owns one part, so a third friend later simply adds a third part.

| Step | Phase | Host | TV | Phones | Budget |
| --- | --- | --- | --- | --- | --- |
| 1 | Lobby | Both, light chatter | QR code, players arriving | Join, name, avatar, ready | Until the first player who joined presses start |
| 2 | Intro | Both | One short double act bit, skippable | One tap prediction: who wins tonight? | 45 to 60 s |
| 3 | Round 1 | Host A | Topic doors, 4 questions, reveals | Vote, answer, pick power ups | 2 to 2.5 min |
| 4 | Mini game | Both | Live race or board | Everyone plays at once | 60 to 90 s |
| 5 | Round 2 | Host A | Same as round 1 | Same as round 1 | 2 to 2.5 min |
| 6 | Mini game | Both, handover from A to B | Live race or board | Everyone plays at once | 60 to 90 s |
| 7 | Round 3 | Host B | Same, points worth 1.5x | Same as round 1 | 2 to 2.5 min |
| 8 | Mini game | Both | Live race or board | Everyone plays at once | 60 to 90 s |
| 9 | Round 4 | Host B | Same, points worth 2x | Same as round 1 | 2 to 2.5 min |
| 10 | Final race | Both | Track with capped head starts and a chasing threat | Rapid answers; caught players play on as ghosts | 2 to 3 min |
| 11 | Results | Both | Winner, awards, play again | Personal stats, then the 3 minute poll | About 30 s plus the poll |

Inside every round:

1. **Topic vote** from four doors: 8 to 10 s, ties broken automatically. One door is always a broad family topic, and the trailing player gets a veto.
2. **Four questions of about 25 s each:** shown and read 3 to 5 s, answers up to 15 s (ending the moment everyone locks in, with a 5 s last call), reveal 3 to 4 s, then scores and power up picks together in 3 to 5 s.
3. **Difficulty mix:** 1 easy opener, 2 medium, 1 hard, with the hard one in a low guess format (closest number, ordering or typed).
4. **One special format per round,** never the same special format in back to back rounds.

| Mode | Contents | Length |
| --- | --- | --- |
| Quick | 2 rounds (one per host), 1 mini game, final race | 9 to 11 min |
| Standard, the default | 4 rounds, 3 mini games, final race | 15 to 19 min |
| Long | 6 rounds, 5 mini games, final race | 21 to 27 min |

All of this is data: a show script file lists the segments of each mode, so a third host or a new mode means editing a file, not code. Every time here is a first playtest setting from the research, to be tuned with real logs.

## Design documents to write before any code

Ten short documents, one or two pages each, written in this order because each leans on the one above. Claude drafts each one from the research and your answers, you correct it, and it lives in this Project.

1. **Vision and pillars:** who plays, where, for how long, and the five pillars. One page.
2. **Show flow:** the phase list, the time budgets, the three modes, and what the phones do in every phase.
3. **Rules and numbers:** scoring curve, round multipliers, power ups with their effects and caps, final race seeding. Every number sits in one table, which later becomes the config file.
4. **Question format catalog:** one card per format with its TV view, phone view, timer, scoring and the fields a question writer fills in.
5. **Phone screens list:** every screen the phone can show, each with its one decision.
6. **Mini game catalog:** one card per mini game with its input, length and how it pays out power ups.
7. **Host bibles and the line list:** one page per host (want, flaw, catchphrase, attitude to the other host, limits) plus every game moment that can trigger a line.
8. **Content guidelines:** the question checklist, the difficulty mix, family mode and photo consent rules.
9. **Settings and accessibility:** every toggle and slider with its default.
10. **Playtest plan:** what we log, the observation sheet, the poll and the decision rules.

Then comes the **paper prototype evening**: one Standard game on paper with the family, using question cards, four topic cards, power up cards, a whiteboard for scores, you reading host lines, and a stopwatch on every phase. It costs one evening and tests the show flow and the budgets before a single line of code exists. The architecture documents (overview, protocol, decision records) are written alongside, as described below.

## Architecture: three programs, one owner

The TV app owns every rule and every second, while the phones and the relay are thin pipes, so all game logic lives in one place, in C#.

Diagram in the live doc, in words:

- **Phone page, one per player** (web page, no app, join by QR code; shows only the screen it is sent; sends taps with on phone timing) talks over WebSocket to the **relay in the cloud over HTTPS** (serves the phone page, forwards messages inside one room, knows nothing about quizzes).
- The relay talks over WebSocket to the **network adapter** inside the **TV app** (Unity on the laptop, shown over HDMI). The adapter turns phone messages into commands and builds the screen for each phone from events.
- **Content and config** (questions, host lines, show scripts, every timer and score value) are read by the core at start.
- The **core** (plain C#, no Unity, no network, fully testable) holds the Show Director (runs the phases on its own clock and ends them early when everyone is done), the rules (scoring, power up targeting and caps, the final race, the question picker) and the game state (only commands can change it).
- The core sends events only to four listeners that never change the game: **Presentation** (TV views and hosts, fit phase budgets), **Dialogue Director** (picks host lines after each outcome), **Audio** (music and effects, ducked under voices), **Telemetry** (one JSON line for every event).

Read it top down: a tap travels from a phone through the relay into the TV app, becomes a command for the core, and the core's events fan out to everything that draws, speaks, plays sound or logs.

- **TV app:** Unity 6.3 LTS, the newest long term support version ([Unity](https://unity.com/ja/releases/unity-6/support)). It runs on the laptop, shows the game on the TV over HDMI, and holds the only copy of the game state.
- **Phone page:** a TypeScript web page with nothing to install. It draws one screen type at a time (vote, multiple choice, order, number, power up pick, mini game, poll) and sends back taps.
- **Relay:** a tiny server that hosts the phone page over HTTPS and forwards messages per room. HTTPS is what lets phones stay awake and join from any WiFi. Cloudflare Durable Objects fit well, one object per room: they run on the free Workers plan with 100,000 requests a day, and 20 incoming WebSocket messages count as one request ([Cloudflare](https://developers.cloudflare.com/durable-objects/platform/pricing)). A game night of a few thousand taps stays far below that. A small rented server with automatic HTTPS works too.
- **Why the relay sits in the cloud:** joining works on any network, including guest WiFi that hides devices from each other, and relatives in another city can join while watching the TV through a video call. The cost is needing internet, so a home WiFi mode stays possible later behind the same transport interface.
- **Why not Unity's multiplayer tools:** Netcode for GameObjects, Mirror and Photon connect Unity to Unity. Our players are browsers, so a plain WebSocket protocol is simpler.
- **Inside the TV app:** the core, the network adapter, content and config, and four listeners: presentation, the Dialogue Director, audio and telemetry.

## How the layers connect

Dependencies point one way: everything may use the core, and the core uses nothing, so every rule can be tested without Unity, a network or a phone.

### Dependency rules

- **The core is plain C#:** no UnityEngine, no network, no audio. Its assembly definition has "No Engine References" switched on, so a Unity call inside the core fails to compile.
- **Commands in, events out.** The only way to change the game is a command such as `SubmitAnswer`, `VoteTopic`, `UsePowerUp` or `Skip`. The only way to learn what happened is an event such as `PhaseStarted`, `AnswerLocked`, `Revealed`, `ScoreChanged` or `PowerUpApplied`.
- **Listeners only listen.** Presentation, the Dialogue Director, audio and telemetry react to events. None of them can change the state or delay a phase.
- **Phones are thin.** They draw the screen the TV app sends and send back taps. No game rule lives on a phone, so a new question format needs only a new screen type there.
- **The relay is dumb.** It forwards messages inside a room and knows nothing about quizzes.
- **Content and config are data files** the core reads at startup, so timers, scores and lines can be tuned between game nights without touching code.

### The time model

- The core owns the clock. Each phase takes its budget from config and announces `PhaseStarted(phase, endsAt)`.
- Presentation fits its animation inside that window. If it cannot, the animation is cut, never the game.
- The core ends a phase early when its condition is met: everyone answered, or everyone voted.
- The clock and the random numbers are injected, so a test can run a game at 100x speed and a saved seed replays any game exactly.
- Network messages arrive on a background thread. They wait in a queue and are handled once per frame on Unity's main thread.

### The journey of one answer

1. The player taps B. The phone highlights B at once, with no network involved, and notes the milliseconds since the options appeared.
2. The phone sends `{"type": "input", "q": 12, "choice": "B", "ms": 3410}` to the relay, which forwards it to the TV app in that room.
3. The network adapter queues it, and the main loop turns it into a `SubmitAnswer` command.
4. The core checks it (right question, still open, plausible time), stores it and emits `AnswerLocked`.
5. Listeners react: the avatar on the TV pops with a tick, telemetry writes a line, and the adapter sends "Locked: B" back to that phone.
6. When the last answer arrives, the core ends the answer phase and emits `RevealStarted`, and the Dialogue Director picks a line for what actually happened.

### The message protocol

| Message | Direction | Purpose |
| --- | --- | --- |
| `join` | Phone to TV app | Room code, name and avatar; the reply carries a rejoin token |
| `rejoin` | Phone to TV app | The stored token after a screen lock or reload |
| `snapshot` | TV app to phone | Everything the phone needs to draw its current screen, after a join or rejoin |
| `screen` | TV app to phone | What to show now: a screen type plus its data |
| `input` | Phone to TV app | A tap or a value, with the time measured on the phone |
| `ack` | TV app to phone | Input accepted or refused, so the phone can show its check mark |
| `ping`, `pong` | Both ways | Keeps the connection alive and estimates the clock offset |

Every message passes through the relay and carries a protocol version. `PROTOCOL.md` is the single source of truth that both sides are checked against.

## How we make sure the architecture is right

We prove the risky parts first with throwaway spikes, then lock the rules in with compiler checks, tests and bot simulations, so a mistake shows up on the day it is made.

### Risk spikes in the first week

Small throwaway experiments, each answering one question before the real build starts:

- Does a phone join by QR over HTTPS through the relay, and do its taps reach Unity?
- Does an iPhone that was locked for 60 s rejoin with its token and get its screen back?
- Does Screen Wake Lock keep an iPhone and an Android phone awake through a whole game?
- Do Cyrillic and Latin text render crisply on the TV with TextMeshPro, if we go bilingual?
- How much delay does your TV add over HDMI? Film a phone tap and the TV together in slow motion; the research found no data on this.

Each spike ends with a one page decision record: what we tried, what we chose and why.

### Rules the tools enforce

- **Assembly definitions:** `Core` has no engine references, and `Presentation` may reference `Core` but never the reverse. A wrong dependency fails to compile.
- **Config validation at startup:** a missing timer or a broken question file stops the game with a clear message instead of failing mid show.
- **`CLAUDE.md` in the repo** states these rules, so Claude Code follows them in every session. After each milestone, ask Claude to review the code against them.

### Tests

- **Unit tests** with the Unity Test Framework for scoring, power up targeting and caps, final race seeding and line selection.
- **Bot simulations:** headless games with simulated players of different speed and skill, 1,000 at a time. The run fails if a Standard game falls outside 15 to 19 minutes, if idle time passes 20%, or if last place has no chance when the final starts.
- **Replays:** every playtest saves its seed and its command log, so a bug from Saturday's game night replays exactly on Sunday.

### Tools that let you test alone

- Bots that fill empty seats, so one person can play a full game.
- A debug overlay with the current phase, its budget and the live idle share.
- Keys to skip a phase or jump straight to the final race.

## From research to code

Each research finding has one home in the code and one check, so a playtest problem always points at the setting to change.

| Research finding | Where it lives | How we check it |
| --- | --- | --- |
| Question cycle of about 25 s | Show Director, `timers` in config | Phase durations in the log |
| End the question when the last player answers, with a 5 s last call | Show Director, `endWhenAllAnswered`, `lastCallSeconds` | Bot simulation: no wait after the last answer |
| Timers 1.5 to 2 times longer for kids or relaxed play | Settings, `timerMultiplier` per player | Correct rate and idle time by age in the log |
| Speed counts, but a correct answer at the buzzer still earns 500 of 1,000 points | Scoring, `speedCurve` | Unit test |
| Closest number answers scored by distance | The closest number format | Unit test |
| Power ups earned by correct answers, weighted toward trailing players | Power up dealer | Bot simulation: draws by rank |
| Aim up: you may only target players ranked above you | Power up targeting rule | Unit test |
| One hit per target per question, then one question of immunity | Power up rules, `maxHits`, `immunityQuestions` | Unit test; who targets whom in the log |
| Effects cost the target 2 to 4 s, never points | Power up definitions | Poll: most frustrating moment |
| Final race: head start capped at 3 steps, leader handicap, ghosts play on | Final race rules | Bot simulation: last place can still win |
| Reveal holds of 1 to 2 s, 3 to 5 s at round end, at most 8 s at the final | Presentation timings in config | Debug overlay and log |
| Host lines of 1 to 4 s that never hold up a phase and stop when it ends | Dialogue Director | Bot simulation: no phase extended by a line |
| No line repeats until its pool runs out, across evenings | Shuffle bags saved to disk | Unit test; line log |
| Tease the play, not the person, and tease the leader most | Line tags and the writing guide | Content review checklist |
| Tap feedback within 0.1 s | Phone page, highlight on touch | Slow motion video test |
| Keep phones awake, which needs HTTPS | Relay over HTTPS, Wake Lock on the phone | Spike; disconnects in the log |
| Rejoin after a screen lock without losing the seat | Protocol `rejoin`, seat held 5 minutes | Spike; reconnect test |
| Flag questions under 35% correct or at 100% | Content pipeline | Correct rate per question in the log |
| Under 20% of game time with no phone accepting input | Telemetry | Weekly metrics after each game night |

When a game night shows a problem, find its row, change the setting, and rerun the check before the next game night.

## Build order

Ten milestones, each closed by a gate that must pass before the next starts. The game first runs end to end at M2, and everything after it adds one tested layer.

| Milestone | What it builds | Gate to pass before moving on |
| --- | --- | --- |
| M0 Plan on paper | Ten design docs, then a paper game night with a stopwatch | One paper game played and its phase times written down |
| M1 Risk spikes | QR join, iPhone rejoin, Wake Lock, Cyrillic text, TV lag | Decision records for transport, phone library and content files |
| **M2 Walking skeleton** | Lobby, 4 questions, scores, play again, bots, event logger | 3 phones and 3 bots finish a game with no crash |
| M3 Full show in grey boxes | All phases, topic vote, 3 formats, final race, TTS hosts | Simulated Standard game lasts 15 to 19 min, idle under 20% |
| M4 First family game night | Real phones, the event log and the poll, then a weekly loop | Top 3 problems fixed before any new feature |
| M5 Power ups and mini games | Aim up targeting, hit caps, 2 or 3 mini games, Family and Chaos | Each kept mini game wins mostly Yes replay votes |
| M6 Hosts | Host bibles, Dialogue Director, Wizard of Oz night, recording day | Only lines that earned laughs get recorded |
| M7 Question bank | Question sheet, AI drafting with checks, family photo mode | About 500 checked questions, no flagged one left unreviewed |
| M8 Look and sound | 2D host portraits, juice pass, music and effects, accessibility | Tap feedback within 0.1 s, no confusion in observations |
| M9 Ready for any living room | Reconnects, installer, settings, both languages, crash logs | 3 game nights in a row with no restart |
| Later | 3D hosts, a third host, home WiFi mode, selling | |

M0 and M1 can overlap, and M7 (the question bank) can run alongside M5 and M6. How long each takes depends on your weekly hours, so the plan tracks gates, not dates. A first bank of about 500 checked questions covers roughly 6 game nights of 3 Standard games without repeats.

## Repo layout and the working loop

One Git repository holds all three programs, the content and the docs, so Claude Code always sees the whole picture.

```
FamilyQuiz/
  CLAUDE.md            rules every Claude Code session follows
  docs/                design docs, PROTOCOL.md, decisions/
  unity/               the TV app (Unity 6.3 LTS)
    Assets/Game/Core/           plain C#, no engine references
    Assets/Game/Net/            relay connection, phone screens
    Assets/Game/Presentation/   TV views, hosts, juice
    Assets/Game/Dialogue/       line rules, shuffle bags
    Assets/Game/Audio/
    Assets/Game/Telemetry/
    Assets/Game/DevTools/       bots, overlay, phase skips
    Assets/Tests/               unit tests and bot simulations
  phone/               the phone page (TypeScript)
  relay/               the room server
  content/             questions, host lines, show scripts, config
  tools/               content checks, AI drafting, log analysis
  playtests/           logs, polls and notes per game night
```

The loop for every feature:

1. Write or update its spec in `docs/`: the behavior, the numbers, the check.
2. Ask Claude Code to write the tests first, a unit test or a bot simulation, straight from the spec.
3. Ask it to build until the tests pass, inside the rules in `CLAUDE.md`.
4. Play it yourself with bots, then with the family on game night.
5. Log keep, change or drop, then update the spec and the config.

Housekeeping: Git LFS for audio and images, one branch per feature, and a commit after every green test run. Unity can run its tests from the command line, so Claude Code can run them itself after every change.

## Decisions for Ilia

Ten choices shape the skeleton, and each has a default we can start with. Status is tracked in the live doc.

| Decision | Options | Recommended default |
| --- | --- | --- |
| TV app engine | Unity, or a web page on the TV | Unity 6.3 LTS: your skills, and 3D hosts later |
| Languages | Russian, English, both | Both, with every text a localization key from day one |
| How phones connect | Cloud relay, home WiFi only, both | Cloud relay first, home WiFi mode later behind the same interface |
| Must it work without internet? | Yes, no | Decide before the spikes; a yes moves home WiFi mode up |
| Phone page library | Preact, Svelte, plain TypeScript | Preact: React style code that AI tools write fluently, at a tiny size |
| Players per game | 2 to 6, 2 to 8 | 2 to 8, tested at 3 and at 6 |
| First platform | macOS, Windows | macOS first for the MacBook, Windows when selling |
| Host look until the art milestone | Text only, 2D portraits, 3D | 2D portraits with a few expressions and mouth flaps |
| Host names and consent | Real names, stage names | Stage names, plus written consent before recording or cloning |
| Question base | Own questions, Wikidata facts, open databases | Own questions built on Wikidata facts, the cleanest licensing for a game you may sell |

## Sources

- The research report Making a party quiz game fun, in this Project: every timing, rule and threshold above
- [Unity 6 support page](https://unity.com/ja/releases/unity-6/support): Unity 6.3 is the latest LTS, supported until December 2027
- [Cloudflare Durable Objects pricing](https://developers.cloudflare.com/durable-objects/platform/pricing): free plan limits for the relay
