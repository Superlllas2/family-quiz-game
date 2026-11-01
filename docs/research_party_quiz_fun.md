# Cut the Waiting, Keep the Sabotage

Snapshot of the research report "Making a party quiz game fun" from the Family Quiz Project (Oct 31, 2026).

Keep the mechanics critics loved in Knowledge is Power and delete the time that surrounded them. Reviewers called its Power Plays **"easily the most fun part of the game"**, yet a match asked only **about 12 to 15 questions in 20 to 30 minutes**, and one critic estimated that **roughly half of play time** went to watching effects, door walks and transitions ([Push Square](https://www.pushsquare.com/reviews/ps4/knowledge_is_power); [TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/); [Worthplaying](https://worthplaying.com/article/2017/12/16/reviews/106536-ps4-review-knowledge-is-power/)). The fix is to make things happen at the same time instead of one after another: power ups play out on the target's phone during the answer window, host lines run over the action, and every question ends the moment the last player locks in. At about **25 seconds per question** *(inferred)*, a default game of 4 rounds (16 questions), 3 mini games and a final race fits in **15 to 19 minutes** *(inferred)*, inside the **15 to 20 minute** sessions Jackbox is known for ([Jason Shen](https://www.jasonshen.com/2020/the-product-genius-behind-jackbox-games/)). The game feels good when taps react within **0.1 seconds** ([Nielsen](https://jakobnielsenphd.substack.com/p/time-scale-ux)) and celebrations grow with what a player actually achieved ([Kao et al. 2024](https://spiral.imperial.ac.uk/entities/publication/253c32f5-d124-4a23-88b5-9aaaf114608d)). A mixed family keeps enjoying it when speed scoring is capped, sabotage only hits players ranked above you, and last place can still win the final race. Family testers inflate ratings, so prove all of this in weekly game nights that measure waiting, laughter and unprompted requests for another game.

**How to read the numbers.** A number next to a source link comes from that source. *(Computed)* marks simple arithmetic on a sourced figure. *(Inferred)* marks a starting value I derived for this game from the sources; nobody has tested it, so treat it as your first playtest setting.

## Roughly half the original's play time went to watching, so budget every second

The complaints about Knowledge is Power (KiP) were about the gaps between questions, not the answer timer. TheSixthAxis wrote **"Time between questions is a bit long"**, and Push Square felt **"you're only being quizzed for half of the game"** ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/); [Push Square](https://www.pushsquare.com/reviews/ps4/knowledge_is_power)). Jokes were not the cause: You Don't Know Jack fits **11 questions into about 10 to 15 minutes** while its host jokes throughout ([Stevivor](https://stevivor.com/reviews/you-dont-know-jack/)).

### A question cycle of about 25 seconds

| Phase | Budget | Basis |
|---|---|---|
| Question shown on TV and read aloud | 3 to 5 s | Inferred |
| Answer window, four options | Up to 15 s, ending the moment everyone has locked in | Inferred; the early end copies Buzz! ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV)) |
| Last call once only one player is left | 5 s | Inferred; The Chase gives a rival 5 s to lock in ([Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)) |
| Correct answer reveal | 3 to 4 s | Inferred |
| Score and rank movement, with power up picks running at the same time | 3 to 5 s | Inferred |
| **Full cycle** | **20 to 30 s, about 25 s typical** | Inferred |

### Timers by question type

* **Typed, ordering or closest number:** 20 to 30 s *(inferred)*. Kahoot's minimum for typed and puzzle answers is **20 s** ([Kahoot](https://support.kahoot.com/hc/articles/115002308428)).
* **Topic vote from four:** 8 to 10 s with an automatic tie break *(inferred)*.
* **Final race questions:** 6 to 10 s *(inferred)*. Trivia Murder Party 2's final uses **12 s** ([Jackbox fan wiki](https://jackboxgames.fandom.com/wiki/Trivia_Murder_Party_2?oldid=3322)), and The Chase values a three step head start at about **12 seconds**, roughly 4 s per expert answer *(computed)* ([Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)).
* **Kids or relaxed play:** multiply every timer by 1.5 to 2 *(inferred)*, like Jackbox's Extended Timers setting ([Jackbox](https://www.jackboxgames.com/blog/accessibility-features-in-the-jackbox-party-pack-10)).

### A standard game of 15 to 19 minutes

