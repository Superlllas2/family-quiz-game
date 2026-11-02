# 19 Acceptance criteria: game night 1

Status: Done (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). Target: game night with friends on the weekend of Nov 7 and 8, 2026.

## Scope

**Must have:** relay, phone page, lobby with QR join, character pick from the roster, language per phone, Start and Play again from any phone with the start countdown, the TV settings panel, the Standard show (intro, 4 rounds, final race, results, poll), topic vote, multiple choice, closest value, scoring with speed curve and multipliers, 3 power ups with free targeting, final race with true or false, host text bubbles, debug overlay, event log, play again, reconnects and leave, macOS build.

**Stretch, only after every must have passes:** clue by clue format, one mini game, a Windows build, Russian text to speech for host lines, and the TV staging grey box (`21_staging.md`; only after every Day 4 check passes, checks below).

**Not in game night 1:** art, music, voices, the 3D set, characters and band (at most the grey box), Quick and Long modes as tested features, family photos, an installer, bots or computer players in the game.

## Checks

Each check is automated (a unit test or a simulated game in the test code) unless marked (manual). Simulated games use the six simulated players from `13_architecture.md` (2 fast, 2 normal, 2 slow), which exist only in tests. Manual checks with more players than phones use extra browser tabs on the laptop (`&tab=<n>`).

### Day 1: relay and join (Tue)

- [ ] A phone opens the QR link, joins with a name, and appears in the TV lobby within 1 s (manual)
- [ ] A tap on the phone arrives in Unity and the `ack` is back on the phone within 300 ms on home WiFi (manual, measured in the log)
- [ ] A phone locked for 60 s gets its seat and its screen back after unlocking (manual, iPhone and Android)
- [ ] The relay forwards only within a room; a phone in room ABCD never receives messages for room WXYZ (relay test)
- [ ] A second TV with the wrong secret cannot take over a room (relay test)
- [ ] When the TV socket closes, phones get `tvAway`; when it returns, `tvBack`; phone messages meanwhile are dropped (relay test)
- [ ] A `close` control message closes exactly that phone socket with 4002; a phone over 20 messages per second loses the extra messages but stays connected (relay test)

### Day 2: walking skeleton (Wed)

- [ ] 1 real phone and 3 browser tabs on the laptop finish a game of 4 multiple choice questions with scores and Play again, without errors (manual)
- [ ] Scoring: a correct answer at 0 ms gets 1000, at the full window 500, wrong 0; in round 3 every point is multiplied by 1.5; a 2x timer multiplier doubles `T` (unit tests)
- [ ] A question ends within 200 ms after the last connected player locks in (simulated game)
- [ ] Personal deadlines: an input after the player's own deadline plus `net.inputGraceMs` gets `closed`; last call resends the screen with the same phaseId (unit tests)
- [ ] Start from any phone runs the 3 s countdown; Cancel from any phone stops it; Start needs 2 players (unit tests)
- [ ] Names: only Latin and Cyrillic letters, digits and spaces, 1 to 16 characters; `characterTaken` when a character was just taken; `leave` frees the seat (unit tests)
- [ ] The event log is written with `phaseStarted`, `phaseEnded` and `input` lines

### Day 3: the full show (Thu)

- [ ] A simulated Standard game with the six simulated players runs every phase in order and lasts 8 to 12 minutes of game clock from the end of the start countdown to the end of Results, without mini games (simulated game, 100 runs). With simulated players that always use the full answer window it must not exceed 12 minutes
- [ ] No stretch without input lasts longer than the idle limit outside Intro, FinalIntro and Results. The limit starts at 11 s; like every timer it is a starting value (D56), set again whenever timers or staging extras change after a playtest. The current timers give exactly 11 s (QuestionShow + Reveal + Scores, and Reveal + Scores + RoundEnd), so the simulation must end each phase at its exact budget on the fake clock, never a frame later. The idle share is printed and logged, not asserted (simulated game)
- [ ] Topic vote: doors follow the rules in `02_show_flow.md`; ties resolve as specified (unit tests)
- [ ] Ranks: equal scores share the better rank (1, 1, 3) (unit tests)
- [ ] Closest value: distance scoring and the closest bonus work, including ties for closest (unit tests)
- [ ] Final race: head starts match the formula; `lastPlaceCanWin` (both conditions in `02_show_flow.md`) is true in every run; the race ends at the finish or after 12 questions (simulated game)
- [ ] No question or race statement is repeated within 2 games in a row (simulated game over 2 games, starting from an empty seen list)

### Day 4: power ups, hosts, poll (Fri)

- [ ] Draw chances by rank match the formula in `03_rules.md` over 10,000 simulated draws within 2% (unit test)
- [ ] Targeting free and aim up both follow `03_rules.md`; caps and immunity hold (unit tests)
- [ ] Freeze, slime and fog appear on the target's phone at the start of the next answer phase and can be cleared (manual)
- [ ] Host lines: at most one line per question cycle, chosen by the priority in `15_schemas.md`; `fastAnswer` only for the single fastest correct player; `playerJoined` fires for each new player in the Lobby, never for a rejoin or Play again; `startCountdown` fires when the countdown begins; the host silent longer speaks; never more than one bubble at a time per host; no line repeats in an evening while others of its trigger remain (unit tests)
- [ ] The poll appears on every phone after Results, ends when every connected phone submitted or after 90 s, and answers land in the log without player ids
- [ ] The TV settings panel opens with Tab and changes game language, power ups, each power up type and targeting; Reset seen questions and Back to lobby work (manual)
- [ ] All content files load and validate; a broken file shows a clear error (unit test with a broken fixture)

### Stretch: staging grey box (only after every Day 4 check passes)

- [ ] The TV cuts between two cameras by phase: `Stage` during QuestionShow and Answer, `Wide` in every other phase, with the flat race track over `Wide` in the race (manual)
- [ ] A joining player appears as a capsule in their character's signature color in the first free seat, with the character's name above it, and disappears when they leave (manual)
- [ ] On Reveal every capsule shows a colored card with its letter or number, an empty card for no answer, readable from the sofa at 3 m; the cards hide when Scores ends (manual)
- [ ] Every `staging.extraSeconds` value is 0 and every Day 3 timing check still passes (simulated game)

### Day 5: rehearsal (Sat)

- [ ] A full Standard game with 2 real phones and 4 browser tabs on the laptop, twice in a row with Play again, without a restart (manual)
- [ ] Every screen in `06_phone_screens.md` has been seen on a real iPhone and a real Android phone (manual)
- [ ] Text on the TV is readable from the sofa at 3 m (manual)
- [ ] macOS build starts from Finder and connects to the deployed relay (manual)

### Game night

- [ ] Reset seen questions before the first game
- [ ] 2 full Standard games with friends, no restart
- [ ] Poll filled in by everyone
- [ ] Logs and notes saved to `playtests/<date>/`
