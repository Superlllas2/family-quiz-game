# 21 Staging: the TV as a late night show

Status: Draft for prototyping (Nov 2, 2026; `decisions/0004_staging.md`, answers in `decisions/0005_staging_answers.md`, art direction in `decisions/0006_art_direction.md`, game night 1 art in `decisions/0015_art_for_game_night_1.md`, the Seats shot, the question board and the set pieces in `decisions/0016_lobby_shot_question_board.md`). The final race staging is decided after game night 1. The character roster is a draft waiting for approval. The look itself (materials, palette, fonts) is written up later in `10_visual_style.md`; game night 1 gets the version at the end of this document (D89, D90).

**Every duration here is a suggested starting point to tune in playtests, not a hard rule.** Each one is a config key under `staging` in `content/config.json` (table in `03_rules.md`), or an existing phase timer. The column "Test" marks the numbers that are pure guesses (**guess**) and those that follow from an existing timer (**timer**).

## The concept

The TV shows a 3D late night talk show set. The players are the guests, each playing a character they picked, two hosts run the show from the desk, and a small live band plays the music cues.

- **Set:** a guest area with a sofa and armchairs, the host desk next to it, a curtain stage with stage lamps, and a band riser with drums, bass, keys and, if the budget allows, guitar. An evening city view behind the windows. The walls form a bay around the cameras (Ilia, Nov 4, 2026, from the reference pictures, so the studio feels compact and cosy): the guests and the desk along the window wall, a 45 degree turn to the curtain wall facing the audience, another 45 degree turn to the band wall. Seen from the audience, left to right: the guest area in front of the windows, the desk by the first corner, the curtain stage, the band riser. Since the studio rebuild (Ilia, Nov 5, 2026) the window wall stops just past the desk, the curtain stage is a deep apron across the whole curtain wall, right beside the hosts, and the band has its own big stage with an angled front on a longer band wall.
- **Question board:** behind the curtain stands a plain flat board, lit by the stage lamps (D92). It has no screen bezel and no frame of bulbs. The question is never drawn on it in 3D: the `Stage` shot frames the board, and the flat question overlay sits inside it.
- **Original design:** inspired by Russian late night TV, never a copy. No real show's name, logo, floor projection, signature set piece or color scheme. Real show stills never go into the repo (D59); only original generated images go into `docs/moodboard/`. The pieces seen in the reference footage are replaced like this (D93):

  | Seen in the reference footage | In our set |
  | --- | --- |
  | A teal curtain inside an arch of light bulbs | A deep, muted teal velvet curtain in a wooden arch with no bulbs, lit by plain stage lamps on stands (teal instead of deep green: Ilia, Nov 5, 2026) |
  | The show's logo projected on the floor | A worn wooden floor with rugs, no projection |
  | A recognizable Moscow skyline | An invented evening city |
  | A desk built from stacked wood blocks | A ribbed wooden desk with a warm light strip under the top |
  | A teal tufted sofa and teal armchairs | Muted dark green or brown fabric, so the characters' signature colors stand out against the set |
- **Look:** a realistic world with cartoon guests (D62). The set, props, lights and hosts are realistic: physically based materials at real world scale, never plastic, glossy or toy like; matte, slightly textured wood, fabric and a worn wooden floor; warm practical lights (desk lamps, pendants, stage lamps, bulb strips), soft shadows, and a cinematic TV broadcast feel (light film grain and vignette, shallow depth of field on close shots), as far as the laptop runs it smoothly.
- **Guests:** the one exception. They are cartoonish and may be mixed media: 2D billboards in the 3D space, or funny looking cartoonish 3D models. The clash is the joke: they look like they walked in from a cartoon, but the set's lights light them and they cast shadows on it, so they sit in the scene instead of floating on top of it.
- **Overlay:** all game information (question, options, timer, leaderboard, race track, room code) stays flat UGUI over the 3D camera (`13_architecture.md`). The 3D scene frames it. The only information a player reads in 3D is the name tags and the answer cards. In `Stage` the question overlay sits inside the question board, so it looks printed on the board but stays sharp.

## Camera shots

