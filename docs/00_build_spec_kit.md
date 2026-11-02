# Family Quiz Build Spec Kit

Snapshot from Nov 1, 2026 of the live doc at https://claude.ai/artifact/UQA5Hd1S8UzTiAANuSngNJ. **This repo copy holds the current status** (updated Nov 2, 2026 for game night 1).

**Game night 1:** the documents needed for the technical prototype are done. "Later" means not needed before game night 1. Daily build prompts are in `docs/prompts/`. Decisions from the spec review are in `docs/decisions/0002_spec_review.md`.

## How to use this kit

The spec is complete when an AI building any part of the game has nothing left to guess. Twenty one documents in four groups cover the whole game. The master plan already covers 3 of them, covers 12 in part, and leaves 6 untouched.

**The test for every line:** if two good developers could build it two different ways, the spec must say which way. Anything left open becomes a generic default.

**How to fill it:**

1. Work through the documents in the order at the end of this kit.
2. For each one, answer every open question listed under it. Claude drafts from the research; you correct.
3. Save each finished document in the repo under `docs/` and in this Project.
4. Run the completeness check at the end before asking an AI to build.

**Status in the overview:** *Covered* means the master plan already holds it and it only needs moving into its own file. *Partial* means some decisions exist. *Missing* means nothing is written yet.

## All documents at a glance

Update the Status column as you go; the kit is ready when every row says Done.

| # | Document | Group | Status | Repo file |
| --- | --- | --- | --- | --- |
| 1 | Vision and pillars | Game design | Done | `docs/01_vision.md` |
| 2 | Show flow | Game design | Done for game night 1 | `docs/02_show_flow.md` |
| 3 | Rules and numbers | Game design | Done for game night 1 | `docs/03_rules.md` + `content/config.json` |
| 4 | Question format catalog | Game design | Done for game night 1 | `docs/04_formats.md` |
| 5 | Mini game catalog | Game design | Later | `docs/05_minigames.md` |
| 6 | Phone screens and user flow | Game design | Done for game night 1 | `docs/06_phone_screens.md` |
| 7 | Host bibles and line list | Game design | Later | `docs/07_hosts.md` |
| 8 | Content guidelines | Game design | Later | `docs/08_content.md` |
| 9 | Settings and accessibility | Game design | Partial (game night 1 settings listed in 02, 03 and 06) | `docs/09_settings.md` |
| 10 | Visual style guide | Look and feel | Later (stylized 3D, `decisions/0003_3d_and_idle_limit.md`) | `docs/10_visual_style.md` |
| 11 | Motion and feedback spec | Look and feel | Later | `docs/11_motion.md` |
| 12 | Audio direction and sound list | Look and feel | Later | `docs/12_audio.md` |
| 13 | Tech spec and architecture | Technical | Done for game night 1 | `docs/13_architecture.md` |
| 14 | Message protocol | Technical | Done for game night 1 | `docs/PROTOCOL.md` |
| 15 | Data schemas and sample content | Technical | Done for game night 1 | `docs/15_schemas.md` + `content/` |
| 16 | Asset list | Technical | Later | `docs/16_assets.md` |
| 17 | Build rules for AI | Technical | Partial | `CLAUDE.md` |
| 18 | Edge cases | Proof | Done for game night 1 | `docs/18_edge_cases.md` |
| 19 | Acceptance criteria | Proof | Done for game night 1 | `docs/19_acceptance.md` |
| 20 | Playtest plan | Proof | Done for game night 1 | `docs/20_playtest.md` |
| 21 | Decisions log | Proof | Done | `docs/decisions/` |

## Game design documents

These say what the game is and how it plays. Each lists what it must contain, then the questions still open.

### 1. Vision and pillars

- **Contains:** one paragraph pitch, target players (ages, group size, setting), session length, the five pillars, 2 or 3 reference games with "like X, but Y", and a list of what the game is not.
- **Open:** Is the first audience only your family, or buyers too? What is explicitly out of scope for version 1 (online play, accounts, 3D)?

### 2. Show flow

- **Contains:** every phase in order for Quick, Standard and Long, each with its time budget, what the TV shows, what every phone shows, and what ends the phase early. Already in the master plan; move it into its own file.
- **Open:** What happens between games: straight to the lobby or straight into a new game?

### 3. Rules and numbers

- **Contains:** one table of every number in the game (timers, scoring curve, round multipliers, power up effects, hit caps, final race seeding, step sizes), each with its config key and default. Win and tie rules in words.
- **Open:** The full list of power ups with their exact effect and duration. Tie break rule for the winner. Final race track length and step rules.

