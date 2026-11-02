# Day 4 prompt: power ups, hosts, poll, TV settings (Friday)

```
Read CLAUDE.md and re-read docs/02_show_flow.md (TV settings panel), docs/03_rules.md (ranks, power ups), docs/06_phone_screens.md (sabotage, result, poll), docs/15_schemas.md (host_lines, poll, event log), docs/20_playtest.md and docs/19_acceptance.md (section "Day 4") before starting.

Goal for today: the three power ups with free targeting, host text bubbles, the end of game poll and the TV settings panel. After today the game is feature complete for game night 1.

Build on a branch called day4-powerups, tests first:

1. Core: PowerUpDealer (draw after a correct round answer, chance by shared rank with the formula in docs/03_rules.md, max held 1, seeded), Targeting (free and aimUp with shared ranks), caps (one hit per target per question) and immunity, application at the start of the next Answer phase, loss at the end of round 4, power ups on or off and each type on or off from the TV settings, applied from the next game. Events for drawn, used and applied. The pick works through Reveal and Scores on one phaseId.

2. Unit tests: draw chances within 2% over 10,000 draws, free and aimUp targeting, caps, immunity, the immune and noPowerUp ack reasons, nothing on the last question of round 4.

3. Phone: the power up card and target row on the result screen (two taps), the sabotage overlays freeze (tap the ice 6 times), slime (swipe 3 blobs) and fog (blur that clears in 3 s, faster when rubbed), with the attacker's name, and the effectCleared input. The answer clock keeps running under the overlay.

4. Dialogue: line selection from host_lines.json exactly as docs/15_schemas.md "Selection": at most one line per question cycle with the trigger priority, fastAnswer only for the single fastest correct player under scoring.fastAnswerMs, streak3 at hosts.streakTrigger, the host who has been silent longer speaks, shuffle bags per trigger and host saved to disk, weight, minimum gap per host, drop instead of delay, {name} and {target} filled in, the game language, the Intro playing one gameStart line from each host, and playerJoined greeting each new player in the Lobby (not on rejoin or Play again). TV shows lines as text bubbles next to a placeholder portrait box labelled with the host name for hosts.bubbleSeconds. Emit every trigger listed in docs/15_schemas.md from the right core events.

5. Poll: after Results, the poll screens from content/poll.json on every phone; the poll ends when every connected phone submitted or after poll.maxSeconds (90 s); answers logged anonymously as an object from question id to a string or an option index; then pollDone with Play again on every phone.

6. TV settings panel (UGUI, Tab key, mouse): game language, power ups on or off, each power up on or off, targeting free or aim up, Reset seen questions, Back to lobby (everyone keeps seats and avatars). It never pauses the game. Phones keep only language, more time and leave.

7. A small tools/analyze_log script in Node that reads a JSONL log and prints idle share, longest idle stretch outside Intro, FinalIntro and Results, phase durations, correct rate per question, power ups drawn versus used, and who targeted whom.

8. Optional, only after every Day 4 check in docs/19_acceptance.md passes: the staging grey box from docs/21_staging.md (section "Grey box version"), on its own branch day4-greybox. A box stage, a box desk, 8 box chairs in the seat order, a capsule per player in their avatar color with a name tag, two plain cameras (Stage and Wide) switched by a StagingDirector listener in Presentation on phase events, and a colored card above each capsule on Reveal (letter in its option color, number for closest value, empty for no answer), hidden when Scores ends. No music, no Cinemachine or other new package. The listener never delays a phase; every staging.extraSeconds value stays 0. Check it against "Stretch: staging grey box" in docs/19_acceptance.md. Skip this step if anything else is unfinished.

Commit after every green test run. When done, list the Day 4 checks and what is still rough.
```
