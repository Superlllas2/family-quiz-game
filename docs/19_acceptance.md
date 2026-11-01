# 19 Acceptance criteria: game night 1

Status: Done (Nov 2, 2026). Target: game night with friends on the weekend of Nov 7 and 8, 2026.

## Scope

**Must have:** relay, phone page, lobby with QR join, avatars, language per phone, the Standard show (intro, 4 rounds, final race, results, poll), topic vote, multiple choice, closest value, scoring with speed curve and multipliers, 3 power ups with free targeting, final race with true or false, host text bubbles, bots, debug overlay, event log, play again, reconnects, macOS build.

**Stretch, only after every must have passes:** clue by clue format, one mini game, a Windows build, Russian text to speech for host lines.

**Not in game night 1:** art, music, voices, Quick and Long modes as tested features, family photos, an installer.

## Checks

Each check is automated (a test or bot simulation) unless marked (manual).

### Day 1: relay and join (Tue)

- [ ] A phone opens the QR link, joins with a name, and appears in the TV lobby within 1 s (manual)
- [ ] A tap on the phone arrives in Unity and the `ack` is back on the phone within 300 ms on home WiFi (manual, measured in the log)
- [ ] A phone locked for 60 s gets its seat and its screen back after unlocking (manual, iPhone and Android)
- [ ] The relay forwards only within a room; a phone in room ABCD never receives messages for room WXYZ (relay test)
- [ ] A second TV with the wrong secret cannot take over a room (relay test)

### Day 2: walking skeleton (Wed)

- [ ] 3 bots and 1 real phone finish a game of 4 multiple choice questions with scores and Play again, without errors
- [ ] Scoring: a correct answer at 0 ms gets 1000, at the full window 500, wrong 0; in round 3 every point is multiplied by 1.5 (unit tests)
- [ ] A question ends within 200 ms after the last connected player locks in (bot simulation)
- [ ] The event log is written with `phaseStarted`, `phaseEnded` and `input` lines

### Day 3: the full show (Thu)

- [ ] A simulated Standard game with 6 bots runs every phase in order and lasts 8 to 12 minutes of game clock without mini games (bot simulation, 100 runs). With bots that always use the full answer window it must not exceed 12 minutes
- [ ] The idle share is under 20% with bots that answer at normal speed (bot simulation)
- [ ] Topic vote: doors follow the rules in `02_show_flow.md`; ties resolve as specified (unit tests)
- [ ] Closest value: distance scoring and the closest bonus work, including ties for closest (unit tests)
- [ ] Final race: head starts match the formula; `lastPlaceCanWin` is true in every run; the race ends at the finish or after 12 questions (bot simulation)
- [ ] No question is repeated within an evening while unseen questions remain (bot simulation over 3 games)

### Day 4: power ups, hosts, poll (Fri)

- [ ] Draw chances by rank match config over 10,000 simulated draws within 2% (unit test)
- [ ] Targeting free and aim up both follow `03_rules.md`; caps and immunity hold (unit tests)
- [ ] Freeze, slime and fog appear on the target's phone at the start of the next answer phase and can be cleared (manual)
- [ ] Host lines: never more than one bubble at a time per host; no line repeats in an evening while others of its trigger remain (unit tests)
- [ ] The poll appears on every phone after Results, and answers land in the log without player ids
- [ ] All content files load and validate; a broken file shows a clear error (unit test with a broken fixture)

### Day 5: rehearsal (Sat)

- [ ] A full Standard game with 2 real phones and 4 bots on the TV, twice in a row with Play again, without a restart (manual)
- [ ] Every screen in `06_phone_screens.md` has been seen on a real iPhone and a real Android phone (manual)
- [ ] Text on the TV is readable from the sofa at 3 m (manual)
- [ ] macOS build starts from Finder and connects to the deployed relay (manual)

### Game night

- [ ] 2 full Standard games with friends, no restart
- [ ] Poll filled in by everyone
- [ ] Logs and notes saved to `playtests/<date>/`