### 4. Question format catalog

- **Contains:** one card per format (multiple choice, closest number, order, which came first, odd one out, picture zoom, true or false burst): TV view, phone view, timer, scoring, and the fields a question file needs.
- **Open:** Which 3 formats ship first (M3)? How typed answers are matched (spelling tolerance, both languages)?

### 5. Mini game catalog

- **Contains:** one card per mini game: name, input (tap, drag, swipe, sort), rules card text, length, scoring, how it pays out power ups, and how it works with 2 and with 8 players.
- **Open:** Which 2 or 3 mini games come first? Nothing is designed yet.

### 6. Phone screens and user flow

- **Contains:** every phone screen (join, name and avatar, lobby, waiting, vote, each answer type, power up pick, target pick, sabotage effect, mini game, result, poll, reconnecting), each with its one decision, its buttons and where it leads. A flow diagram of how screens connect.
- **Open:** Decided for game night 1 (0002): 12 preset avatars (colored shape plus first letter) and no host phone; every phone has the same rights.

### 7. Host bibles and line list

- **Contains:** one page per host (stage name, want, flaw, catchphrase, attitude to the other host, favorite and hated topics, limits), plus a table of every trigger that can play a line, its priority and how many variants it needs.
- **Open:** Stage names. Which friend is the needler and which the overreactor. Written consent.

### 8. Content guidelines

- **Contains:** the six point question checklist, difficulty mix, topic list, family mode filter, photo consent rules, and the AI drafting pipeline with its checks.
- **Open:** The starting topic list (aim for 20 or more). Which languages each question must exist in.

### 9. Settings and accessibility

