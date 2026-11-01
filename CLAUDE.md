# CLAUDE.md

Rules every Claude Code session follows in this repository. Read this file fully before writing code.

## What this project is

A party quiz game for families, inspired by Knowledge is Power. A Unity app on a laptop shows the game on the TV; players use their phones as controllers through a web page (no app install). Two hosts, based on and voiced by friends, react to the game.

## Source of truth

Read these before building anything:

1. `docs/00_build_spec_kit.md`: the list of every spec document and its status. Do not build a feature whose spec documents are not marked Done.
2. The spec documents `docs/01_*.md` to `docs/19_*.md` and `docs/PROTOCOL.md`. When a spec and the master plan disagree, the spec wins.
3. `docs/decisions/`: logged decisions. Never reopen a logged decision on your own; ask first.
4. `docs/master_plan.md`: pillars, architecture, build order. Background.
5. `docs/research_party_quiz_fun.md`: the research behind every timing, rule and threshold. Background.

The current goal is **game night 1** (weekend of Nov 7 and 8, 2026). Its scope and acceptance checks are in `docs/19_acceptance.md`. Daily build prompts are in `docs/prompts/`. Do not build beyond that scope unless asked.

Development happens on both Windows and macOS. Use cross platform paths and scripts (no bash only tooling for required steps; Node scripts are fine).

If a spec is missing something you would have to guess, stop and list the question instead of picking a default.

## The five pillars

Every feature must pass all five. A feature that breaks one is redesigned or cut.

1. **Never wait:** things happen at the same time; a phase ends the moment everyone is done.
2. **Everyone can win:** capped speed scoring and a final race that last place can still win. Targeting is free by default (ganging up is part of the fun); "aim up only" is a setting.
3. **Every tap feels good:** phone feedback within 0.1 s, celebrations sized to what the player did.
4. **Hosts react, never block:** host lines never extend a phase and never repeat in an evening.
5. **Fun is measured:** every build logs events; every game night ends with a phone poll.

## Architecture rules (enforced)

- Three programs: the TV app (`unity/`, Unity 6.3 LTS, C#), the phone page (`phone/`, TypeScript), the relay (`relay/`).
- The TV app owns every rule and every second. Phones and the relay hold no game logic.
- `Assets/Game/Core` is plain C#: no `UnityEngine`, no network, no audio. Its assembly definition has "No Engine References" on.
- Dependencies point one way: every layer may use `Core`; `Core` uses nothing. `Presentation` may reference `Core`, never the reverse.
- Commands in, events out. Only commands (`SubmitAnswer`, `VoteTopic`, `UsePowerUp`, `Skip`) change the game state. Only events (`PhaseStarted`, `AnswerLocked`, `Revealed`, `ScoreChanged`, `PowerUpApplied`) report what happened.
- Listeners (Presentation, Dialogue, Audio, Telemetry) only listen. They can never change state or delay a phase.
- The core owns the clock. Clock and random numbers are injected so tests run at 100x speed and seeds replay games exactly.
- Network messages are queued and handled once per frame on Unity's main thread.
- Every number (timers, scores, caps) lives in `content/config.json`, never as a literal in code.
- Every player facing text is a localization key (Russian and English).
- `docs/PROTOCOL.md` is the single source of truth for phone and TV messages. Never change a message without updating it and asking first.

## How to work

1. Read the spec for the feature in `docs/`.
2. Write the tests first: a unit test or a bot simulation, straight from the spec's acceptance criteria.
3. Build until the tests pass, inside the rules above.
4. Run the tests from the command line after every change.
5. One branch per feature; commit after every green test run.

## Never without asking

- Change `docs/PROTOCOL.md`, config key names, or a logged decision.
- Add a new library or package.
- Delete content files or playtest logs.

## Repo layout

```
CLAUDE.md            this file
docs/                spec documents, PROTOCOL.md, decisions/, moodboard/
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

## Open items

Coding conventions and commit message style are still to be decided (spec document 17). Until then: C# PascalCase for types and methods, one class per file, no singletons outside the composition root.
