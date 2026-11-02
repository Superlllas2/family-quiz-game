# 13 Tech spec and architecture

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). The rules in `CLAUDE.md` apply; this file adds the concrete stack and structure.

## Stack

| Program | Stack | Folder |
| --- | --- | --- |
| TV app | Unity 6.3 LTS, C#, Universal Render Pipeline in 3D (the Universal 3D template; D43), UGUI screen space overlay over the 3D camera for the question UI, UGUI with TextMeshPro for all TV UI (no UI Toolkit), Unity Test Framework. JSON: Newtonsoft Json.NET (`com.unity.nuget.newtonsoft-json`). WebSocket client: `System.Net.WebSockets.ClientWebSocket` (no extra package) | `unity/` |
| Phone page | Preact, TypeScript, Vite. No UI library. Plain CSS | `phone/` |
| Relay | Cloudflare Workers with Durable Objects (one object per room), TypeScript, Wrangler. Serves the built phone page as static assets | `relay/` |
| Tools | Node 20+ scripts in TypeScript or plain JS, run with `node` or `npx tsx` | `tools/` |

**TV font:** Inter (SIL Open Font License 1.1) as a placeholder (D66; may change with `10_visual_style.md`). Font files and `OFL.txt` in `unity/Assets/Fonts/Inter/`, from the official release (github.com/rsms/inter); a TextMeshPro font asset with Latin and Cyrillic is the default TMP font. TV texts come from `content/ui_strings.json` (D65); no Unity Localization package.

Allowed packages beyond these: `com.unity.nuget.newtonsoft-json` (TV app, including Core: it is plain .NET), and `qrcode` (phone or TV QR generation, or a C# QR library vendored as source with an MIT license). Anything else: ask first.

## TV app structure

```
unity/Assets/Game/
  Core/            FamilyQuiz.Core.asmdef      no engine references; Override References on, with Newtonsoft.Json.dll as the only precompiled reference
    Commands/      SubmitAnswer, VoteTopic, UsePowerUp, LobbyAction, PlayerJoin, PlayerLeave, ChangeSettings, ...
    Events/        PhaseStarted, PhaseEnded, AnswerLocked, Revealed, ScoreChanged, PowerUpApplied, HostLineRequested, ...
    State/         GameState, PlayerState, RoundState, RaceState
    Rules/         Scoring, Ranking, PowerUpDealer, Targeting, TopicPicker, QuestionPicker, RaceRules
    Show/          ShowDirector (runs phases on the injected clock; adds staging.extraSeconds to a phase budget)
    Content/       content and config models plus the validator (parsed with Newtonsoft)
    IClock, IRandom
  Net/             FamilyQuiz.Net.asmdef       references Core
    RelayClient, MessageQueue (thread safe), Protocol DTOs, ScreenBuilder (events to per phone screens)
  Presentation/    FamilyQuiz.Presentation.asmdef  references Core
    TV views per phase (UGUI + TextMeshPro), character chips from characters.json, host text bubbles, TV settings panel, debug overlay
    StagingDirector (stretch, 21_staging.md): listens to phase events, switches cameras, places guests and answer cards; never delays a phase
  Dialogue/        references Core: line selection with shuffle bags
  Telemetry/       references Core: JSONL writer
  DevTools/        references Core, Net: F1 debug overlay, phase skip keys
  Bootstrap/       composition root: builds everything, loads content, starts the clock
  Editor/          FamilyQuiz.Editor.asmdef (Editor only): copies content/ into StreamingAssets
unity/Assets/Tests/
  EditMode/        FamilyQuiz.Tests.asmdef: unit tests and simulated games against Core only
```

There are no bots in the game: every seat is a real phone or browser tab.

Staging uses plain Unity cameras and, later, Timeline (already in the project). Cinemachine or any other camera package needs asking first.

## Core loop

1. `Bootstrap` loads config and content, validates them, creates `GameState`, `ShowDirector`, the listeners and the `RelayClient`.
2. Every frame on the main thread: drain `MessageQueue`, turn messages into commands, call `ShowDirector.Tick(now)`.
3. `ShowDirector` applies commands, ends phases on time or early, and raises events.
4. Listeners react to events. `ScreenBuilder` turns events into `screen` messages per phone.

## TV settings panel

Tab on the laptop keyboard opens and closes it; it is used with the mouse. Its controls (game language, power ups on or off and per type, targeting, reset seen questions, back to lobby; defaults and timing in `02_show_flow.md`) send `ChangeSettings` or `LobbyAction` commands to Core like any other input. Opening it never pauses the game.

## Content copy

An editor script copies `content/` into `unity/Assets/StreamingAssets/content/` before entering Play mode and before every build (an `IPreprocessBuildWithReport` plus a play mode state hook). The copy is not in git. The TV app reads only the copy.

## Simulated games in tests

`Tests/EditMode/SimulatedGame`: runs full Standard games headless with a fake clock at 100x or faster and simulated players that exist only inside the test code, never in the game. Default table of 6 players:

| Profile | Players | Answer time (uniform) | Accuracy |
| --- | --- | --- | --- |
| Fast | 2 | 1 to 3 s | 0.9 |
| Normal | 2 | 3 to 8 s | 0.6 |
| Slow | 2 | 7 to 14 s | 0.3 |

Answer times apply to votes and race answers too; a time past a player's deadline means no answer. A setting turns power up use on or off. The simulation asserts the checks in `19_acceptance.md` and prints phase durations, the idle share and the longest idle stretch.

## Testing alone

Open the phone page in several browser tabs on the laptop, each with its own `&tab=<n>` in the URL (`https://<relay-host>/?room=ABCD&tab=2`), so each tab keeps its own token and seat (`PROTOCOL.md`, section 4). Add one or two real phones for touch and wake lock checks.

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
