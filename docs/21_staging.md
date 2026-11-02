# 21 Staging: the TV as a late night show

Status: Draft for prototyping (Nov 2, 2026; `decisions/0004_staging.md`, answers in `decisions/0005_staging_answers.md`, art direction in `decisions/0006_art_direction.md`). The final race staging is decided after game night 1. The character roster is a draft waiting for approval. The look itself (materials, palette, fonts) is written up later in `10_visual_style.md`; game night 1 gets at most a grey box version.

**Every duration here is a suggested starting point to tune in playtests, not a hard rule.** Each one is a config key under `staging` in `content/config.json` (table in `03_rules.md`), or an existing phase timer. The column "Test" marks the numbers that are pure guesses (**guess**) and those that follow from an existing timer (**timer**).

## The concept

The TV shows a 3D late night talk show set. The players are the guests, each playing a character they picked, two hosts run the show from the desk, and a small live band plays the music cues.

- **Set:** a guest area with a sofa and armchairs, the host desk next to it, a curtain stage with stage lamps, and a band riser with drums, bass, keys and, if the budget allows, guitar. An evening city view behind the windows.
- **Original design:** inspired by Russian late night TV, never a copy. No real show's name, logo, floor projection, signature set piece or color scheme. Real show stills never go into the repo (D59); only original generated images go into `docs/moodboard/`.
- **Look:** a realistic world with cartoon guests (D62). The set, props, lights and hosts are realistic: physically based materials at real world scale, never plastic, glossy or toy like; matte, slightly textured wood, fabric and a worn wooden floor; warm practical lights (desk lamps, pendants, stage lamps, bulb strips), soft shadows, and a cinematic TV broadcast feel (light film grain and vignette, shallow depth of field on close shots), as far as the laptop runs it smoothly.
- **Guests:** the one exception. They are cartoonish and may be mixed media: 2D billboards in the 3D space, or funny looking cartoonish 3D models. The clash is the joke: they look like they walked in from a cartoon, but the set's lights light them and they cast shadows on it, so they sit in the scene instead of floating on top of it.
- **Overlay:** all game information (question, options, timer, leaderboard, race track, room code) stays flat UGUI over the 3D camera (`13_architecture.md`). The 3D scene frames it. The only information a player reads in 3D is the name tags and the answer cards.

## Camera shots

| Shot | Frames | Used in |
| --- | --- | --- |
| `Establish` | The whole studio from high at the back, slowly booming down | Intro start |
| `Wide` | Guest area and desk; every seat and its name tag visible | Lobby, StartCountdown, Reveal, Scores, RoundEnd, FinalIntro, the race, Results, Poll |
| `Desk` | Both hosts at the desk, medium shot | Intro host lines, Results host line |
| `Curtain` | The closed curtain and the stage lamps, medium wide | TopicVote |
| `Stage` | The stage opening, framed so the question panel fills it | QuestionShow, Answer |
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

The phase timers in `02_show_flow.md` stay as they are. In game night 1 every `staging.extraSeconds` value is 0, so staging adds no time and the 11 s idle check still holds. Both are starting values (D56).

