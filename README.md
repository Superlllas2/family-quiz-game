# Family Quiz

A party quiz game for families, inspired by Knowledge is Power. The game runs on a laptop connected to the TV, and players answer on their phones through a web page.

**Status:** specs done for game night 1 (weekend of Nov 7 and 8, 2026). Building starts with `docs/prompts/day0_setup.md`.

## Where to start

| File | What it is |
| --- | --- |
| [`docs/00_build_spec_kit.md`](docs/00_build_spec_kit.md) | Every spec document the game needs, with status and open questions |
| [`docs/master_plan.md`](docs/master_plan.md) | Pillars, show flow, architecture, build order, decisions |
| [`docs/research_party_quiz_fun.md`](docs/research_party_quiz_fun.md) | The research behind every timing and rule |
| [`CLAUDE.md`](CLAUDE.md) | Rules for AI coding sessions in this repo |
| [`docs/prompts/`](docs/prompts/) | Day by day build prompts for game night 1 |
| [`docs/19_acceptance.md`](docs/19_acceptance.md) | Scope and checks for game night 1 |

## Layout

- `unity/`: the TV app (Unity 6.3 LTS)
- `phone/`: the phone controller page (TypeScript)
- `relay/`: the room server that connects phones to the TV app
- `content/`: questions, host lines, show scripts, config
- `tools/`: content checks, AI drafting, log analysis
- `playtests/`: logs, polls and notes from each game night