| Shot | Frames | Used in |
| --- | --- | --- |
| `Seats` | Only the 8 seats, close, every seat and its name tag visible. The rest of the studio is dark; only the lamps over the seats are on (D91) | Lobby, StartCountdown |
| `Establish` | The whole studio from high at the back, slowly booming down. The first time the players see the whole set: the studio lights come up as the shot starts (D91) | Intro start |
| `Wide` | Guest area and desk; every seat and its name tag visible | Reveal, Scores, RoundEnd, FinalIntro, the race, Results, Poll |
| `Desk` | Both hosts at the desk, medium shot | Intro host lines, Results host line |
| `HostSolo` | The speaking host alone at the desk, close; the other host out of frame (D97) | While a `gameStart` line shows in the Intro and while a `finalStart` line shows in FinalIntro: rule explanations and jokes |
| `Curtain` | The closed curtain and the stage lamps, medium wide | TopicVote |
| `Stage` | The stage opening, framed so the question board fills it and the question overlay sits inside the board with a margin (D92) | QuestionShow, Answer |
| `Band` | The band on its riser | Short cutaways in Intro and Results only |

### Camera rules

1. **Shots follow phases; staging never holds a phase.** The Presentation layer only listens to events (`CLAUDE.md`). When a phase ends, the next shot cuts in even if an animation is still running; that animation jumps to its end pose. Only the core can make a phase longer, and only through the config keys under `staging.extraSeconds`.
2. **Hard cuts** between shots. Moves inside a shot are slow (a push in or a drift of a few percent of the frame), never a whip pan or a shake.
3. **No cut during Answer.** The question and options stay in the same place on screen for the whole window.
4. **Every guest stays in frame** in `Seats` and `Wide`, with the name tag and, on Reveal, the card readable from the sofa at 3 m.
5. **Host lines never force a cut** during Reveal and Scores. The speaking host turns toward the guests; when no host is in the shot, the bubble shows with the placeholder portrait as in game night 1.
6. The `Band` shot is a cutaway only where no player needs information (Intro, Results).
7. **Host solo:** `HostSolo` cuts in when a `gameStart` or `finalStart` line starts and cuts back to the phase's shot when its bubble ends. It frames the host who speaks, so the two Intro lines cut from one host to the other, before the establishing shot (see "Game night 1 version"). It holds 0.25 s past the bubble so the second Intro line never flashes the studio between the hosts. Never in Answer, Reveal, Scores, TopicVote, QuestionShow, RoundEnd or Results: there the players need the guests, the cards or the stage. The overlay stays on top, so in FinalIntro the race track and its rules stay readable. A line that explains a rule says it out loud in the bubble; a dedicated rules trigger is an idea for later.
8. A shot lasts at least about 1.5 s, unless a phase that ends early forces the cut (**guess**).

## Shot list per phase

The phase timers in `02_show_flow.md` stay as they are. In game night 1 every `staging.extraSeconds` value is 0, so staging adds no time and the 11 s idle check still holds. Both are starting values (D56).