Jackbox games are described as **15 to 20 minute** sessions that leave room for "just one more round" ([Jason Shen](https://www.jasonshen.com/2020/the-product-genius-behind-jackbox-games/)), and Kahoot advises **5 to 15 questions** per game, with several short games preferred over one long one ([Kahoot](https://kahoot.com/files/2020/02/Events-top-tips.pdf)). Too short also fails: That's You was criticized because games "don't go on for long enough" ([Push Square](https://pushsquare.com/reviews/ps4/thats_you)). So offer three lengths.

| Mode | Contents | Length *(inferred)* |
|---|---|---|
| Quick | 2 rounds (8 questions), 1 mini game, final race | 9 to 11 min |
| Standard, the default | 4 rounds (16 questions), 3 mini games, final race | 15 to 19 min |
| Long | 6 rounds (24 questions), 5 mini games, final race | 21 to 27 min |

The building blocks *(inferred)*: a round of four questions with its topic vote takes 2 to 2.5 minutes, a mini game 60 to 90 s, the final race 2 to 3 minutes, and the intro and outro about 1.5 minutes together. Even the Standard game asks more questions than the original did in 20 to 30 minutes.

### Rules that delete waiting

* **End every timer early.** Move on the moment the last answer arrives.
* **Never run a separate power up phase.** Picks happen during the reveal or the topic vote (next section).
* **Give phones a job in every TV moment:** a personal result, a one tap prediction, a reaction button *(inferred; no source has tested side tasks like these)*.
* **Keep every rule explainable in 30 seconds.** Most Jackbox games "can be explained in less than 30 seconds or simply intuited as you play" ([Jason Shen](https://www.jasonshen.com/2020/the-product-genius-behind-jackbox-games/)).
* **Show each tutorial once.** Let the host phone skip it, as Jackbox has allowed since Party Pack 6 ([Jackbox](https://www.jackboxgames.com/blog/responding-to-your-questions-from-the-jackbox-customer-survey)), and skip it automatically on "play again" *(inferred)*.
* **Preload the next round during reveals** so a loading screen never appears *(inferred)*.
* **Watch scene changes.** A 2026 early access review of Trivia Murder Party 3, Jackbox's first game built in Unreal Engine with 3D animation, still noted "a little sluggishness" in scene changes and phone connections ([TheSixthAxis](https://www.thesixthaxis.com/2026/09/22/trivia-murder-party-3-early-access-review-not-so-trivial-pursuit/)). Richer visuals cost time unless you guard against it *(inferred)*.
* **Hook players in the first 30 to 60 s** with a fast first question, then build, putting calm beats such as topic picks and mini games before peaks *(inferred)*. This follows Jesse Schell's interest curve ([Notes by Lex](https://notesbylex.com/interest-curve)) and Mike Stout's warning: "If it's always 'turned up to eleven,' eleven becomes the new five" ([Game Developer](https://gamedeveloper.com/design/trinity-part-6---intensity-ramps)).
* **Raise stakes late:** round values of 1x, 1.5x and 2x *(inferred)*. Jackbox raises point values as a game goes on ([Jason Shen](https://www.jasonshen.com/2020/the-product-genius-behind-jackbox-games/)).
* **Make the next game effortless:** "play again" keeps phones connected and skips the intro, and a family championship across the evening, like Kahoot's combined leaderboard ([Kahoot](https://kahoot.com/files/2020/02/Events-top-tips.pdf)), gives the loser of game one a reason to stay *(inferred)*.
* **Ignore attention span myths.** The adult "8 second" figure has no evidence behind it, and researchers say attention depends on the task ([University of Wisconsin handout](https://internet.psych.wisc.edu/wp-content/uploads/532-Master/532-UnitPages/Unit-09/Goldfish_MythBusting.pdf)). Variety and short games are the safer levers *(inferred)*.

## Sabotage should cost seconds, never turns, and always aim up

KiP's Power Plays were physical effects on the target's phone: Gloop to wipe away, ice to tap through, bouncing Bombles that cost time when touched, Nibblers that eat letters, and a Double Play combining two effects that tended to go to trailing players ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)). Critics loved the effects; the trouble sat around them.

Players chose effects in a separate step before every question and could not switch them off, which Worthplaying called **"an annoyance"** ([Worthplaying](https://worthplaying.com/article/2017/12/16/reviews/106536-ps4-review-knowledge-is-power/)). The sequel added toggles for players who "really can't take being glooped one more single time" ([PlayStation Blog](https://blog.eu.playstation.com/2018/08/22/battle-friends-over-pop-culture-trivia-in-playlink-quiz-show-knowledge-is-power-decades-out-14th-november/)), and one critic felt the effects "only ever prolonged you clicking the right answer" ([God is a Geek](https://godisageek.com/reviews/knowledge-is-power-review/)).

### Power up rules

* **Earn them by answering correctly.** Buzz!'s Point Stealer and Pie Fight, Blooket's treasure chest after each correct answer and Quizizz's streak rewards all tie attacks to knowledge ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV); [Blooket](https://help.blooket.com/hc/en-us/articles/42100784373655-Game-Overview-Gold-Quest); [Wayground](https://wayground.com/blog/top-5-new-quizizz-student-motivators)). Skill earns the draw and luck sets its strength, so kids can pull off upsets *(inferred)*.
* **Weight the draws toward trailing players,** as KiP did with Double Play ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)) and Mario Kart does with its blue shell ([Wikipedia](https://en.wikipedia.org/wiki/Blue_shell)).
* **Two taps, no extra phase:** pick, then target, during the reveal. A default target applies if the player doesn't choose *(inferred)*.
* **Resolve on the target's phone during the next answer window.** The TV shows only an icon by the avatar and at most one short host line *(inferred)*.
* **Cost seconds, never points or turns:** 2 to 4 s of wiping or tapping *(inferred)*. Designer Nicholas Kinstler calls effects that make you lose outright "pretty much player kryptonite" ([Game Developer](https://gamedeveloper.com/design/card-games---no-gain-from-pain-part-1)).
* **Aim up:** you may only target players ranked above you, and the leader gets a shield or a self boost instead *(inferred)*. Trailing players "attack whoever is ahead" anyway ([Skeleton Code Machine](https://www.skeletoncodemachine.com/p/kingmaking)), and KiP's losing players often teamed up against the leader ([Push Square](https://www.pushsquare.com/reviews/ps4/knowledge_is_power)). The rule also stops adults piling onto a child in last place.
* **Cap hits:** one effect per target per question, then one question of immunity *(inferred)*. Buzz!'s Pie Fight knocks a player out after two hits ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV)).
* **Give the leader counterplay,** such as a shield after a streak of 3 *(inferred)*. Mario Kart's blue shell later gained counterplay like the Super Horn ([Wikipedia](https://en.wikipedia.org/wiki/Blue_shell)).
* **Ship toggles on day one** for each power up and for all of them, as the sequel did ([PlayStation Blog](https://blog.eu.playstation.com/2018/08/22/battle-friends-over-pop-culture-trivia-in-playlink-quiz-show-knowledge-is-power-decades-out-14th-november/)), and **agree on the chaos level in the lobby:** "Family" (mild effects aimed at the leader automatically) or "Chaos" (free targeting and score swaps) *(inferred)*. Designer Cole Wehrle argues that without consent up front, kingmaking "can feel like a cruel joke rather than a coup" ([Skeleton Code Machine](https://www.skeletoncodemachine.com/p/kingmaking)).
* **With two players,** switch to self boosts or a team against the hosts. TheSixthAxis found the targeting mechanic "nullified" with only two ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)).
* **Still unknown:** whether naming the attacker ("Dad froze you") is funnier or breeds grudges. No source compares visible and anonymous attacks, so test both.

### Scoring that keeps slow but correct players in it

* **Use Kahoot's curve:** up to 1,000 points for a correct answer, minus at most half for slowness, so a correct answer at the buzzer still earns 500 ([Kahoot](https://support.kahoot.com/hc/articles/115002303908)). On a 15 s timer, a 3 s sabotage delay then costs about 100 points *(computed)*: a small cost, not a lost turn.
* **Avoid pure speed scoring.** Students guess to chase speed points ([Wang and Tahir](https://research.gold.ac.uk/id/eprint/39435/1/1-s2.0-S0360131520300208-main.pdf)), and IGN said Buzz! games "descend into a question of fastest finger first" ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV)).
* **Offer an accuracy only mode,** as Kahoot does ([Kahoot](https://support.kahoot.com/hc/articles/115002303908)).
* **Score number questions by distance.** Jackbox's Timejinx counts years off, which a critic said makes it "feel fair even if you're an absolute dunce about history" ([TheSixthAxis](https://www.thesixthaxis.com/2023/10/18/the-jackbox-party-pack-10-review/)).
* **Prefer visible handicaps** (longer kid timers, fewer kid options) over hidden help *(inferred)*. In a racing game study, balancing was preferred by experts and novices alike and did not hurt experts' experience, but the abstract does not say whether players noticed it ([Cechanowicz et al.](https://scholar2.islandarchives.ca/islandora/object/ir%3A10734)).

### Mini games of 60 to 90 seconds, with everyone playing

* **Budget 60 to 90 s each,** including a rules card of 10 s at most *(inferred)*. Critics said KiP's mini games "help to break up the same style of play" ([God is a Geek](https://godisageek.com/reviews/knowledge-is-power-review/)).
* **Change the input:** link, sort, swipe or race, as KiP's Linking and Sorting challenges did ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)).
* **Everyone plays at once,** with no turn taking *(inferred)*.
* **Let mini games hand out power ups,** as KiP's Steps of Destiny decided which Power Play you won ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)).
* **Test at 3 players and at 6 or more.** Buzz! dropped Pie Fight and Pass the Bomb with more than four players ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV)).

### A final race that last place can still win

Every strong precedent turns earlier scores into a head start, not a locked lead. Buzz!'s podiums start higher for leaders but sink until each player answers. Trivia Murder Party 2 (TMP2) makes the leader answer only two of three options, sends shadows after stragglers and blocks the exit with a barrier that needs a fully correct answer. The Chase gives one step of head start per contestant ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV); [Jackbox fan wiki](https://jackboxgames.fandom.com/wiki/Trivia_Murder_Party_2?oldid=3322); [Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)).

* **Seed by rank and cap the gap:** the leader starts at most 3 steps ahead of last place *(inferred)*. Early rounds buy position, but no lead is safe.
* **Handicap the leader** the TMP2 way, and put a barrier that needs a perfect answer before the finish ([Jackbox fan wiki](https://jackboxgames.fandom.com/wiki/Trivia_Murder_Party_2?oldid=3322)).
* **Add pressure that ends the race in 2 to 3 minutes** *(inferred)*, such as a sinking floor or a chasing shadow. Each side of The Chase's final gets two minutes ([Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)).
* **Caught players keep playing.** TMP2's ghosts keep answering and can still win ([Jackbox fan wiki](https://jackboxgames.fandom.com/wiki/Trivia_Murder_Party_2?oldid=3322)), and Trivia Murder Party 3 gives ghosts extra answer toggles so they can overtake ([TheSixthAxis](https://www.thesixthaxis.com/2026/09/22/trivia-murder-party-3-early-access-review-not-so-trivial-pursuit/)). A family version turns caught players into mischief makers who slow the leader *(inferred)*.
* **Always crown someone.** TMP2 revives the richest player if everyone dies, and The Chase still plays its final when all contestants are caught ([Jackbox fan wiki](https://jackboxgames.fandom.com/wiki/Trivia_Murder_Party_2?oldid=3322); [Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)).
* **Never allow a "lock."** In Final Jeopardy games where second place has less than half the leader's score, the final is effectively decided ([The Jeopardy Fan](https://thejeopardyfan.com/final-jeopardy-betting)). Log whether last place could still win when each final starts *(inferred)*.
* **Give trailing players choices, not charity:** a topic veto, a wager, or a trade of points for steps before the final *(inferred)*. Designer Matt Pioch warns that engineering close scores implies anyone who loses big should feel stupid ([University XP](https://www.universityxp.com/news/2023/11/28/the-runaway-leader-problem)).

### Teams and wagers for mixed ages

* **Family Teams** *(inferred)*: pair a child with an adult and average the scores, as Kahoot's team mode does ([Kahoot](https://support.kahoot.com/hc/en-us/articles/4408679135891-Kahoot-game-play-in-team-mode)), and sometimes show the right option only on the child's phone, an idea from Quizlet Live ([NGPF](https://www.ngpf.org/blog/ngpf-fellows/tech-tool-review-quizlet-live/)).
* **Everyone against the hosts** as a cooperative final *(inferred)*. The Chase's final is "resolutely a team game, with a dramatic climax" ([Wikipedia](https://en.wikipedia.org/wiki/The_Chase_%28British_game_show%29)).
* **Wagers with fixed sizes** and the topic shown first, as in Buzz!'s High Stakes round (bets of 50, 100, 250 or 500) ([Wikipedia](https://en.wikipedia.org/wiki/Buzz!:_Quiz_TV)), plus a floor so nobody sinks below a minimum, used only while scores are within reach *(inferred)*.

## Questions need an "oh, yeah" payoff and a way in for every age

KiP's questions drew criticism from both ends: "far too banal" for adult critics ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)) yet sometimes "overly tough" ([Common Sense Media](https://www.commonsensemedia.org/game-reviews/knowledge-is-power)). Because critics ranked the Power Plays above the questions, the remake's questions have to carry more of the fun themselves *(inferred)*. Luke Kelly, a question writer on The Chase, names the goal: when you hear a good answer, "you go, 'Oh, yeah'" ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).

### A six point checklist from professional setters

* **One defensible answer.** NPR's Ask Me Another faced a controversy when more than one fruit fit a question ([PEM Playtime](https://playtime.pem.org/?p=3243)).
* **The shortest possible answer** ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).
* **An answer worth retelling,** one that tells "you something you want to tell your mates" ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).
* **A reasoning hook,** so someone who doesn't know the fact can still work it out and come away "feeling smart, not stupid" ([SUPERVERBOSE](https://www.superverbose.com/2011/03/09/good-questions-part-1/)). It must not be so strong that logic alone solves the question, a flaw critics found in KiP ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)).
* **Wrong options built from the likely wrong answers** a confident player would give ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).
* **Two sources for every fact,** as Millionaire's setters require ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).

### Difficulty by the numbers

* **The guessing floor:** with four options, **25%** answer correctly even when nobody knows; if half the room knows, about **62.5%** do *(computed)*, using Sorum's guessing formula ([ETS](https://www.ets.org/research/policy_research_reports/publications/report/1958/ibsi.html)). True or false starts at **50%**.
* **So put each round's hard question in a low guess format** such as closest number, ordering or a typed answer *(inferred)*.
* **Round mix:** 1 easy opener, 2 medium, 1 hard *(inferred)*. TV setters grade questions and often open with an easy one ([RTS](https://rts.org.uk/article/meet-question-setters-brains-behind-your-favourite-quiz-shows)).
* **Target 60 to 75% correct for adults** on average *(inferred)*. Flag questions under **35%** correct, the line Kahoot's reports use for "difficult" ([Kahoot](https://support.kahoot.com/hc/articles/360035063054)), and any question everyone gets right. Track kids separately.

### One special format per round

Keep multiple choice as the base and give each round one special format, never repeating a special format in back to back rounds *(inferred)*. Precedents are thin: KiP put a mini game every fourth round ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)), and Pointless rotates through formats such as open ended questions and picture boards ([Wikipedia](https://en.wikipedia.org/wiki/Pointless)). Table entries are inferred unless linked.

| Format | What it adds | Writer's burden |
|---|---|---|
| Closest number | Everyone can guess; partial credit | One authoritative number with units |
| Put in order | Rewards partial knowledge; Kahoot caps it at 3 to 4 items ([Kahoot](https://support.kahoot.com/hc/articles/115002308428)) | Values spaced far apart |
| Which came first | Fast and deducible | Reliable dates |
| Odd one out | Aha moments | Highest risk of a second valid answer |
| Picture zoom | Spectacle; suits family photos | Image rights |
| True or false burst | Pressure | 50% guess rate, so best in the final |

### Topic doors without the trap

* **Keep the pick from four.** Worthplaying praised KiP's linked topics over "a random selection of categories" ([Worthplaying](https://worthplaying.com/article/2017/12/16/reviews/106536-ps4-review-knowledge-is-power/)).
* **Avoid the trap** TheSixthAxis named, linked topics that "pigeonholes you" ([TheSixthAxis](https://www.thesixthaxis.com/2017/11/22/knowledge-is-power-review/)). Make one door always a broad family topic, break the chain every round or two, and give the trailing player the pick or a veto *(inferred)*.
* **Never offer the same four twice.** The sequel's decade votes felt like "every few questions just choosing a decade" ([Push Square](https://www.pushsquare.com/reviews/ps4/knowledge_is_power_decades)).

### Mixed ages and family content

* **Same TV question, different phone help:** kids get 2 or 3 options instead of 4, extra time or a private hint *(inferred; no source has evaluated this)*.
* **Mix high and pop culture in one question,** You Don't Know Jack's founding idea ([Wikipedia](https://en.wikipedia.org/wiki/You_Don%27t_Know_Jack_%281995_video_game%29)), so each generation has a way in *(inferred)*.
* **Filter content.** KiP was rated 13+ partly for questions about sex and drugs ([Common Sense Media](https://www.commonsensemedia.org/game-reviews/knowledge-is-power)).
* **Family photo questions can be the highlight, or awkward.** That's You's "most likely to" questions revealed how others see you, "usually for the worse" ([Push Square](https://pushsquare.com/reviews/ps4/thats_you)). Celebrate memories instead (who is this, which year, scored by closeness), let the featured person approve the answer, let anyone pictured veto a photo, and take answer keys from the family, never from an AI model *(inferred)*.

### Sources and AI drafting for a game you may sell

* **Licenses** (have a lawyer confirm before sale): Wikidata's structured data is CC0, a public domain dedication ([Wikidata](https://www.wikidata.org/wiki/Wikidata:Licensing)). Open Trivia Database uses a Creative Commons attribution and share alike license ([Open Trivia Database](https://opentdb.com/api_config.php)), so adapted question sets would likely have to be shared on the same terms *(inferred)*. The Trivia API uses a Creative Commons noncommercial license and needs a paid plan for commercial use ([The Trivia API](https://the-trivia-api.com/)). Your own questions plus Wikidata facts are the cleanest base *(inferred)*.
* **AI drafts need checking.** On hard short factual questions, the best model tested answered **42.7%** correctly, and "models consistently overstate their confidence" ([SimpleQA](https://arxiv.org/html/2411.04368v1)). Only **32%** of ChatGPT drafted exam questions were correct with explanations ([Boston University](https://www.bumc.bu.edu/camed/2023/12/20/study-finds-ai-language-model-failed-to-produce-appropriate-questions-answers-for-medical-school-exam)).
* **A drafting pipeline** *(inferred)*: fetch the fact first and write around it; have a second model answer the finished question blind several times and drop disagreements; ask for every other defensible answer; run a critique and rewrite loop, which raised expert ratings in one study ([arXiv](https://arxiv.org/abs/2410.13191)); review by hand; add a "report this question" button.
* **Russian:** write natively, or from Wikidata's Russian labels, instead of translating English wordplay. The Trivia API's Russian versions are written by an LLM ([The Trivia API](https://the-trivia-api.com/)).

## Two hosts earn laughs by talking over the action, never between it

Hosts can cover waiting or become it. Worthplaying found KiP's host "plays it too safe" next to You Don't Know Jack (YDKJ), which "filled its downtime with witty comments from the host" ([Worthplaying](https://worthplaying.com/article/2017/12/16/reviews/106536-ps4-review-knowledge-is-power/)). YDKJ's Cookie "keeps the game fast" with lines that change with answer speed and correctness, and his rarely repeating lines drive replay value ([Stevivor](https://stevivor.com/reviews/you-dont-know-jack/)). Buzz!'s host was marked down as "still a little irritating and repetitive" ([Trusted Reviews](https://www.trustedreviews.com/reviews/buzz-quiz-tv)).

### Timing rules

* **Reaction lines of 1 to 4 s, transitions under 6 s,** and one longer double act bit per session, skippable *(inferred)*.
* **Talk during answer windows, reveals and score movement,** never as a gate before the next question *(inferred)*.
* **Choose the reaction after the outcome is known,** the way Millionaire's presenter only sees the answer after "final answer" ([Wikipedia](https://en.wikipedia.org/wiki/Who_Wants_to_Be_a_Millionaire%3F_%28British_game_show%29)).
* **Make lines interruptible:** cut them when everyone has answered or the host phone taps skip, put the joke early, and record a few "fine, I'll stop talking" reactions *(inferred)*. Valve built its dialogue system so conversations break off sensibly ([Emily Short](https://emshort.blog/2012/03/16/gdc-2012-talk-on-dynamic-dialogue/)).
* **Leave air.** Valve's system has a default delay setting that mutes a speaker for 2.8 to 3.2 s after a line, and it can give a line a set chance of silence ([Valve](https://developer.valvesoftware.com/wiki/Response_System)).

### Repetition is a bug, not polish

* **Shuffle without repeats.** Valve's response system plays every line in a group before repeating any ([Valve](https://developer.valvesoftware.com/wiki/Response_System)). Save the list of lines already heard across game nights *(inferred)*.
* **Why it matters** *(computed)*: picking randomly from 10 variants, an event that fires 4 times repeats a line about **50%** of the time; with 20 variants, about **27%**; with 10 variants fired 8 times, about **98%**. Shuffling gives zero repeats until the pool runs out.
* **Size pools by frequency** *(inferred)*. A reaction to every question fires about 48 times in an evening of three Standard games, so frequent moments mostly get sound effects or silence. Rare, specific moments (a comeback from last place, a perfect round, a power up that backfires, everyone wrong) get 3 to 5 lines each. One game writing guide suggests drafting 5 to 10 alternatives per line ([How To Write a Game](https://howtowriteagame.substack.com/p/how-to-write-video-game-barks)).
* **The most specific line wins.** "Leader, on a streak of 3, just frozen" beats a generic "correct" line, as in Valve's rule matching ([Emily Short](https://emshort.blog/2012/03/16/gdc-2012-talk-on-dynamic-dialogue/)). Specific lines create the "illusion of awareness" that Jackbox designs its hosts around ([Built In](https://builtin.com/media-gaming/jackbox-games-design-party-pack)).

### A double act built on a status seesaw

* **Contrast with swappable roles.** One partner plays the reasonable feed and one the comic, but "in many successful acts the roles are interchangeable" ([Wikipedia](https://en.wikipedia.org/wiki/Double_act)).
* **The Urgant and Nagiev pattern.** In a 2017 guest spot on Vecherniy Urgant, Urgant opened with mock flattery about Nagiev turning 50, Nagiev answered with sarcasm and self irony, and the bit escalated into staged physical one upmanship on the desk ([Vokrug TV](https://www.vokrug.tv/amp/article/show/50letnii_dmitrii_nagiev_pomerilsya_fizicheskoi_formoi_s_ivanom_urgantom_58912/)). Elsewhere Urgant teases by building absurd theories from harmless public facts ([Vokrug TV](https://www.vokrug.tv/amp/article/show/16155994881/)).
* **Turn it into roles** *(inferred)*: one host needles with fake compliments and absurd "evidence"; the other overreacts with wounded pride and bravado; neither wins for long. Improv teacher Keith Johnstone describes this kind of trade as a status seesaw, an idea that has not been tested empirically ([Psychology Today](https://www.psychologytoday.com/us/blog/once-more-with-feeling/201705/every-human-interaction-explained)). Make the rivalry about the game: whose round went better, who reads better, who deserves credit for the winner.
* **Keep affection visible.** Cook and Moore's act degraded into "aggressive attacks on the defenseless Moore" ([Wikipedia](https://en.wikipedia.org/wiki/Double_act)). Let the hosts defend each other when a player mocks one *(inferred)*.
* **Build humor strangers can get** *(inferred)*: traits readable at first sight, callbacks to what happened tonight, and a running gag that escalates, improv's "repeated pattern of funny behaviour with escalating emotion" ([Hoopla Impro](https://www.hooplaimpro.com/quick-guide-game-of-scene)). Inside jokes about your friends' real lives will not travel to buyers.
* **Write a one page bible per host** (want, flaw, catchphrase, attitude to the other host, favorite and hated topics, limits) and keep a canon list *(inferred)*. YDKJ's host performer once caught a writer referencing the host's father, who was dead in the canon ([CGMagazine](https://www.cgmagonline.com/interviews/pop-culture-lightning-round-an-interview-with-jellyvision)). Edit with Jellyvision's two questions, "what's simpler?" or "what's funnier?" ([Game Developer](https://www.gamedeveloper.com/game-platforms/q-a-jellyvision-insists-i-you-don-t-know-jack-i-again)).

### Teasing players without hurting anyone

A review of teasing research found that a tease reads as play when it carries many "off record" markers such as exaggeration, ritual phrases, nicknames and a singsong delivery; that "Comments that are highly relevant to the target run the risk of greater offense"; and that teasing from higher status people was more hostile ([Keltner et al. 2001](https://esilab.berkeley.edu/wp-content/uploads/2017/12/Teasing-Review.pdf)). The rules that follow are *(inferred)*:

* **Tease the play, not the person:** answers, speed, streaks, power up choices. Never looks, weight, age, intelligence, family ties, money, school or accents.
* **Tease the leader most,** and give last place sympathy or comeback hype.
* **Give every player at least one compliment per game,** and cap teases per player.
* **Send the sharpest jokes host to host,** since the hosts are fictional and willing targets.
* **Offer a gentle setting** that swaps teasing for encouragement when young kids play.
* **Show names on screen** while recorded lines say "you", since recordings cannot speak typed names.
* **Direct your friends to sound warm,** so the wink is audible.

### Recording and cloning your friends

* **Prototype with text to speech first,** as Goldfire Studios did to judge flow and pacing before recording actors ([Goldfire Studios](https://goldfirestudios.com/prototyping-dialogue-with-google-text-to-speech)).
* **Keep one spreadsheet** with line ID, host, trigger, direction and filename, and record 2 or 3 takes per line ([Game Developer](https://www.gamedeveloper.com/audio/screen-play-documenting-voice-assets)). Yarn Spinner for Unity links audio files to lines by line ID ([Yarn Spinner](https://docs.yarnspinner.dev/yarn-spinner-for-unity/samples/sample-guide-voice-over-and-localisation)).
* **Record "blind" with direction tags** (emotion, intensity from 1 to 3, who is being addressed) instead of the joke's setup, and batch lines by emotion *(inferred)*.
* **Cloning for question reading:** ElevenLabs states "Even with their consent, you cannot clone someone else's voice", so each friend creates the clone on their own account and shares it. It needs at least 30 minutes of audio (2 to 3 hours recommended) and copies the speaking style and any noise in the samples ([ElevenLabs Help](https://help.elevenlabs.io/hc/en-us/articles/36842751624209-Can-I-create-a-Professional-Voice-Clone-of-someone-else-s-voice); [ElevenLabs docs](https://elevenlabs.io/docs/product-guides/voices/voice-cloning/professional-voice-cloning)). Record the training audio in the same room and on the same mic as the banter, reading quiz questions in host voice *(inferred)*.
* **Get written consent** covering uses, sale, credit or payment, who controls the voice model, withdrawal and a final listen with veto, and keep jokes human unless a friend agrees otherwise *(inferred)*. The 2025 SAG AFTRA video game agreement requires informed consent for every planned AI use ([TheWrap](https://www.thewrap.com/sag-aftra-video-game-deal-details/)).

## Instant taps and earned celebrations make the game feel good

Juice, the burst of motion and sound that answers an input, is "maximum output for minimum input" ([Game Developer](https://gamedeveloper.com/design/video-is-your-game-juicy-enough-)). Nearly all of it lasts under a second, so it can run alongside play *(inferred)*.

The dose matters: medium and high juice beat both no juice and extreme juice ([Pichlmair and Johansen](https://arxiv.org/pdf/2011.09201)). In a preregistered experiment with **1,699 participants**, feedback tied to success raised competence and curiosity, blanket amplification "unexpectedly reduced them", and curiosity was the strongest predictor of enjoyment ([Kao et al. 2024](https://spiral.imperial.ac.uk/entities/publication/253c32f5-d124-4a23-88b5-9aaaf114608d)). Sound counts too: in a study of **593 students**, Kahoot played without points and music gave the worst results ([Wang and Tahir](https://research.gold.ac.uk/id/eprint/39435/1/1-s2.0-S0360131520300208-main.pdf)).

### The phone controller

* **No app.** Join with a QR code that carries the room code, or a four letter code the TV reads aloud *(inferred)*. Reviewers disliked KiP's separate app ([Push Square](https://www.pushsquare.com/reviews/ps4/knowledge_is_power)), and at Jackbox "No app download or peripherals are needed" ([Built In](https://builtin.com/media-gaming/jackbox-games-design-party-pack)).
* **One decision per screen,** since Jackbox never asks players for more than one choice at a time ([Built In](https://builtin.com/media-gaming/jackbox-games-design-party-pack)). Use four huge buttons in a 2x2 grid matching the TV, each with position, color, shape and letter, because accessibility guidelines say no essential information should depend on color alone ([Game Accessibility Guidelines](https://gameaccessibilityguidelines.com/full-list/)). WCAG's minimum target is 24 by 24 CSS pixels ([W3C](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html)); give each button a quarter of the screen *(inferred)*.
* **React on touch down within 1 to 2 frames** *(inferred)*, well inside the 0.1 s that "creates the illusion of instantaneous response" ([Nielsen](https://jakobnielsenphd.substack.com/p/time-scale-ux)). Show "Locked: B" at once, a check mark when the server confirms (target under 0.5 s), and "Sending…" with a retry after 1 s *(inferred)*.
* **Personal payoff on the phone** after each reveal ("Correct! +850, streak 3, you are 2nd"), shared spectacle on the TV *(inferred)*. Jackbox values the phone for information private to each player ([Built In](https://builtin.com/media-gaming/jackbox-games-design-party-pack)).
* **Keep phones silent** so the TV is the one sound source *(inferred)*.
* **Treat haptics as a bonus.** Chrome on Android vibrates only after the player's first tap ([W3C on GitHub](https://github.com/w3c/vibration/issues/25)), iOS Safari lacks the Vibration API through version 27.2 ([caniuse](https://caniuse.com/vibration)), and the iOS workaround may stop working in future versions ([azukiazusa.dev](https://azukiazusa.dev/en/blog/ios-safari-web-haptics)).
* **Keeping phones awake needs HTTPS.** Screen Wake Lock works in iOS Safari from version 16.4, but only on secure pages, and it is released whenever the page is hidden ([caniuse](https://caniuse.com/wake-lock); [MDN](https://developer.mozilla.org/en-US/docs/Web/API/Screen_Wake_Lock_API)). A controller served over plain HTTP from the laptop's local network address cannot use it *(inferred)*. Use an HTTPS relay or a local certificate, and request the lock again whenever the page returns.
* **Build reconnects in from the start.** A WebKit bug report describes iPhones closing WebSockets immediately when the screen locks ([WebKit](https://bugs.webkit.org/show_bug.cgi?id=247943)), and Socket.IO warns that its recovery "will not always be successful" ([Socket.IO](https://socket.io/docs/v4/connection-state-recovery)). Store a rejoin token on the phone, reconnect when the page becomes visible, request a full state snapshot, hold the seat and score for several minutes, and show a sleeping badge on the TV instead of waiting *(inferred)*.
* **Time answers on the phone,** from the moment the options render to the tap, so clock differences between devices don't matter. Use a clock offset estimate such as Cristian's algorithm ([Wikipedia](https://en.wikipedia.org/wiki/Cristian%27s_algorithm)) only to unlock all phones at the same moment *(inferred)*.

### TV feedback and suspense

Durations here are *(inferred)* unless linked.

* **Lock in:** the player's avatar pops with a tick, showing who answered but not what.
* **Correct:** a white flash for a frame or two, the hit flash from Jan Willem Nijman's game feel talk ([Pichlmair and Johansen](https://arxiv.org/pdf/2011.09201)), then the tile grows under a bright sting, with bursts on correct avatars sized by speed.
* **Wrong:** desaturate and shrink with a short low thud, always smaller than the success feedback.
* **Scores:** count up in under 1 s, move all leaderboard rows together in about 0.5 s, and give a new leader a crown handover.
* **Big effects** (confetti, strobes, screen shake) only for round wins and the final.
* **Suspense holds:** 1 to 2 s for an ordinary reveal, 3 to 5 s at the end of a round, at most 6 to 8 s for the final result, always below the 10 s that Nielsen calls "the maximum delay before the user's attention drifts" ([Nielsen](https://jakobnielsenphd.substack.com/p/time-scale-ux)). Fill holds with live information: lock ins arriving, answer bars growing, the last 5 seconds ticking.
* **Escalate like Millionaire.** Its UK score played through the episode and rose a semitone per question, "mimicking the sound of a beating heart" ([Wikipedia](https://en.wikipedia.org/wiki/Who_Wants_to_Be_a_Millionaire%3F_%28British_game_show%29)). Raise your music a semitone per round and per final question (a pitch ratio of about 1.059 *(computed)*), and darken the set for the final.

### Audio and accessibility

* **Separate sliders** for music, effects and host voice, as Jackbox has offered since Party Pack 7 ([Jackbox support](https://support.jackboxgames.com/hc/en-us/articles/15794801592855-What-accessibility-features-are-available-in-your-games)), plus music off with effects on, since Kahoot users called its fixed countdown music "annoying" ([Kahoot community](https://support.kahoot.com/hc/en-us/community/posts/115001072588-Ability-to-customize-or-disable-the-countdown-music?page=2)). Lower the music under host lines *(inferred)*.
* **Audio you can sell with:** Kenney's sounds are CC0 ([Cinevva](https://app.cinevva.com/guides/free-sound-effects-music)); the free Sonniss GDC 2026 bundle holds 7.47 GB of royalty free effects with no attribution required ([Rekkerd](https://rekkerd.org/sonniss-releases-gdc-2026-game-audio-bundle/)); Pixabay tracks marked with a Content ID shield can trigger claims on streamers' videos ([Pixabay](https://pixabay.com/service/faq/)). Keep a license log from day one *(inferred)*.
* **TV text:** the Xbox guidelines' minimum body text height for console games is 26 px at 1080p ([Xbox](https://devdocs.xbox.com/build/game-principles/accessibility/xag-deep-dives/xag-101-text-display.md)). Questions read from the sofa need about 2 to 3 times that, roughly 52 to 78 px, in sentence case and a sans serif font, two lines at most *(inferred)*.
* **Read every question aloud** with matching subtitles ([PCC](https://www.pcc.edu/instructional-support/tools/kahoot/accessibility-tips-from-aedr/)), and offer a "show question on my phone" option like Kahoot's ([Kahoot](https://kahoot.com/accessibility/)).
* **Start the speed clock after the reading.** Adults average 238 words per minute ([BPS](https://www.bps.org.uk/research-digest/most-comprehensive-review-date-finds-average-persons-reading-speed-slower)), so a 20 word question with its options takes about 5 s at that pace and 12 s at 100 words per minute *(computed)*. Doubled timers (30 s) match a college accessibility guide's 30 s minimum for multiple choice ([PCC](https://www.pcc.edu/instructional-support/tools/kahoot/accessibility-tips-from-aedr/)).
* **Add a reduce flashes and motion toggle,** like Jackbox's motion sensitivity setting ([Jackbox support](https://support.jackboxgames.com/hc/en-us/articles/15794801592855-What-accessibility-features-are-available-in-your-games)).

## Family game nights prove fun only when you measure behavior

Jackbox paper tests every idea, playtests weekly in groups of 3 to 8, talks through the emotions players felt, and watches Twitch streams for laughter and frustration; player suggestions "usually point to a bigger problem" ([Built In](https://builtin.com/media-gaming/jackbox-games-design-party-pack)). Valve treats designs as hypotheses and playtests as experiments, "DEFINITELY not focus testing" ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)).

Family testers bring a known bias: relatives "don't want to hurt your feelings" ([Jesse Schell](https://stewart.sdsu.edu/cs583_spr2013/LearningXNA4_lects/JesseSchell_playtesting101.pdf)), and playing against a friend produces more positive emotion than playing against a stranger ([Ravaja 2009](https://synapsesocial.com/papers/W1998854493)).

### Build in four cheap stages

* **Paper show:** question cards, four topic cards, power up cards, a whiteboard and you reading host lines, with a stopwatch on every phase *(inferred)*. Jackbox's Blather 'Round paper test simulated digital features by voice ([Jackbox](https://www.jackboxgames.com/behind-the-scenes-the-evolution-of-blather-round)).
* **Grey box** (a plain placeholder build): a simple Unity screen, plain phone buttons, text to speech hosts and logging from the first build *(inferred)*.
* **Wizard of Oz hosts:** you or the two friends trigger or improvise lines live, to learn which moments need a line at all. The person playing the system must respond consistently so sessions stay comparable ([QUT](https://www.ixdmethods.qut.edu.au/wizard-of-oz/)).
* **Record only the lines that earned laughs,** keeping text to speech as a fallback *(inferred)*. Expect to throw prototypes away: Jackbox rebuilt Blather 'Round from scratch ([Jackbox](https://www.jackboxgames.com/behind-the-scenes-the-evolution-of-blather-round)).

### Log these numbers

Kahoot's per question reports ([Kahoot](https://support.kahoot.com/hc/articles/360035063054)) and Valve's gameplay stats ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)) are the precedent. Targets are *(inferred)* unless linked.

| Metric | Target or flag |
|---|---|
| Share of game time when no phone accepts input | Under about 20%; the original was roughly 50% by one critic's estimate ([Worthplaying](https://worthplaying.com/article/2017/12/16/reviews/106536-ps4-review-knowledge-is-power/)) |
| Longest stretch with nothing to do | Under 10 s, outside the intro and the final result |
| Idle time per player | Watch fast answerers and last place most closely |
| Correct rate per question, adults and kids apart | Flag under 35% ([Kahoot](https://support.kahoot.com/hc/articles/360035063054)) and at 100% |
| Can last place still win when the final starts | Always yes |
| Power ups offered versus used, and who targets whom | Low use means fix the phone screen first |
| Disconnects and page hidden events | Falling toward zero |
| "Play again" presses | Rising across weeks |

* **Write one JSON line per event** (phase start and end, every phone input, with session, build, player and an adult or kid tag) to a file on the laptop *(inferred)*. Ask your AI coding tool for this logger in the very first build.
* **Read patterns across sessions, not averages.** Valve warns that averages hide extremes and that analysts can see "illusory" patterns ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)).

### Watch, then ask

* **Fill an observation sheet per round** *(inferred)*: laughs and their cause, frustration ("that's unfair"), confusion, waiting (phones down, side talk, trips to the kitchen) with the phase it happened in, and whether last place still leans in. Let a relative fill it, or record audio with consent, since nobody can host and observe at once.
* **Read the same intro every time:** you are testing the game, not them ([Jesse Schell](https://stewart.sdsu.edu/cs583_spr2013/LearningXNA4_lects/JesseSchell_playtesting101.pdf)).
* **Run a three minute anonymous phone poll before any group talk,** because group discussion invites anchoring and social pressure ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)):
  * the most fun moment
  * the most frustrating or confusing moment ([Stonemaier Games](https://stonemaiergames.com/?p=11002))
  * when you were waiting with nothing to do
  * play again next game night: Yes, Maybe or No ([Read and Horton](https://knowledge.lancashire.ac.uk/id/eprint/54967/1/54967%20Read%20&%20Horton%20VOR.pdf))
  * a ranking of tonight's mini games
* **With kids, trust behavior over answers.** Over 40% of younger children gave everything the same score in one study ([Read and Horton](https://knowledge.lancashire.ac.uk/id/eprint/54967/1/54967%20Read%20&%20Horton%20VOR.pdf)), and kids are "much more likely to say what they think you want to hear" ([Games User Research](https://gamesuserresearch.com/how-to-test-games-with-kids/)). Keep sessions under 2 hours with a break ([Games User Research](https://gamesuserresearch.com/how-to-test-games-with-kids/)).
* **Beat the friend effect with comparisons:** alternate version A and version B across the rounds of one evening *(inferred)*, in the spirit of Valve's design experiments ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)), and invite people new to the game now and then for first impressions ([Jesse Schell](https://stewart.sdsu.edu/cs583_spr2013/LearningXNA4_lects/JesseSchell_playtesting101.pdf)).
* **Skip the heavy tools.** The full Game Experience Questionnaire showed structural weaknesses in a 447 person test ([Brühlmann and Schmid](https://edoc.unibas.ch/40075/)), and Valve calls physiological sensors expensive and intrusive ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)).

### Decide with simple rules

No studio publishes numeric cut rules, so these are *(inferred)*:

* **Judge a mini game after 3 sessions,** ideally with 2 different groups.
* **Rework or shelve it** if most replay votes are No, it keeps ranking last, or it shows no laughs and visible waiting twice. **Keep it** if most vote Yes and players ask for it by name. Shelving is cheap: Jackbox revives shelved ideas in later packs ([Jason Shen](https://www.jasonshen.com/2020/the-product-genius-behind-jackbox-games/)).
* **Fix confusion before judging fun,** and shorten a slow phase before cutting the feature.
* **Run a weekly loop:** write 1 to 3 hypotheses on Sunday, build only those changes behind toggles, test on game night, then log keep, change or drop for each, Valve's three outcomes ([Valve](https://cdn.fastly.steamstatic.com/apps/valve/2009/GDC2009_ValvesApproachToPlaytesting.pdf)).
* **Treat an unprompted "one more game"** as the strongest signal you will get.

## Conclusion

The core design problem is subtraction, not invention. Every mechanic critics enjoyed in Knowledge is Power survives in this plan; what changes is that sabotage, host comedy, scoring drama and loading run alongside answering instead of in sequence. That makes one number more useful than any rating: the share of game time when no phone accepts input. Log it from the first grey box build, because every animation, transition and host line added later pushes it up without anyone noticing. Since you build by prompting, the budgets above can go straight into prompts as testable requirements, such as "start the reveal within 1 second of the last answer."

The rules that keep kids and grandparents in contention also make the game faster: capped speed scoring, sabotage that aims up and costs only seconds, generous timers that end early, and a capped head start in the final. Fairness and pace pull in the same direction, so neither has to be traded for the other. Real gaps remain: no source gives an ideal family session length, reveal duration, number of line variants or cut threshold, and nobody has measured phone latency over home WiFi or TV lag over HDMI. Every *(inferred)* number here is a first setting; your family's behavior on game night sets the final ones.
