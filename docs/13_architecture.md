# 13 Tech spec and architecture

Status: Done for game night 1 (Nov 2, 2026). The rules in `CLAUDE.md` apply; this file adds the concrete stack and structure.

## Stack

| Program | Stack | Folder |
| --- | --- | --- |
| TV app | Unity 6.3 LTS, C#, Universal Render Pipeline 2D, TextMeshPro, Unity Test Framework. WebSocket client: `System.Net.WebSockets.ClientWebSocket` (no extra package) | `unity/` |
| Phone page | Preact, TypeScript, Vite. No UI library. Plain CSS | `phone/` |
| Relay | Cloudflare Workers with Durable Objects (one object per room), TypeScript, Wrangler. Serves the built phone page as static assets | `relay/` |
| Tools | Node 20+ scripts in TypeScript or plain JS, run with `node` or `npx tsx` | `tools/` |

Allowed packages beyond these: `qrcode` (phone or TV QR generation, or a C# QR library vendored as source with an MIT license). Anything else: ask first.

## TV app structure

```
unity/Assets/Game/
  Core/            FamilyQuiz.Core.asmdef      no engine references
    Commands/      SubmitAnswer, VoteTopic, UsePowerUp, HostAction, PlayerJoin, ...
    Events/        PhaseStarted, PhaseEnded, AnswerLocked, Revealed, ScoreChanged, PowerUpApplied, HostLineRequested, ...
    State/         GameState, PlayerState, RoundState, RaceState
    Rules/         Scoring, PowerUpDealer, Targeting, TopicPicker, QuestionPicker, RaceRules
    Show/          ShowDirector (runs phases on the injected clock)
    Content/       content and config models plus the validator
    IClock, IRandom
  Net/             FamilyQuiz.Net.asmdef       references Core
    RelayClient, MessageQueue (thread safe), Protocol DTOs, ScreenBuilder (events to per phone screens)
  Presentation/    FamilyQuiz.Presentation.asmdef  references Core
    TV views per phase, host text bubbles, debug overlay
  Dialogue/        references Core: line selection with shuffle bags
  Telemetry/       references Core: JSONL writer
  DevTools/        references Core, Net: bots, phase skip keys, fake phones
  Bootstrap/       composition root: builds everything, loads content, starts the clock
unity/Assets/Tests/
  EditMode/        FamilyQuiz.Tests.asmdef: unit tests and bot simulations against Core only
```

## Core loop

1. `Bootstrap` loads config and content, validates them, creates `GameState`, `ShowDirector`, the listeners and the `RelayClient`.
2. Every frame on the main thread: drain `MessageQueue`, turn messages into commands, call `ShowDirector.Tick(now)`.
3. `ShowDirector` applies commands, ends phases on time or early, and raises events.
4. Listeners react to events. `ScreenBuilder` turns events into `screen` messages per phone.

## Bot simulation

`Tests/EditMode/BotSimulation`: runs a full Standard game with a fake clock at 100x or faster and N bots with settings for answer speed (fast, normal, slow), accuracy (0.3 to 0.9) and power up use. It asserts the checks in `19_acceptance.md` and prints phase durations and the idle share.

## Running tests from the command line

Windows (PowerShell):

```
& "C:\Program Files\Unity\Hub\Editor\6000.3.<x>f1\Editor\Unity.exe" -batchmode -projectPath unity -runTests -testPlatform EditMode -testResults TestResults.xml -logFile -
```

macOS:

```
/Applications/Unity/Hub/Editor/6000.3.<x>f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath unity -runTests -testPlatform EditMode -testResults TestResults.xml -logFile -
```

Replace `<x>` with the installed version. Phone and relay: `npm test` in their folders (Vitest).

## Local development without deploying

- Relay: `npx wrangler dev` in `relay/` serves on `http://localhost:8787`. Phones on the same WiFi can use the laptop's address for quick tests, without wake lock (needs HTTPS).
- For phone tests over HTTPS before deploying, `npx wrangler dev --remote` or a deploy to a `dev` worker.
- `local.json` in the TV app chooses the relay host.