| Phase | Shot | What moves | Music cue | Overlay | Suggested durations (config key) | Test |
| --- | --- | --- | --- | --- | --- | --- |
| Lobby | `Wide`, slow drift | When a player joins, their character makes its signature entrance into a free seat, starting with a small pop sound, and gets a name tag; a host turns to them and greets them (`playerJoined` line). A player who picks another character swaps: the old one leaves with a pop, the new one makes its entrance. When a player leaves, their character stands up and walks off. A disconnected player's character turns grey | Lobby loop | Room code, QR code, "Waiting for players" | Swap pop 0.5 s (`staging.lobby.seatPopSeconds`); entrances about 1.5 s, no timer in the Lobby | guess |
| StartCountdown | `Wide` | Guests sit up; a host says a short `startCountdown` line; the drummer counts in with the sticks | Lobby loop ends on the count | Big 3, 2, 1 | 3 s (`lobby.startCountdownSeconds`) | timer |
| Intro | `Establish`, then `Desk`, then `Band`, then `Wide` | Boom down over the studio; the hosts say their `gameStart` lines at the desk; a short band cutaway; guests wave | Show open played over the lobby loop | Title card, then "Who wins tonight?" with prediction counts | Establishing shot 6 s (`staging.intro.establishSeconds`); the rest follows the host lines inside `timers.intro` (30 s) | guess |
| TopicVote | `Curtain` | Curtain closed; stage lamps sweep slowly over it | Lobby loop, quieter | Four topic cards in front of the curtain with live votes (voters' character chips) | 10 s (`timers.topicVote`) | timer |
| QuestionShow | Cut to `Stage` | The curtains open; the stage lamps swing onto the opening; the question panel appears full screen on the stage, lit by the lamps | The band stops on one held chord | Question text, read time bar | Curtain opens in 1.2 s (`staging.questionShow.curtainOpenSeconds`); the question fades in within 0.4 s (`staging.questionShow.questionInSeconds`), so it is readable for almost the whole 4 s | guess |
| Answer | `Stage`, slow push in | Lamps brighten in the last seconds | Suspense loop; it rises in the last 5 s (`staging.answer.suspenseRiseSeconds`, see below) | Question, four options (or the slider prompt), countdown, character chips popping as players lock in, power up icons | Answer window as in `03_rules.md` | timer |
| Reveal | Cut to `Wide` | Every guest raises a card with their answer (A to D, or their number for closest value), each in their character's own way. Right answers play the character's happy reaction, wrong ones the sad reaction, no answer shrugs with an empty card. The speaking host reacts | One stinger as the cards go up (see "Which stinger") | Correct answer banner; points above each guest who scored; the closest guess marked on closest value | Cards up in 0.5 s (`staging.reveal.cardsUpSeconds`), reactions 1.5 s (`staging.reveal.reactionSeconds`), then hold until the end of the 3.5 s | guess |
| Scores | `Wide`, no cut | Cards go down; guests settle | Stinger tail, then the lobby loop quietly | The leaderboard as a flat overlay slides in; rows move to their new ranks | Overlay in 0.4 s (`staging.scores.overlayInSeconds`) inside 3.5 s | guess |
| RoundEnd | `Wide` | The round winner stands up and waves; the others clap | Round end cue | Round leaderboard and round winner | 4 s (`timers.roundEnd`) | timer |
| FinalIntro | `Wide` | Guests sit up | Final race cue starts | Flat race track overlay with head starts, one line of rules | 8 s (`timers.finalIntro`) | timer |
| RaceQuestion | `Wide` | Guests react with their happy or sad reaction on each race reveal | Final race loop | Statement, flat race track overlay | `race.answer`, `race.reveal` | timer |
| Results | `Wide`, then `Band`, then `Desk` | The winner walks to the center of the floor under a spotlight and plays their victory; the others applaud | Winner cue, then the lobby loop | Winner, top 3, two awards | Winner walk 3 s (`staging.results.winnerWalkSeconds`) inside 20 s | guess |
| Poll | `Wide` | Guests look at their phones | Lobby loop, quiet | "Fill in the poll on your phone", count of submitted polls | | |

**Between questions** the curtain closes off camera at the start of Reveal, so every QuestionShow opens it again without costing time.

**Suspense rise:** the riser starts `staging.answer.suspenseRiseSeconds` before the latest personal deadline among the connected players who have not locked in. When last call starts, the riser starts at once (last call is also 5 s). If everyone locks in earlier, the loop cuts straight to the Reveal stinger.

**Which stinger** (D53), counting only the players who answered: everyone right plays the everyone right stinger; nobody right, or nobody answered, plays the nobody right stinger; otherwise the right stinger when at least half were right, the wrong stinger when fewer were.

**Making a phase longer:** staging may lengthen a phase when that makes the show feel better, only through `staging.extraSeconds.questionShow`, `.reveal`, `.scores` and `.roundEnd`. The core adds the value to that phase's budget; Presentation never waits for an animation. `phaseStarted` logs the budget with the extra included and the extra itself (`stagingExtraMs`), and `phaseEnded` logs how long the phase really took (`15_schemas.md`), so playtests show the cost in waiting. All four start at 0 for game night 1. After the first playtest they may go above 0 (D56); the 11 s idle limit is a starting value too and is set again together with them.

### Seats

Eight seats for `players.max` = 8: a four seat sofa in the middle and two armchairs on each side, in a shallow arc facing `Wide`, with the desk at one end. A new player takes the first free seat in this order: the two middle sofa seats, the two outer sofa seats, the inner armchairs, the outer armchairs. Two to four players therefore sit together on the sofa. Seat positions are markers in the scene, not config numbers. Players keep their seat through Play again and Back to lobby, like their character.

## Characters

### Guests: a cast to pick from

The guests are a cast of 16 distinct characters (D55, D61), like the contestants in Knowledge is Power: each player picks one on the phone (`pickCharacter`), and each character can be taken by one player at a time. They are personas, not people: no real names, no nationality and no age; unserious and not realistic (a robot, a ghost, a self made superhero); and as different from each other as possible in silhouette, color and temperament (D60). The persona drives every signature animation and emotion. The roster lives in `content/characters.json` (schema in `15_schemas.md`). **The roster below is a draft for Ilia to approve.**

Each character has its own silhouette and is one of three media, chosen per character later (D62, D64; until then testing uses grey 3D boxes):

- **3D model:** cartoonish proportions, rigged as a Unity Humanoid so the shared animations retarget onto it.
- **2D billboard:** a flat drawn character on a card in the 3D space that turns to face the active camera. Every shared and signature animation is drawn for it (15 per character), as flipbook frames or a cutout rig, decided only if a character becomes 2D (D64).
- **Mixed:** for example a 3D body with a drawn 2D face.

However unrealistic, each one has a head, two arms and a way to sit (the ghost hovers over its seat with its legs hidden). Playful, never mocking appearance, ethnicity, weight or disability.

**Name tag:** the character's name, with the player's name under it in smaller text.

**Shared animations** (one set, retargeted onto every 3D character and drawn for every 2D one):

| Animation | When | Plays |
| --- | --- | --- |
| Stand up and walk off | Leave | Once |
| Wave | Intro, round winner | Once |
| Lock in nod | The player locks in an answer | Once, short |
| Lower card | Scores | Once |
| Shrug with an empty card | No answer | Once |
| Hit flinch | A power up lands on this player | Once |
| Applaud | Someone else wins a round or the game | Loop |
| Look at phone | Poll | Loop |
| Walk to center | Game winner in Results, before their victory | Once |
| Away (grey material, no animation) | Disconnected | |

**Signature animations** (six per character, 96 in all), each played the way the persona would: an entrance (on join and on a character swap, about 1.5 s; the Lobby has no timer), an idle pose (loop), a happy and a sad reaction (once each, about 1.5 s to fit `staging.reveal.reactionSeconds`), the way they raise and hold their answer card (raise once, then a hold loop), and a victory (game winner in Results, then loop).

**Draft roster** (`content/characters.json`; looks are in the file):

| Id | Character | Color | Persona | Entrance | Idle pose (loop) | Happy | Sad | Card hold | Victory |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 0 | The Robot (Робот) | `#5B9BD5` | Runs on facts. Glitches when it's wrong. | Rolls in on hidden wheels, unfolds into the seat with whirs and clicks | Screen face blinks, the antenna bulb pulses, small whirring calculations | Antenna lights up, a stiff robot dance with both arms | Screen shows an error, a few sparks, powers down and slumps, then reboots | The card pops out of a slot in its chest; it holds it up on a telescoping arm | Spins its head all the way round and fires a confetti cannon from its chest |
| 1 | The Diva (Дива) | `#E57A9A` | Sure every question is about her. | Sweeps in blowing kisses to the audience and settles into the seat with a flick of the boa | Checks her reflection in the hand mirror and fluffs her hair | Deep bow, throws invisible roses to the audience | Faints dramatically onto the armrest, the back of a hand on her forehead | Holds it like an award, the other hand on her heart | Accepts invisible flowers and gives a tearful thank you speech |
| 2 | The Ghost (Призрак) | `#DDE3EA` | Tries so hard to be scary. Nobody is scared. | Rises through the seat with a big BOO; nobody reacts | Bobs up and down, rattles the tiny chain, practises scary faces | A spooky laugh and a loop in the air | Deflates into a puddle of sheet, then slowly floats back up | The card floats up beside it, wobbling spookily | Splits into three ghosts that all shout boo at once, then back into one |
| 3 | The Caveman (Пещерный человек) | `#C08457` | Never read a book. Trusts the gut. | Stomps in dragging the club, sniffs the seat, then sits | Gnaws a bone, scratches the head, sniffs the air | Beats the chest and roars | Bonks their own head gently with the club; little stars circle | Chisels the answer onto a stone tablet and lifts it overhead | Discovers fire: a tiny flame on the club, a dance around it |
| 4 | The Superhero (Супергерой) | `#E0533D` | Has a cape. Has no superpowers. | Runs in with both arms forward, flying, and trips into the seat | Strikes heroic poses while the cape flutters with no wind | Leaps up into the classic flying pose | The cape flops over the whole face | Thrusts it toward the sky with a clenched fist | Rips open the sweater to show another sweater with a bigger lightning bolt |
| 5 | The Wizard (Волшебник) | `#8E7CC3` | Sees answers in a crystal ball. Mostly wrong. | Appears in a puff of smoke, coughing | Rubs the crystal ball, which glows faintly | The wand shoots a small firework | The wand fizzles and the hat droops down over the whole face | Conjures it out of thin air with a flick of the wand | Makes a swirl of little stars rain from the hat |
| 6 | The Chef (Шеф) | `#FF8A3D` | Treats every question like a recipe. Hot tempered. | Bursts in through invisible swing doors carrying a tray | Tastes from the ladle and adds an invisible pinch of salt | Chef's kiss, then tosses the hat into the air | Steam puffs from the ears; slaps the towel onto the shoulder | Serves it on a silver tray, lifting the lid | A big harmless flambé whoosh from a pan, then a bow |
| 7 | The Sleepyhead (Соня) | `#8DB255` | Always half asleep. Sometimes a genius. | Shuffles in yawning and falls asleep the moment they sit | Dozes; a snore bubble grows and pops | Wakes with a start, a sleepy thumbs up, nods off again | Yawns and pulls the pillow over the head | Holds it up with one floppy arm, eyes still closed | Wide awake for one second, cheers, then falls asleep standing up |
| 8 | The Champ (Чемпион) | `#F2C14E` | Fights every question. Gloves never come off. | Walks in like a fighter: hood up, bouncing, punching the air | Bounces in the seat and shadowboxes | Gloves up, a champion's strut | A dramatic knockout fall; little birds circle the head | Clamps it between both gloves | Lifts an invisible title belt and runs a lap of honour |
| 9 | The Spy (Шпион) | `#4FA3A5` | Trusts no one. Whispers every answer. | Rolls in sideways like in a spy film and checks the seat for bugs | Peeks over the newspaper through the eye holes, looks left and right | A smug collar adjust; the fake moustache flies off | Slides down behind the newspaper until only the hat shows | Flashes it like a badge for a moment, then hides it | Rips off the disguise to reveal an identical disguise underneath |
| 10 | The Gentleman (Джентльмен) | `#D2B48C` | Impeccable manners. Apologises for wrong answers. | Tips the bowler to everyone, dusts the seat with a handkerchief, sits with legs crossed | Sips tea with the little finger raised, checks a pocket watch | A restrained golf clap; the monocle gleams | The monocle pops out and plops into the tea | Draws it from the breast pocket and presents it with a slight bow | One wild jolly good fist pump, then straightens the bow tie, embarrassed |
| 11 | The Bombshell (Красотка) | `#F25CC6` | Too gorgeous to try. Winks at every answer. | Struts in in slow motion while a wind machine blows her hair, sits with a hair flip | Blows a bubble gum bubble, files her nails, glances at the camera | Lowers the heart sunglasses and winks; a sparkle pings off her smile | The gum bubble pops all over her face | Holds it up between two fingers without even looking | A slow motion hair flip in front of the wind machine, then finger guns |
| 12 | The Introvert (Интроверт) | `#9AA8BA` | Knows every answer. Would rather not say it. | Edges in along the wall and perches on the very edge of the seat | Reads the book, peeks at the others over it, looks away fast | A tiny fist pump, hidden behind the book | Pulls the hood strings tight until only the nose shows | Slides it up from behind the book, barely visible | The spotlight hits; hides behind the book, then peeks out with a tiny wave |
| 13 | The Emo (Эмо) | `#B784F0` | Nobody understands. Not even the questions. | Drags in under the personal rain cloud and slumps into the seat | Blows the fringe out of the eye; it falls straight back | Almost smiles, catches itself; the cloud lets one ray of sun through | A content sigh, finally at home; the rain gets heavier | Tears the answer out of the poem notebook and holds it up limply | The cloud turns into a rainbow; horrified, pushes it away and pulls the cloud back |
| 14 | The Alien (Пришелец) | `#6EE07A` | Studying Earth. Gets Earth very wrong. | Beamed down in a tractor beam, lands in the seat upside down, turns itself the right way up | Scans the other guests and the furniture with the beeping gadget | Copies an Earth celebration wrong: claps with its feet | The eyes droop on their stalks; it phones home on the gadget | Holds the card upside down, then turns it round | The tractor beam lifts it up spinning; it plants a tiny flag on the seat |
| 15 | The Mime (Мим) | `#7FDBE8` | Never says a word. Lives in an invisible box. | Walks in against an invisible wind, sits on an invisible chair, then on the real one | Feels the walls of an invisible box | A silent belly laugh, slapping the knee without a sound | An invisible rope pulls it down into the seat; wipes the painted tear | Pulls the card out of the air on an invisible rope and holds it up | Climbs an invisible ladder and waves from the top |

### Hosts

Two unique characters based on Konstantin and Ilja, in the realistic style of the set, not the cartoon style of the guests (D62, D63). Both sit at the desk (D52).

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
| Hit | The four Reveal stingers, round end | Once |
| Count in (drums only) | StartCountdown | Once |
| Celebrate | Winner, everyone right | Once |

## Music cues for the band

All lengths are suggestions (**guess**). Every cue is played by the band on screen. For the prototype every cue is a free placeholder loop or stinger; sources and licenses are in `12_audio.md`, and the final music is decided later (D57). Game night 1 has no music.

| Cue | Where | Suggested length | Mood |
| --- | --- | --- | --- |
| Lobby loop | Lobby, TopicVote (quieter), Scores (quiet), Poll, and under Results after the winner cue | 60 to 90 s, seamless loop | Warm late night groove: keys, bass, brushed drums, about 100 BPM |
| Suspense loop with a rising last 5 s | Answer | A 16 s loop, plus a separate 5 s riser that ends exactly on the deadline | Low pulse and a ticking hi hat; the riser adds drum rolls and a rising bass line |
| Right stinger | Reveal: at least half of those who answered were right | 1.5 to 2 s | Bright and major, a short brass or keys hit |
| Wrong stinger | Reveal: fewer than half were right | 1.5 to 2 s | Comic and gentle (a falling trombone line), never mean |
| Everyone right stinger | Reveal: everyone who answered was right | 2 to 3 s | A bigger version of the right stinger: full band hit and a cymbal swell |
| Nobody right stinger | Reveal: nobody right, or nobody answered | 2 to 3 s | A comic full band "wah wah" ending on a drum rimshot, still friendly |
| Round end | RoundEnd | 4 s, matching `timers.roundEnd` | A turnaround with a small fanfare |
| Final race | FinalIntro and the race | 30 to 60 s loop, about 140 BPM, with a more intense layer near the finish | Driving and urgent |
| Winner | Results | 8 to 12 s | A big celebratory finish, then back to the lobby loop |

The Intro "show open" is the lobby loop with a short fanfare on top; it needs no separate cue unless playtests ask for one. The pop when a guest appears and the curtain sound are sound effects, not band cues; they go into the sound list in `12_audio.md`.

## Final race

Decided after game night 1 (D54). Until then the race is the flat track overlay over the `Wide` shot: guests stay in their seats and react on each race reveal. Ideas for later:

1. **The floor is the track:** guests leave their seats and line up at the back of the floor; each correct answer moves them one step toward the stage; the first to reach the stage wins.
2. **Floor light strips:** guests stay seated; a strip of floor lights in front of each seat lights up one step at a time, like a level meter.
3. **Race on the stage:** the curtain opens on a painted race track; small stand ins of the characters move along it.
4. **Band pressure:** the tempo of the final race cue rises with the leader's step; the drummer rolls when someone is one step from the finish.
5. **Hosts as commentators:** the hosts stand at the desk and turn to follow the leader, with lines when the lead changes.

## Ideas for later

- **Sidekick by the band:** one host moves from the desk to a spot by the band for some segments, as a sidekick who heckles and cues the band. For now both hosts sit at the desk (D52).
- **Medium per character (D64):** which characters are 3D, 2D billboards or mixed. Then a `medium` field goes into `content/characters.json` and its schema in `15_schemas.md`.
- **2D animation (D64), only if a character becomes 2D:** plain flipbook frames (no package), or a cutout rig with Unity's 2D Animation package (a new package needs asking first).

## Grey box version (game night 1 stretch)

Only after every Day 4 check in `19_acceptance.md` passes.

- **Scene:** a box stage with an opening, a box desk, and 8 box chairs in the seat order above. Each seated player is a capsule in their character's signature color, with the character's name above it and the player's name under it. No curtains, hosts, band, lights work or music.
- **Cameras:** two plain Unity cameras, `Stage` and `Wide`, switched by a listener in Presentation (`StagingDirector`) on phase events: `Stage` for QuestionShow and Answer, `Wide` for every other phase, with the flat race track overlay over `Wide` in the race. No Cinemachine (a new package needs asking first).
- **Reveal:** a colored card above each capsule with its answer: the letter in its option color from the TV and phone for multiple choice, the number for closest value, an empty card for no answer. The cards hide when Scores ends.
- **Timing:** the overlay UI stays as it is. Every `staging.extraSeconds` value stays 0, so all timing checks still pass.

## Open questions

1. Approve or change the draft roster in `content/characters.json`.
2. Should the idle limit move into `content/config.json` like the timers, so tests read it from there instead of a number in the test?