- **Contains:** every toggle and slider with its default and where it is changed (TV settings panel or the player's own phone): game length, chaos level, each power up, kid timers, volume sliders, reduce motion, show question on phone, language.
- **Open:** Decided for game night 1 (0002): game settings live in a TV settings panel (Tab key, mouse); phones keep only language, more time and leave.

## Look and feel documents

This is the largest gap: the master plan has no visual decisions yet, so an AI would invent a generic look. Each value below must be an exact number or code, not an adjective.

### 10. Visual style guide

- **Art direction:** one sentence of style (for example "late night TV studio, warm and slightly retro"), plus a mood board of 8 to 12 reference images saved in `docs/moodboard/`.
- **Palette with roles,** as hex codes:

| Role | Hex | Used for |
| --- | --- | --- |
| Background dark | | TV backdrop |
| Surface | | Cards, panels |
| Primary accent | | Highlights, timers |
| Correct | | Right answer |
| Wrong | | Wrong answer, always calmer than Correct |
| Answer A, B, C, D | | Four button colors, each also with a shape |
| Player 1 to 8 | | Avatar rings and score rows |
| Text on dark, text on light | | Body text |

- **Typography:** font for headings, font for body, both with Cyrillic and Latin glyphs and a license that allows selling. Sizes at 1080p: question text 52 to 78 px, options, scores, small labels.
- **Layout:** TV safe area margins, a grid for the question screen, the score bar, avatar positions for 2 to 8 players. One wireframe per TV screen.
- **Components:** buttons, answer tiles, avatar, timer, score row, power up icon, each with its states (idle, pressed, locked, correct, wrong, disabled).
- **Host look:** portrait size and position, the list of expressions (neutral, smug, shocked, laughing), mouth flap frames.
- **Open:** Style sentence, mood board, palette, fonts, logo or title treatment. Light or dark phone page.

### 11. Motion and feedback spec

- **Contains:** a table of every animated moment (tap, lock in, correct, wrong, score count, leaderboard move, new leader, sabotage hit, round win, final win) with duration in ms, easing curve, sound cue and screen shake or not. The research gives first values; move them here.
- **Open:** Easing style (snappy and bouncy, or smooth). Transition between phases (cut, slide, wipe).

### 12. Audio direction and sound list

- **Contains:** music style with 2 or 3 reference tracks, the music pieces needed (lobby, round, final, results), the full list of sound effects with filename and length, ducking rules under host voice, and a license log.
- **Open:** Music style. Composed by you or licensed? One track per round with a semitone rise, or separate tracks?

## Technical documents

These tell the AI exactly how to build, so it never picks a library, a folder or a file format on its own.

### 13. Tech spec and architecture

- **Contains:** engine and exact versions (Unity 6.3 LTS, TypeScript, relay platform), every library allowed, the three programs, the layers inside the TV app, the dependency rules, the time model, the folder layout. Already in the master plan; move it into its own file.
- **Open:** Confirm the defaults in the decisions table: phone library (Preact), relay host (Cloudflare), first platform (macOS), offline needed or not.

### 14. Message protocol (PROTOCOL.md)

- **Contains:** every message type with its exact JSON fields, types and an example; every phone screen type with its data; error codes; protocol version rule; rejoin and timeout values.
- **Open:** The full field list per message. The master plan has only the message names.

### 15. Data schemas and sample content

- **Contains:** the exact file format (JSON or CSV) and every field for: questions of each format, topics, host lines with their triggers and tags, show scripts per mode, the config file, and the event log line. Plus 10 real samples of each in `content/samples/`.
- **Why it matters:** an AI copies the shape of real examples far more reliably than it follows a description.
- **Open:** JSON or spreadsheet as the master format for questions? How a question stores both languages.

### 16. Asset list

- **Contains:** one row per asset: filename, type (sprite, font, sound, music, voice), size or length, where it is used, placeholder allowed (yes or no), source and license.
- **Rule:** the AI builds with placeholders named exactly like the final files, so swapping art later needs no code change.
- **Open:** Nothing listed yet; fills itself once docs 10 to 12 are done.

### 17. Build rules for AI (CLAUDE.md)

- **Contains:** the dependency rules, coding conventions (naming, one class per file, no singletons outside the composition root), "tests first" rule, how to run tests from the command line, what never to change without asking (protocol, config keys), and links to every doc above.
- **Open:** Your coding conventions. Commit message style.

## Proof documents

These define "done", so the AI knows when to stop and you know whether it worked.

### 18. Edge cases

AI skips these unless they are listed. Each needs one line saying what the game does:

- Only 2 players, or 8 players
- A player joins mid game, or joins twice from the same phone
- A phone locks, reloads, or loses WiFi during a question
- A player leaves on purpose
- Nobody answers a question; everyone answers wrong
- Two players tie for the win, or tie for a rank that decides targeting
- A power up targets a player who just disconnected
- The laptop loses internet, or the relay is down
- The question bank runs out of unseen questions for a topic
- A name with emoji, Cyrillic, or 30 characters

### 19. Acceptance criteria

- **Contains:** for each feature and each screen, 2 to 5 checks that a test or a person can mark pass or fail, for example "the reveal starts within 1 s of the last answer" or "a phone locked for 60 s gets its seat back". The milestone gates in the master plan are the top level; this goes one level down.
- **Open:** Written per feature, just before that feature is built.

### 20. Playtest plan

- **Contains:** what is logged, the observation sheet, the 3 minute poll, and the keep, change or drop rules. Fully covered by the research; move it into its own file.

### 21. Decisions log

- **Contains:** one short record per decision: date, question, options, choice, reason. Start with the ten decisions in the master plan, then add one per risk spike.
- **Rule:** the AI may never reopen a logged decision on its own.

## Fill order and the final check

Fill the documents in this order. Each step leans on the one before, and steps 1 to 4 need your decisions most.

1. **Decide:** Vision (1) and the Decisions log (21), so every later doc has fixed ground.
2. **Design the game:** Show flow (2), Rules and numbers (3), Question formats (4), Mini games (5).
3. **Design the look:** Visual style guide (10), Motion (11), Audio (12).
4. **Design the players' view:** Phone screens (6), Settings (9).
5. **Write the people and words:** Hosts (7), Content guidelines (8).
6. **Translate into tech:** Tech spec (13), Protocol (14), Schemas and samples (15), Asset list (16), CLAUDE.md (17).
7. **Define done:** Edge cases (18), Acceptance criteria (19), Playtest plan (20).
8. **Paper game night,** then correct any document it proved wrong.

### Ready to build when all of these are true

- [ ] Every row in the overview table says Done
- [ ] Every number in the game appears exactly once, in the Rules table, with a config key
- [ ] Every color is a hex code with a role; every font has a license that allows selling
- [ ] Every phone screen and every TV screen has a wireframe or a written layout
- [ ] Every content type has 10 real samples in the final file format
- [ ] Every edge case has a written answer
- [ ] Every feature has acceptance checks a test can run
- [ ] Claude can read the full kit and list zero questions it would have to guess

The last check is the real test: give Claude the whole kit and ask "What would you have to guess to build milestone M2?" Fix every item it names, then ask again.
