# Day 4 prompt: power ups, hosts, poll (Friday)

```
Read CLAUDE.md and re-read docs/03_rules.md (power ups), docs/06_phone_screens.md (sabotage, result, poll), docs/15_schemas.md (host_lines, poll, event log), docs/20_playtest.md and docs/19_acceptance.md (section "Day 4") before starting.

Goal for today: the three power ups with free targeting, host text bubbles, and the end of game poll. After today the game is feature complete for game night 1.

Build on a branch called day4-powerups, tests first:

1. Core: PowerUpDealer (draw after a correct round answer, chance by rank from config, max held 1, seeded), Targeting (free and aimUp), caps (one hit per target per question) and immunity, application at the start of the next Answer phase, loss at the end of round 4, settings to switch power ups and each type on or off. Events for drawn, used and applied.

2. Unit tests: draw chances within 2% over 10,000 draws, free and aimUp targeting, caps, immunity, the immune and noPowerUp ack reasons, nothing on the last question of round 4.

3. Phone: the power up card and target row on the result screen (two taps), the sabotage overlays freeze (tap the ice 6 times), slime (swipe 3 blobs) and fog (blur that clears in 3 s, faster when rubbed), with the attacker's name, and the effectCleared input.

4. Dialogue: line selection from host_lines.json per trigger with shuffle bags saved to disk, weight, minimum gap per host, drop instead of delay, {name} and {target} filled in, the game language. TV shows lines as text bubbles next to a placeholder portrait box labelled with the host name for hosts.bubbleSeconds. Emit every trigger listed in docs/15_schemas.md from the right core events.

5. Poll: after Results, the poll screens from content/poll.json on every phone, answers logged anonymously, host phone can finish the poll, then Play again or New players.

6. Lobby settings on the host phone: game language, power ups on or off, targeting free or aim up.

7. A small tools/analyze_log script in Node that reads a JSONL log and prints idle share, longest idle stretch, phase durations, correct rate per question, power ups drawn versus used, and who targeted whom.

Commit after every green test run. When done, list the Day 4 checks and what is still rough.
```
