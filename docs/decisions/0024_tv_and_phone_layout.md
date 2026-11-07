# Decision record 0024: the same layout on the TV and the phone

Date: Nov 7, 2026. After the Nov 6 playtest, Ilia noted that some choices were laid out horizontally on the TV but vertically on the phone, and every mismatch was noticed by the players (`playtests/2026-11-06/notes.md`, 2c). The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D128 | Layout of choices on the TV and the phone | **Every choice has the same shape, position and colour on both screens**, so the eye finds a spot on the TV and the thumb goes to the same spot on the phone. Four choices are two by two on both, numbered row by row (1 2 on top, 3 4 below): the answer options (already so) and the topic vote (the TV's fan of cards and the phone's stacked list both become two by two; door 4, the mixed door, is plum on both). Two choices sit side by side on both (the race). A ranking shown as a row reads left to right with 1st place on the right (the Scores row); vertical lists (RoundEnd, Results) keep 1st at the top | The TV's fanned topic cards (0017, `TopicVoteView`), the phone's stacked topic buttons and true or false halves (06), the Scores row with 1st place on the left (0017) |
| D129 | Race answer order | "Правда / True" on the left in green, "Ложь / False" on the right in red, on the phone's buttons and as a pair on the TV's statement card while players answer. On RaceReveal the pair gives way to the stamp with the right answer, as before | The phone's True on top, False below |

Code: `phone/src/style.css` (`.door-list`, `.btn.door`, `.tf-buttons`), `phone/src/screens/AnswerTrueFalseScreen.tsx`, `Presentation/TopicVoteView.cs`, `TopicDoor.cs` (voters in one row on the wide cards), `LeaderboardView.cs` (`X`), `RaceView.cs` (the True and False pair).
