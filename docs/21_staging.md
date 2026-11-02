# 21 Staging: the TV as a late night show

Status: Draft for prototyping (Nov 2, 2026; `decisions/0004_staging.md`). The final race is still open. The look itself (materials, palette, fonts) is written up later in `10_visual_style.md`; game night 1 gets at most a grey box version.

**Every duration here is a suggested starting point to tune in playtests, not a hard rule.** Each one is a config key under `staging` in `content/config.json` (table in `03_rules.md`), or an existing phase timer. The column "Test" marks the numbers that are pure guesses (**guess**) and those that follow from an existing timer (**timer**).

## The concept

The TV shows a 3D late night talk show set. The players are the guests, two hosts run the show from the desk, and a small live band plays the music cues.

- **Set:** a guest area with a sofa and armchairs, the host desk next to it, a curtain stage with stage lamps, and a band riser with drums, bass, keys and, if the budget allows, guitar. An evening city view behind the windows.
- **Original design:** inspired by Russian late night TV, never a copy. No real show's name, logo, floor projection, signature set piece or color scheme. The moodboard stills are references for mood and lighting only.
- **Look:** not plastic, not glossy, not toy like. Matte, slightly textured materials (wood, fabric, a worn wooden floor), warm practical lights (desk lamps, pendants, stage lamps, bulb strips), soft shadows, a cinematic TV broadcast feel (light film grain and vignette, shallow depth of field on close shots). Characters are stylized but with believable proportions and fabrics.
- **Overlay:** all game information (question, options, timer, leaderboard, room code) stays flat UGUI over the 3D camera (`13_architecture.md`). The 3D scene frames it. The only information a player reads in 3D is the name tags and the answer cards.

## Camera shots

| Shot | Frames | Used in |
| --- | --- | --- |
| `Establish` | The whole studio from high at the back, slowly booming down | Intro start |
| `Wide` | Guest area and desk; every seat and its name tag visible | Lobby, StartCountdown, Reveal, Scores, RoundEnd, Results, Poll, PlayAgain |
| `Desk` | Both hosts at the desk, medium shot | Intro host lines, Results host line |
| `Curtain` | The closed curtain and the stage lamps, medium wide | TopicVote |
| `Stage` | The stage opening, framed so the question panel fills it | QuestionShow, Answer, FinalIntro, the race (until its staging is decided) |
| `Band` | The band on its riser | Short cutaways in Intro and Results only |

### Camera rules

1. **Shots follow phases; staging never holds a phase.** The Presentation layer only listens to events (`CLAUDE.md`). When a phase ends, the next shot cuts in even if an animation is still running; that animation jumps to its end pose. Only the core can make a phase longer, and only through the config keys under `staging.extraSeconds`.
2. **Hard cuts** between shots. Moves inside a shot are slow (a push in or a drift of a few percent of the frame), never a whip pan or a shake.
3. **No cut during Answer.** The question and options stay in the same place on screen for the whole window.
4. **Every guest stays in frame** in `Wide`, with the name tag and, on Reveal, the card readable from the sofa at 3 m.
5. **Host lines never force a cut** during Reveal and Scores. The speaking host turns toward the guests; when no host is in the shot, the bubble shows with the placeholder portrait as in game night 1.
6. The `Band` shot is a cutaway only where no player needs information (Intro, Results).
7. A shot lasts at least about 1.5 s, unless a phase that ends early forces the cut (**guess**).

## Shot list per phase

The phase timers in `02_show_flow.md` stay as they are. In game night 1 every `staging.extraSeconds` value is 0, so staging adds no time and the 11 s idle check (D42) still holds.

