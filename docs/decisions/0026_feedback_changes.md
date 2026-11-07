# Decision record 0026: the changes from the first playtest

Date: Nov 7, 2026. Ilia asked the AI to "go for everything from the feedback" overnight (`playtests/2026-11-06/notes.md`) and to review the result in the morning. The values here are the AI's proposals from the notes, applied on the branch `night/feedback-2026-11-07`. They are starting values like every timer (D48): change any of them after the next playtest. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D141 | Answer options on the phone only | During Answer the TV shows the question larger and four empty tiles A to D in the phone's 2×2 layout and colours, without the option texts. The option texts appear on the TV at Reveal, with the correct one marked. The phone keeps its option texts (D88). Closest value is unchanged (the slider on the phone, the ruler on the TV). | The TV's option texts during Answer |
| D142 | A readable reveal | At Reveal the correct tile stands out clearly (strong colour, high contrast against the card), and each player's character chip sits on the tile they chose, with their name. Points earned show next to each name, never only a letter | |
| D143 | Answers count on release | On the phone an answer counts when the finger lifts inside the button. Sliding off before lifting cancels it. The button reacts the moment it is touched (pillar 3) | Answer on touch |
| D144 | Speed counts more | `scoring.speedPenalty` 0.5 to 0.8: a correct answer at the buzzer earns 200 instead of 500 | 0.5 |
| D145 | Power ups | At most 3 hits per target per question (`powerUps.maxHitsPerQuestion` 1 to 3), so players can gang up on the leader. Stronger effects: freeze 6 to 10 taps, slime 3 to 5 blobs, fog 3 to 6 s. Effects on one phone play one after another. When a hit lands, the TV shows it clearly ("Аня → Слизь → Соня"). When a player draws a power up, the TV says why ("right answer", and the better chance for players behind the leader) | maxHitsPerQuestion 1, the old effect strengths |
| D146 | A longer final race | `race.maxQuestions` 12 to 20, `race.finishSteps` 10 to 15, `race.maxHeadStart` 3 to 4. `race.json` grows to at least 100 statements, none about Russia (D130's direction) | 12, 10, 3 |
| D147 | Bugs | The topic label on the question card never covers the question text | |

Not done overnight, waiting for Ilia: picture and sound questions (needs ffmpeg and a TMDB key), the hosts announcing the topic (three open questions), host voices, character customisation, a cinematic intro, a brighter studio (needs a light bake).
