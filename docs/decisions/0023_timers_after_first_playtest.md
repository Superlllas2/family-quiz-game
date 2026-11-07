# Decision record 0023: timers after the first playtest

Date: Nov 7, 2026. After the Nov 6 playtest (`playtests/2026-11-06/notes.md`), Ilia asked to apply every proposed timer change, and said the game should grow in length as much as it needs: "It felt really fast." The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D126 | Phase timers | `timers.topicVote` 10 to 20 s, `timers.questionShow` 8 to 10 s, `timers.reveal` 3.5 to 6 s, `timers.scores` 3.5 to 5 s, `race.answer` 8 to 12 s. The idle limit of D42 is set again to 21 s (QuestionShow 10 + Reveal 6 + Scores 5), as D56 allows; `tools/analyze_log.mjs` uses it too | The read time of D125 and its 15 s idle limit |
| D127 | Game length | **An outcome, not a target.** A game takes as long as it needs to be fun; time is worth adding for more questions, a longer race, or hosts that make sense. No design cap and no length goal when tuning. The simulated game tests keep only a 60 minute bound that catches a game that never ends, and the 8 minute minimum that catches skipped phases | The 10 to 12 minutes of D41 and the 30 minute cap of D125 |

**Evidence:**
- **Not enough time:** 9 of the 10 phone poll answers said "too little time", 3 of 4 even with the 8 s read.
- **Interview:** "I only had time to read the topics" (topic vote). "Couldn't see who was right or the points" (reveal and scores). The race's 8 s "should be longer".
- **Answer window:** it ended early in 63 of 64 questions because everyone had answered, so the answer timers stay as they are.

**Game length:** a game now takes about 12 to 14 minutes, at most about 16 with full answer windows. This is a result, not a limit (D127).

**Not part of this record:** the other proposals in the notes are not timers and are not decided: a longer race (`race.maxQuestions`, `race.finishSteps`), `scoring.speedPenalty` and `powerUps.maxHitsPerQuestion`.