| Phase | Shot | What moves | Music cue | Overlay | Suggested durations (config key) | Test |
| --- | --- | --- | --- | --- | --- | --- |
| Lobby | `Wide`, slow drift | When a player joins, a guest pops into a free seat with a small pop sound and a name tag; a host turns to them and greets them (`playerJoined` line). When a player leaves, their guest stands up and walks off. A disconnected player's guest turns grey | Lobby loop | Room code, QR code, "Waiting for players" | Pop in 0.5 s (`staging.lobby.seatPopSeconds`) | guess |
| StartCountdown | `Wide` | Guests sit up; the drummer counts in with the sticks | Lobby loop ends on the count | Big 3, 2, 1 | 3 s (`lobby.startCountdownSeconds`) | timer |
| Intro | `Establish`, then `Desk`, then `Band`, then `Wide` | Boom down over the studio; the hosts say their `gameStart` lines at the desk (the first one is the opening line); a short band cutaway; guests wave | Show open played over the lobby loop | Title card, then "Who wins tonight?" with prediction counts | Establishing shot 6 s (`staging.intro.establishSeconds`); the rest follows the host lines inside `timers.intro` (30 s) | guess |
| TopicVote | `Curtain` | Curtain closed; stage lamps sweep slowly over it | Lobby loop, quieter | Four topic cards in front of the curtain with live votes (voters' avatar badges) | 10 s (`timers.topicVote`) | timer |
| QuestionShow | Cut to `Stage` | The curtains open; the stage lamps swing onto the opening; the question panel appears full screen on the stage, lit by the lamps | The band stops on one held chord | Question text, read time bar | Curtain opens in 1.2 s (`staging.questionShow.curtainOpenSeconds`); the question fades in within 0.4 s (`staging.questionShow.questionInSeconds`), so it is readable for almost the whole 4 s | guess |
| Answer | `Stage`, slow push in | Lamps brighten in the last seconds | Suspense loop; it rises in the last 5 s (`staging.answer.suspenseRiseSeconds`, see below) | Question, four options (or the slider prompt), countdown, avatar badges popping as players lock in, power up icons | Answer window as in `03_rules.md` | timer |
| Reveal | Cut to `Wide` | Every guest raises a card with their answer: A to D, or their number for closest value. Right answers react happily, wrong ones slump, no answer shrugs with an empty card. The speaking host reacts | Right or wrong stinger as the cards go up | Correct answer banner; points above each guest who scored; the closest guess marked on closest value | Cards up in 0.5 s (`staging.reveal.cardsUpSeconds`), reactions 1.5 s (`staging.reveal.reactionSeconds`), then hold until the end of the 3.5 s | guess |
| Scores | `Wide`, no cut | Cards go down; guests settle | Stinger tail, then the lobby loop quietly | The leaderboard as a flat overlay slides in; rows move to their new ranks | Overlay in 0.4 s (`staging.scores.overlayInSeconds`) inside 3.5 s | guess |
| RoundEnd | `Wide` | The round winner stands up and waves; the others clap | Round end cue | Round leaderboard and round winner | 4 s (`timers.roundEnd`) | timer |
| FinalIntro | `Stage` (placeholder) | Curtains open on the race | Final race cue starts | Race track with head starts, one line of rules | 8 s (`timers.finalIntro`) | open, see "Final race" |
| RaceQuestion | `Stage` (placeholder) | | Final race loop | Statement, track | `race.answer`, `race.reveal` | open |
| Results | `Wide`, then `Band`, then `Desk` | The winner walks to the center of the floor under a spotlight; the others applaud | Winner cue, then the lobby loop | Winner, top 3, two awards | Winner walk 3 s (`staging.results.winnerWalkSeconds`) inside 20 s | guess |
| Poll | `Wide` | Guests look at their phones | Lobby loop, quiet | "Fill in the poll on your phone", count of submitted polls | | |
| PlayAgain | `Wide` | Idle | Lobby loop | Final standings, "Play again?" | | |

**Between questions** the curtain closes off camera at the start of Reveal, so every QuestionShow opens it again without costing time.

**Suspense rise:** the riser starts `staging.answer.suspenseRiseSeconds` before the latest personal deadline among the connected players who have not locked in. When last call starts, the riser starts at once (last call is also 5 s). If everyone locks in earlier, the loop cuts straight to the Reveal stinger.

**Which stinger:** right when at least half of the players who answered got it right, wrong otherwise, and wrong when nobody answered (**guess**).

**Making a phase longer:** staging may lengthen a phase when that makes the show feel better, only through `staging.extraSeconds.questionShow`, `.reveal`, `.scores` and `.roundEnd`. The core adds the value to that phase's budget; Presentation never waits for an animation. `phaseStarted` logs the budget with the extra included and the extra itself (`stagingExtraMs`), and `phaseEnded` logs how long the phase really took (`15_schemas.md`), so playtests show the cost in waiting. All four are 0 for game night 1. Raising one above 0 makes a stretch without input longer than the 11 s limit of D42, so it needs a decision first.

### Seats

Eight seats for `players.max` = 8: a four seat sofa in the middle and two armchairs on each side, in a shallow arc facing `Wide`, with the desk at one end. A new player takes the first free seat in this order: the two middle sofa seats, the two outer sofa seats, the inner armchairs, the outer armchairs. Two to four players therefore sit together on the sofa (**guess**). Seat positions are markers in the scene, not config numbers. Players keep their seat through Play again and Back to lobby, like their avatar.

## Characters

### Guests (players)

One base guest character, rigged as a Unity Humanoid so bought or generated animations retarget onto it. Up to 8 on screen at once.

- **Variants:** the outfit's main color is the player's avatar color (`avatars.json`); the accessory follows the avatar shape, one accessory per shape (for example a cap, glasses, a scarf, headphones, a bow tie, a beanie). The name tag shows the avatar badge and the name, so the phone and the guest match (**guess**). A later choice of body type for kids and grandparents would need a phone screen and a protocol change.

| Animation | When | Plays |
| --- | --- | --- |
| Sit idle (2 or 3 variants, offset per guest) | Always while seated | Loop |
| Pop in and sit | Join | Once, 0.5 s |
| Stand up and walk off | Leave | Once |
| Wave | Intro, round winner | Once |
| Lock in nod | The player locks in an answer | Once, short |
| Raise card, hold card, lower card | Reveal, Scores | Once, loop, once |
| Happy reaction (2 variants: fist pump, clap) | Right answer | Once |
| Slump (2 variants: facepalm, sag) | Wrong answer | Once |
| Shrug | No answer | Once |
| Hit flinch | A power up lands on this player | Once |
| Applaud | Someone else wins a round or the game | Loop |
| Look at phone | Poll | Loop |
| Walk to center and celebrate | Game winner in Results | Once, then loop |
| Away (grey material, no animation) | Disconnected | |

### Hosts

Two unique characters based on Konstantin and Ilja, with the same proportions and material rules as the guests. Both sit at the desk (**guess**, see open questions).

| Animation | When | Plays |
| --- | --- | --- |
| Desk idle | Always | Loop |
| Talk (3 variants: neutral, laughing, pointing at the guests), with mouth movement while the bubble shows | Any host line | Loop while the bubble shows |
| Look at a guest | `playerJoined`, any line with `{name}` | Procedural (look at) |
| Greet wave | `playerJoined` | Once |
| React happy, react mock dismay | Reveal lines | Once |
| Applaud | RoundEnd, Results | Loop |
| Stand and present | Intro, FinalIntro | Once |

### Band

Four members on the riser: drums, bass, keys, and guitar if the budget allows (cut first). Each member needs looped animations that match the tempo of the cue they play.

| Animation | Cue | Plays |
| --- | --- | --- |
| Idle (hands resting, small sway) | Silence | Loop |
| Play calm | Lobby loop | Loop |
| Play tense | Suspense loop | Loop |
| Play intense | Suspense riser, final race | Loop |
| Hit | Right and wrong stingers, round end | Once |
| Count in (drums only) | StartCountdown | Once |
| Celebrate | Winner | Once |

## Music cues for the band

All lengths are suggestions (**guess**). Every cue is played by the band on screen. Composer, licenses and mixing rules go into `12_audio.md` later.

| Cue | Where | Suggested length | Mood |
| --- | --- | --- | --- |
| Lobby loop | Lobby, TopicVote (quieter), Scores (quiet), Poll, PlayAgain, and under Results after the winner cue | 60 to 90 s, seamless loop | Warm late night groove: keys, bass, brushed drums, about 100 BPM |
| Suspense loop with a rising last 5 s | Answer | A 16 s loop, plus a separate 5 s riser that ends exactly on the deadline | Low pulse and a ticking hi hat; the riser adds drum rolls and a rising bass line |
| Right stinger | Reveal | 1.5 to 2 s | Bright and major, a short brass or keys hit |
| Wrong stinger | Reveal | 1.5 to 2 s | Comic and gentle (a falling trombone line), never mean |
| Round end | RoundEnd | 4 s, matching `timers.roundEnd` | A turnaround with a small fanfare |
| Final race | FinalIntro and the race | 30 to 60 s loop, about 140 BPM, with a more intense layer near the finish | Driving and urgent |
| Winner | Results | 8 to 12 s | A big celebratory finish, then back to the lobby loop |

The Intro "show open" is the lobby loop with a short fanfare on top; it needs no separate cue unless playtests ask for one. The pop when a guest appears and the curtain sound are sound effects, not band cues; they go into the sound list in `12_audio.md`.

## Final race (open)

Until this is decided, the race uses the flat track overlay over the `Stage` shot. Ideas to choose from:

1. **The floor is the track:** guests leave their seats and line up at the back of the floor; each correct answer moves them one step toward the stage; the first to reach the stage wins.
2. **Floor light strips:** guests stay seated; a strip of floor lights in front of each seat lights up one step at a time, like a level meter.
3. **Race on the stage:** the curtain opens on a painted race track; small stand ins of the guests move along it.
4. **Band pressure:** the tempo of the final race cue rises with the leader's step; the drummer rolls when someone is one step from the finish.
5. **Hosts as commentators:** the hosts stand at the desk and turn to follow the leader, with lines when the lead changes.

## Grey box version (game night 1 stretch)

Only after every Day 4 check in `19_acceptance.md` passes.

- **Scene:** a box stage with an opening, a box desk, and 8 box chairs in the seat order above. Each seated player is a capsule in their avatar color with their name tag above it. No curtains, hosts, band, lights work or music.
- **Cameras:** two plain Unity cameras, `Stage` and `Wide`, switched by a listener in Presentation (`StagingDirector`) on phase events: `Stage` for QuestionShow, Answer, FinalIntro and the race; `Wide` for every other phase. No Cinemachine (a new package needs asking first).
- **Reveal:** a colored card above each capsule with its answer: the letter in its option color from the TV and phone for multiple choice, the number for closest value, an empty card for no answer. The cards hide when Scores ends.
- **Timing:** the overlay UI stays as it is. Every `staging.extraSeconds` value stays 0, so all timing checks still pass.

## Open questions

1. Where is the second host: both at the desk, or one by the band as a sidekick?
2. Stinger rule: one stinger for the whole group (half right or more), or follow the leader, or play none when the result is mixed?
3. Which final race idea, or which mix?
4. One guest body for everyone, or a later choice of child, adult or grandparent body?
5. Should any `staging.extraSeconds` value rise above 0 after the first playtest, and how does that sit with the 11 s idle limit of D42?
6. Music: composed, commissioned or licensed?
