# Day 5 prompt: rehearsal and the Mac build (Saturday, before game night)

Do the rehearsal first, then give Claude Code the problems you found.

## Rehearsal (you, about 1 hour)

1. Connect the laptop to the TV. Start the game, add 4 bots, join with 2 real phones (one iPhone, one Android).
2. Play a full Standard game, then Play again for a second one.
3. Write down every problem: crashes, confusing screens, text too small, anything slow, anything that felt boring or unfair.
4. Check every phone screen from `docs/06_phone_screens.md` appeared at least once on both phones.

## Prompt

```
Read CLAUDE.md and docs/19_acceptance.md (section "Day 5"). Game night is tonight or tomorrow. Here are the problems from the rehearsal, most important first:

<paste your list>

Fix them in this order on a branch called day5-fixes. Do not add features. For each fix, add or update a test when it is about game rules. Then:
1. Make a macOS build (or Windows, if I say so) that starts from Finder and reads local.json next to the app or from StreamingAssets.
2. Make sure the event log folder is easy to find: show its path on the Results screen in small text.
3. Run every test and the bot simulation one last time and report the results.
```

## Game night checklist

- [ ] Laptop charged, TV and sound tested
- [ ] Relay deployed and the build pointing at it
- [ ] Observation sheet ready (`docs/20_playtest.md`), someone to fill it in
- [ ] After the evening: copy the logs into `playtests/<date>/` and tell Claude in the Family Quiz chat how it went
