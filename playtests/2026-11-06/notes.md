# Game night notes: Nov 6, 2026 (Mac, first family playtest)

**Who:** 3 players, including Ilia. Nicknames: Ilusha, 21Ravage and Соне4ка in games 1 and 2; Ilushka, Агата Соня and Соне4ка in games 3 and 4. Konstantin answered the host question in the interview; Ilja was not there.
**What:** 4 Standard games, Russian, curated bank in games 2 to 4 (`cur_*` question ids).
- Games 1 and 2: QuestionShow 4 s.
- Games 3 and 4: QuestionShow 8 s (D125, changed between the sessions).

**Sources:**
- Event logs: `logs/`, one file per session. The first two files are aborted lobbies.
- Analyzer output: `analysis_21-56-13.txt` (games 1 and 2), `analysis_22-22-58.txt` (games 3 and 4).
- Phone polls: inside the logs.
- Written scorecard: one card, filled in together after game 1 ([Game Night Scorecard](https://claude.ai/artifact/7Bm25ywE4ZoA2DEsRhUcnj)).
- 31 minute group interview: `interview_transcript.txt`, transcribed by Whisper large-v3-turbo. Expect misheard words; the audio is not in the repo.

## Numbers

| | Game 1 | Game 2 | Game 3 | Game 4 |
| --- | --- | --- | --- | --- |
| QuestionShow | 4 s | 4 s | 8 s | 8 s |
| Game time | 8:05 | 7:49 | 9:01 | 8:45 |
| Idle share | 38.0 % | 38.3 % | 47.3 % | 45.9 % |
| Longest idle stretch | 11.0 s | 11.0 s | 15.0 s | 15.0 s |
| Average answer phase | 9.1 s | 9.6 s | 7.3 s | 6.6 s |
| Questions nobody got right | 4 of 16 | 7 of 16 | 5 of 16 | 2 of 16 |
| Power ups used | 7 | 1 | 10 | 7 |
| Winner | 21Ravage | Ilusha | Агата Соня | Агата Соня |

- **Idle stretches:** they match the budgets exactly: 11 s with the old timers and 15 s with QuestionShow at 8 s. `tools/analyze_log.mjs` still checks the old 11 s limit (`IDLE_LIMIT_MS`), so it prints OVER for games 3 and 4. It needs moving to 15 s (D125).
- **Answer phases:** they always ended early because everyone was done (allDone 63 of 64). Nobody ran out the answer clock. The time pressure people felt comes from the phases with fixed lengths: reading the question, the reveal, the scores and the topic vote.
- **Comeback:** game 3's winner started the race last.
- **Race statement repeats:** game 4 repeated race statements from earlier games. The 4 games used 45 statements and `race.json` holds 40, so the bank ran out. This is the documented fallback (unseen first, then seen), not a bug.

## Poll answers (phones)

| Item | Games 1 and 2 (4 s) | Game 3 (8 s) | Game 4 (8 s) |
| --- | --- | --- | --- |
| Enough time? | Too little 6 of 6 | Too little 2, just right 1 | Too little 1 of 1 |
| Waiting with nothing to do | Never 6 of 6 | Never 3 of 3 | Never 1 of 1 |
| Missed the TV because of the phone | Often 3, no 3 | Often 1, sometimes 1, no 1 | Often 1 |
| Any attack unfair? | A bit 1, no 5 | Yes 1, no 2 | Yes 1 |
| Play again? | Yes 5 of 5 | Yes 3 of 3 | Yes 1 of 1 |

Only one of three players answered the game 4 poll.

**Most fun:** the variety of topics, being sure of an answer and being wrong, the design and the questions, and "Я КАМБЕКНУЛ" ("I made a comeback"): "when one player came back, I saw real emotions".

**Most frustrating:** "Everything was very fast, I couldn't read or understand anything. I won't even have time to fill this in." "The questions changed too fast and the right answer wasn't clear." "Not enough time to send traps to other players." "I could tap a button by accident; once I even tapped one with my head."

## Scorecard (one card, after game 1)

- **Ratings:** fun 7/10, would recommend 6/10, could win until the end 4/5, final race 5/5, phone 5/5, look 5/5, TV text easy to read 4/5.
- **Power ups:** using them 1/5, being hit 3/5.
- **Hosts:** lines read: almost none; funny: 1/5.
- **Pace:** too fast. Game length: too short. Wants 3 games in a row.
- **Next time they'd rather play:** Knowledge is Power.
- **Rules understood?** Closest value scoring: no. Multipliers: no. Power ups: no. Speed scoring: partly. Race head start: yes.
- **Best to worst:** final race, multiple choice, closest value, topic vote, power ups, hosts, results.
- **In a word:** «Прикол», «Норм», «Весело» ("a laugh", "OK", "fun").

## Interview findings, grouped

**1. Everything is too fast** (every source, every game; 8 s helped but was not enough)
- Reading the question: "I didn't have time to read or to think." Older relatives will need time to read and to work out where to tap.
- Topic vote 10 s: "I only had time to read the topics". "Even sober people need more time."
- Race statement 8 s: "maybe it needs more time".
- Reveal and scores: "I wanted to see a bit more of who answered what"; "too fast to enjoy the game".
- Host lines: "I literally had no time to read them".

**2. The reveal does not show what happened**
- The correct answer is hard to see: the answer colors are too bright against the white card, with low contrast. Make the colors stronger.
- They can't see who was right at first; it only shows when the points move. They can't see how many points each player got.
- Players are shown as a letter only. Nicknames can share a first letter. Show the character's picture instead.
- The leaderboard runs right to left with first place on the left. They expect progress left to right, with first place on the right.
- **Bug:** a long topic name label covers a few letters of the question.

**2b. Show the answer options on the phone only** (01:34 to 02:07 and 05:37 in the transcript)
- "First you read the question, then the options on the phone." There was confusion about where the options are: some players looked at both the TV and the phone.
- **The proposal:** the answer options appear on the phone only, and no longer on the TV during Answer.
  - The question on the TV gets more room and a bigger font.
  - People pay more attention to their phone, so they notice power ups more.
- It also makes the power ups matter. "Power ups would only make sense if the answers weren't on the TV; then pelting someone with attacks would make it hard to read the answers." Today fog and slime can be ignored by reading the options on the TV.
- Needs a decision: it changes what the TV shows during Answer (`02_show_flow.md`). It builds on D88, which gives the phone "the answer buttons (option texts…)" and the TV everything else.
  - Open question: do the options come back on the TV at Reveal, so the correct answer can be shown?

**2c. TV and phone lay out the same choices differently** (Ilia, after the interview; the players noticed every mismatch)

| What you choose | TV | Phone |
| --- | --- | --- |
| Answer options | 2×2 | 2×2 (the same) |
| Topic vote, 4 topics | Cards side by side in a fan | Stacked top to bottom |
| True or false (race) | Statement and track | Buttons stacked |
| Leaderboard | A row of cards, 1st on the left | Not on the phone; players expect 1st on the right |

- Players have to translate from the TV's order to the phone's order before they can tap, which costs reading time.
- **Proposed rule:** every choice has the same shape, position and color on both screens:
  - 4 choices are 2×2 on both.
  - 2 choices sit side by side on both.
  - Rankings read left to right, with 1st place on the right.
- The rule gets more important if the answer options move to the phone only (2b).

**3. Power ups feel random, weak and invisible** (the scorecard's weakest part, after hosts and results)
- "I didn't understand where they come from" (Ilia, who built it). Explain on the TV how you earn one.
- All three are too weak. Fog doesn't hide the answers, which are on the TV anyway. Slime is wiped off in 1.7 to 6 s. The small speed penalty means a hit costs only about 50 points.
- The TV doesn't show who hit whom, so nobody notices.
- They want to gang up on the leader with several hits. `powerUps.maxHitsPerQuestion` 1 prevents it, and they disagree with it: "this is a family and friends game; ganging up adds fun."
- Two of them named power ups as the one thing to change.
- They suggest also using power ups in the final race, so the losing players can hurt the leader. They called this something to test.

**4. Scoring doesn't reward what they expect**
- Speed matters too little. A correct answer at the buzzer still earns 500. "The scores swing within two questions; you have no momentum."
- They didn't notice the ×1.5 and ×2 rounds.
- Closest value scoring is unclear. Their suggestion: show on the ruler the zone that scores, around the correct answer, as a green gradient. Outside it scores 0.

**5. The final race is the best part and too short**
- "We were literally fighting for the win"; comebacks feel earned.
- "Like one breath": over before it builds. They want more statements and a longer race, and the hosts reacting during it.
- They noticed a race statement repeat. See the race statement repeats under Numbers.

**6. The phone answers too eagerly**
- They want an answer to count when the finger lifts, so you can slide off to change your mind.
- A bump against the head sent an answer. Taps feel instant, and the connection was solid: rejoined in about a second, no lost taps.

**7. Hosts: unread; they want voices**
- "I urgently want to hear them." Lines only make sense voiced. Konstantin hasn't heard them yet.
- **Idea from Ilia (written note):** the hosts announce the topic that won the vote, maybe with a joke about it ("Озвучка темы хостами. Объявление каждой выбранной темы").

**8. Characters don't feel like yours**
- They want customization, like in Knowledge is Power: their own photo or another personal touch.
- The characters should look and act like their descriptions.
- They want a short introduction of each player's character, and emotions on the figures: who is sad, who celebrates.
- "The winner moment could be bigger."

**9. The show and staging**
- The start is too abrupt. They want a cinematic lead-in instead of a still wide shot.
- The studio is beautiful but seldom shown. Mid-game it feels like "just a question screen", with half the screen questions and half figures. They want camera moves.
- The room is too dark on the TV; brighten it but keep the dim mood.
- Sounds are missing at answer moments and at the win.

**10. The questions are liked**
- "Most questions are great."
- They want more questions you can reason your way to.
- More niche questions on specific topics (a singer, a film), and topics younger players can win: Soviet film favors older players.

**11. Question banks and Russian content** (Ilia, after the interview)
- **The curated bank played better than the full bank.** Game 1 used the full bank; games 2 to 4 used the curated one.
- **Too many Russian questions.** Game 1 drew `soviet_film` and `ru_history`, so 2 of its 4 rounds were Russian topics.
  - Russia-related questions also appear inside topics that are not about Russia.
  - A keyword count (rough: it misses some and hits some false ones) finds about 220 of the 2,650 full-bank questions in non-Russian topics about Russia or the USSR. Worst: languages 19/50, classical 13/50, nineties, noughties, world and books_old 11/50 each.
  - In the curated bank: 5 of 110 (Gagarin twice in space, Olivier salad twice in kitchen, Russian Ark in films_new).
- **Ilia's direction:**
  - Russian content belongs in the Russian topics only (soviet_film, ru_tv, ru_music, ussr, ru_history, ru_politics, ru_places).
  - Keep Russian questions down for now, and decide later whether more are needed.

**12. Picture and sound questions** (Ilia, Nov 7)
- Show a still from a film and ask which film it is from, or play a sound and ask which film (or song, game, cartoon) it is from.
- Possibly more question types, used across several topics, not one topic of their own.
- Needs its own spec: a new question format (`04_formats.md`), how the TV shows the picture or plays the sound, where the media files come from, and how the hosts' and the music's audio stays out of the way.

**Do not change:** the jazz, the dim cozy studio, the music level ("fits perfectly"), the overall look. Joining and reconnecting work.

## The three open questions

These are evidence only. Each one needs Ilia's decision, which then goes in a decision record.

- **D16, the timer:** **change**. Too little time in 9 of 10 poll answers, plus the interview. The fixed phases need more time; the answer window does not, since it always ended early. See the starting values below.
- **D14, targeting:** **keep free**. They explicitly asked for more ganging up, not less. The proposal is to raise `powerUps.maxHitsPerQuestion` so several players can hit the leader on the same question.
- **D15, power ups on the phone:** **keep on the phone, change how they show on the TV**. Who hit whom must be visible on the TV, the way to earn one must be explained, and the effects need to be stronger.

## Starting values to change

The timer rows were applied on Nov 7, 2026 (D126, `docs/decisions/0023_timers_after_first_playtest.md`). The race length, speed penalty and hit limit rows are still proposals.

| Key | Now | Proposal | Evidence |
| --- | --- | --- | --- |
| `timers.questionShow` | 8 | 10 | Games 3 and 4 still "too little" from 3 of 4 answers |
| `timers.topicVote` | 10 | 20 | "I only had time to read the topics" |
| `race.answer` | 8 | 12 | "8 s should be longer" |
| `race.maxQuestions` / `race.finishSteps` | 12 / 10 | longer race | "Like one breath" |
| `timers.reveal` | 3.5 | 6 | "Couldn't see who was right or the points" |
| `timers.scores` | 3.5 | 5 | Same |
| `scoring.speedPenalty` | 0.5 | higher, e.g. 0.8 | "Speed matters too little; the scores swing" |
| `powerUps.maxHitsPerQuestion` | 1 | 2 or 3 | They want to gang up on the leader |

With these values the idle limit is set again (D56). Game length is not a constraint (D127): a longer game is fine when the time buys fun, such as more questions or hosts that make sense.

## Top 3 problems

1. **Pace:** the fixed phases are too short to read, see what happened, and enjoy it.
2. **The reveal is unclear:** the right answer, who got it and the points are not visible.
3. **Power ups don't work as a mechanic:** they feel random, weak and invisible on the TV.
