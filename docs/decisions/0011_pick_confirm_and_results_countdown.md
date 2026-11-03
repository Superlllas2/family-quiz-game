# Decision record 0011: Confirm on the character pick; a countdown on Results

Date: Nov 3, 2026. Decided by Ilia after the first Ready game (D76) on real phones. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D77 | How a character is picked | A tap on a card only selects it on the phone; a Confirm button below the grid sends `character` for the selected card. The card the TV assigned starts selected, so Confirm alone keeps it. If someone takes the selected card before Confirm, the selection falls back to the assigned one. Phone only: the `character` message is unchanged | 06: "Pick one" by tapping a card |
| D78 | How players know the next game is coming | Results shows how long until everyone is back in the Lobby: on the TV a draining bar and "Сбор на новую игру через N / Back to the lobby in N", on the phone the same line from `endsInMs`. It runs for the whole of Results (`timers.results`). Once the Poll exists, the line names the Poll instead | Results ending with no warning |

## What changes in `PROTOCOL.md`

Nothing. `results` already carries `endsInMs`.
