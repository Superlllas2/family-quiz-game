# Family Quiz

A party quiz game for families, inspired by Knowledge is Power. The game runs on a laptop connected to the TV, and players answer on their phones through a web page.

**Status:** planning (milestone M0). No code yet.

## Where to start

| File | What it is |
| --- | --- |
| [`docs/00_build_spec_kit.md`](docs/00_build_spec_kit.md) | Every spec document the game needs, with status and open questions |
| [`docs/master_plan.md`](docs/master_plan.md) | Pillars, show flow, architecture, build order, decisions |
| [`docs/research_party_quiz_fun.md`](docs/research_party_quiz_fun.md) | The research behind every timing and rule |
| [`CLAUDE.md`](CLAUDE.md) | Rules for AI coding sessions in this repo |

## Layout

- `unity/`: the TV app (Unity 6.3 LTS)
- `phone/`: the phone controller page (TypeScript)
- `relay/`: the room server that connects phones to the TV app
- `content/`: questions, host lines, show scripts, config
- `tools/`: content checks, AI drafting, log analysis
- `playtests/`: logs, polls and notes from each game night