| Phase | Shot | What moves | Music cue | Overlay | Suggested durations (config key) | Test |
| --- | --- | --- | --- | --- | --- | --- |
| Lobby | `Seats`, slow drift; the studio around the seats stays dark | When a player joins, their character makes its signature entrance into a free seat, starting with a small pop sound, and gets a name tag; a host turns to them and greets them (`playerJoined` line). A player who picks another character swaps: the old one leaves with a pop, the new one makes its entrance. When a player leaves, their character stands up and walks off. A disconnected player's character turns grey | Lobby loop | Room code, QR code, "Waiting for players" | Swap pop 0.5 s (`staging.lobby.seatPopSeconds`); entrances about 1.5 s, no timer in the Lobby | guess |
| StartCountdown | `Seats`, still dark around the seats | Guests sit up; a host says a short `startCountdown` line; the drummer counts in with the sticks | Lobby loop ends on the count | Big 3, 2, 1 | 3 s (`lobby.startCountdownSeconds`) | timer |
| Intro | `Establish`, then `Desk`, then `Band`, then `Wide` | The studio lights come up and the camera booms down over the whole set for the first time; the hosts say their `gameStart` lines at the desk; a short band cutaway; guests wave | Show open played over the lobby loop | Title card, then "Who wins tonight?" with prediction counts | Establishing shot 6 s (`staging.intro.establishSeconds`), the lights come up over its first 1.5 s (`staging.intro.lightsUpSeconds`, D94); the rest follows the host lines inside `timers.intro` (30 s) | guess |
| TopicVote | `Curtain` | Curtain closed; stage lamps sweep slowly over it | Lobby loop, quieter | Four topic cards in front of the curtain with live votes (voters' character chips) | 10 s (`timers.topicVote`) | timer |
| QuestionShow | Cut to `Stage` | The curtains open on the question board; the stage lamps swing onto it; the question appears inside the board, as if lit by the lamps | The band stops on one held chord | Question text, read time bar | Curtain opens in 1.2 s (`staging.questionShow.curtainOpenSeconds`); the question fades in within 0.4 s (`staging.questionShow.questionInSeconds`), so it is readable for almost the whole 4 s | guess |
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

Eight seats for `players.max` = 8: a four seat sofa in the middle and two armchairs on each side, in an arc that comes forward at the ends with the chairs turned in towards the sofa (one conversation group), with the desk at one end, turned a little towards the guests. A new player takes the first free seat in this order: the two middle sofa seats, the two outer sofa seats, the inner armchairs, the outer armchairs. Two to four players therefore sit together on the sofa. Seat positions are markers in the scene, not config numbers. Players keep their seat through the return to the Lobby after a game and Back to lobby, like their character.

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

## Game night 1 version

Replaces the grey box version (D51) with option B (D89, D90, `decisions/0015_art_for_game_night_1.md`) and the Seats shot, question board and set pieces of `decisions/0016_lobby_shot_question_board.md`. A toggle in the TV settings panel switches it on and off (0016); with it off, the current flat views run as they are.

- **Scene:** the set built in Blender and exported as FBX: the sofa and armchairs in the seat order above (soft rounded, slightly worn-in cushions; woven fabric, a muted green sofa and armchairs in two browns), throw pillows and a folded wool throw, a low coffee table with books, coasters and a mug, two side tables (a lamp on one), a walnut desk with a desk microphone, a banker's lamp, mugs and cue cards, the curtain stage with the question board behind the curtain (velvet with irregular folds), the band riser (drums on a rug, keys with a stool, a guitar and a bass on stands, a music stand, a lamp on an amp, no band members), three rugs that mark the zones (guests, desk, the open floor in front of the stage), and a deep window with glass onto an invented evening city (three rows of lit 3D blocks in front of a far skyline, so the view shifts with the camera). Raised wood panels, wainscot and cornices on the walls, lattice trusses with lit lamp cans under a dark ceiling, warm pools of light on the open floor. Real materials, warm lights and a light film look. Lighting is baked in Unity for the static set, with light probes and one or two realtime shadow lights for the guests and hosts; realtime rim lights without shadows separate the guests and hosts from the wall, and the stage lamps stand on the floor in front of the stage, aimed through the arch at the board. The woods are matte browns in four tones (honey floor, dark brown walls, near black walnut trim, a lighter walnut desk), the window a night blue city with about a quarter of its windows lit (cozy pass, Nov 5, 2026).
- **Built set after the studio rebuild (Nov 5, 2026):** the furniture and dressing are bought-in models: a long tufted sofa in muted dark green, two classic tub armchairs and two leather and wood armchairs, a coffee table with a half played chess game, a candle and a mug, side tables with oil lamps, a vase and a photo frame, two floor lamps, a chandelier with fabric shades over the coffee table, candles on the window sill, plants, framed paintings, an encyclopedia set, a mantel clock and vases among the books on the host shelf; on the band stage an upright piano with a bench, a drum kit on a riser, a guitar and a bass on stands, two combo amps, a ukulele on the wall. Warmer light (practicals about 2400 K, candles warmer, a warm key, fill and rim; only the window stays night blue), candle clusters by the stage steps and on both risers, festoon strings over the window and the band stage, sconces beside the window. A thin warm haze shows the light shafts of the hung cans, the curtain wash, the band light and the stage lamps; the city behind the window ignores the haze. Each shot has a lens: a bokeh focused on the guests, the host or the curtain (the city goes soft), and a slow operator's drift; the `Stage` shot stays sharp and still for the board (D92). Velvet sheen on the curtain and the tub chairs, glass that reflects the room, cables and tape spike marks on the floor, a heavier bake (40 texels a metre, 4 bounces) and 4x MSAA. Performance is not tuned yet (Ilia: visuals first, performance later).
- **Guests:** each seated player is a capsule in their character's signature color with one signature prop, animated in code. The character's name is above it and the player's name under it. Capsules come first; the 16 props follow once everything else works (D98).
- **Hosts:** two cubes at the desk with a small idle animation and a speaking animation while their bubble shows (D90).
- **Cameras:** plain Unity cameras switched by a listener in Presentation (`StagingDirector`) on phase events. No Cinemachine (a new package needs asking first). Six shots (D96): `Seats`, `Establish`, `HostSolo` (on a cube), `Curtain`, `Stage` and `Wide`, following the shot list above. `HostSolo` replaces `Desk` in the Intro, and `Wide` plays wherever the list names `Desk` or `Band`, and Results stays on `Wide`. **The Intro opens on the hosts:** both `gameStart` lines start in the first 7 s (two 3.5 s bubbles from the Intro start, D58), so the Intro is `HostSolo` for each line with the studio still dark, then `Establish` with the lights coming up, then `Wide`. A line that starts after `Establish` has begun never cuts back. Without any line, `Establish` starts at once. The flat race track overlay sits over `Wide` in the race.
- **Look:** three lighting setups of the `Wide` shot (warm late night, retro 70s, modern night) are captured in Unity and compared; the AI picks one and shows Ilia all three (D99). Picked: **warm late night**, the closest to the mood pictures, and the one that keeps the characters' colours clean (retro muddies them, modern turns the strips blue).
- **How it is built:** `tools/blender/build_studio.py` models the set headless and exports `Studio.fbx`; the editor's `StudioBuilder` (Family Quiz menu or `-executeMethod FamilyQuiz.EditorTools.Studio.StudioBuilder.Run -look warm -bake`) makes the materials and procedural textures, places the lights, probes, film look and shot points, bakes and saves `Assets/Scenes/Studio.unity`; `-capture <folder>` renders every shot to PNG. Each part of the set (guests and desk, curtain stage, band) is modelled in its own straight frame and turned into place (`ZONES` in the script); the lights, light probes and camera positions are markers in the model (`LK_`, `CAM_`, `Zone_`), so moving a zone moves its lights and shots with it. `--plan <png>` after the FBX path renders a top down plan of the layout. The wood, fabric, leather, velvet and carpet surfaces are photo scanned CC0 sets from Poly Haven, fetched by `node tools/fetch_textures.mjs` (they are in the repo; the script re-fetches them and rewrites the license log `Assets/Game/Art/Textures/Scanned/LICENSES.md`). The runtime loads that scene additively behind the overlay (`StudioStage`). The bought-in props: `node tools/fetch_models.mjs` fetches the Poly Haven models (CC0) into `tools/blender/assets/polyhaven` (git-ignored, re-fetchable); the Sketchfab models (CC BY) are committed in `tools/blender/assets/sketchfab` with their license log and attribution texts. `blender --background --python tools/blender/build_props.py` turns each into one FBX in `Assets/Game/Art/Models/Props` (joined, decimated, facing the audience, origin at the bottom centre) with Unity ready textures and `Props.json`; `build_studio.py` places them with `Prop_<Name>_<n>` markers and `StudioProps` builds their materials and instances them. Shaders of our own in `Assets/Game/Art/Shaders`: `CityGlow` (the city, no fog), `LightBeam` (the light shafts) and `Velvet` (URP Lit plus a sheen, generated from URP's Lit).
- **Name tags:** in front of each seat, the character's name over the player's name (the player's name larger); neighbours on the sofa alternate between two heights so the names never overlap. In the Lobby the player's name turns green when ready.
- **Overlay with the set on:** the Lobby's room code and QR code shrink into the top left corner and the player list hides (the seats show the players); the Intro title moves to the top, out of the host solo shot; in Reveal only the correct option stays, as a banner over the empty floor under the guests, and the closest value line moves down there too. Every other view floats over the set as it is.
- **Lights up (D91, D95):** the studio around the seats is dark in `Seats` and lights up in `Establish`. The bake has the studio fully lit; `Seats` darkens it with post processing (exposure and a strong vignette) and the seat lamps, and `Establish` lifts that over `staging.intro.lightsUpSeconds`.
- **Reveal:** each guest holds up a colored card with its answer: the letter in its option color from the TV and phone for multiple choice, the number for closest value, an empty card for no answer. The cards go down in Scores.
- **Overlay:** the opaque full screen background of the flat views becomes transparent so the set shows behind them. Each view keeps only what the Overlay column of the shot list says (for example the Lobby keeps the room code, QR code and "Waiting for players"; the seats show the players).
- **Timing:** every `staging.extraSeconds` value stays 0, so all timing checks still pass.

## Open questions

1. Approve or change the draft roster in `content/characters.json`.
2. Should the idle limit move into `content/config.json` like the timers, so tests read it from there instead of a number in the test?
