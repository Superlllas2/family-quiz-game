# Refining batch 2: twelve new topics (Nov 7, 2026)

Step 3 of `refining_plan.md`, batch 2. Twelve topics not refined before now hold 20 questions each (6 easy, 10 medium, 4 closest value). The refined bank has **520 questions in 26 topics**. Same method as `batch_01.md`: one editor per topic, three blind readers calibrated on the audit as the gate, then fix rounds until every question passes.

## Result

Scores are the independent readers'. All 240 are 70 or more; the average is 76.7.

| Topic | Same | Rewrite | New | Lowest | Average |
| --- | --- | --- | --- | --- | --- |
| films_old | 3 | 16 | 1 | 70 | 75.8 |
| ru_tv | 0 | 10 | 10 | 70 | 75.5 |
| heroes | 0 | 18 | 2 | 71 | 75.9 |
| board | 0 | 15 | 5 | 73 | 78.8 |
| music_old | 0 | 12 | 8 | 71 | 75.5 |
| art | 0 | 10 | 10 | 70 | 76.5 |
| ancient | 3 | 12 | 5 | 70 | 77.8 |
| history20 | 3 | 15 | 2 | 71 | 76.8 |
| mysteries | 2 | 14 | 4 | 71 | 76.2 |
| science | 6 | 8 | 6 | 71 | 78.5 |
| sea | 3 | 12 | 5 | 71 | 76.3 |
| countries | 4 | 10 | 6 | 70 | 77.2 |

**How many it took:**
- The readers put 36 of the first 240 under 70, ru_tv the most.
- The editors' replacements were scored again: 9 more fell short, then 1 more.
- About 30 facts were corrected, softened or cut. Examples:
  - The ancient dice that really broke the "opposites add to 7" rule.
  - The Voronins' Guinness record at episode 455.
  - Brenda Lee overtaking Mariah Carey's slow climb to No. 1.
  - A Finnish speeding fine that could not be confirmed, replaced by a sourced one.
  - A static-electricity fact that mixed two rows of a table.
- One question was dropped for tone: Kashpirovsky's surgery without anaesthetic.

## Set aside

35 more v4 questions under the Russia rule, in `tools/content/set_aside.json` (88 in all):
- **art (17):** the Russian painters (Shishkin, Repin, Aivazovsky, Malevich and others) and the Hermitage cats.
- **mysteries (10):** Tunguska, the Dyatlov Pass, Anastasia, the Amber Room, Knorozov, Kitezh, Sannikov Land, Tamerlane's tomb.
- **board (2):** Mafia, Russian lotto.
- **history20 (2):** Sputnik, the Cuban Missile Crisis.
- **science (2):** ruthenium, Mendeleev's table.
- **countries (2):** Astana's Soviet names, Russia as a yardstick.
- **sea (1):** Murmansk.

## Claims (later topics must not reuse these)

| Kept by | Fact | Must not be reused by |
| --- | --- | --- |
| ru_tv | «Купил, включил, не работает» (KVN-49) | ussr_032 |
| ru_tv | Senkevich and Heyerdahl's boat | people_020 (also a Russia subject) |
| ancient | pyramid builders' daily beer | drinks_029 |
| ancient | mercury rivers in Qin's tomb; the Antikythera mechanism | (mysteries left them) |
| art | painted Greek statues | misconceptions_017 |
| art | the Gardner museum's empty frames | crime_046's frames |
| languages | Brazil's language, the Bulgarian nod | countries_003, countries_036 |
| | Angkor Wat on Cambodia's flag | nobody uses it now (countries_018 scored 61) |

The editors also left these to the topics that own them: Nirvana's deodorant (nineties), "Paul is dead" (conspiracy), You'll Never Walk Alone (football), cacao money (money, drinks), the Moon rock (conspiracy), the barcode (inventions), Lindbergh and Amundsen (people), the euro and the ATM (money).

## For Ilia: borderline set aside calls

1. **Mendeleev's periodic table** (science_024): set aside as a Russian person, although the periodic table is in the science scope. A good medium question if you want it back.
2. **The Cuban Missile Crisis** (history20_050): half a Soviet subject; set aside the strict way, like HBO's Chernobyl.
3. **Tamerlane's tomb** (mysteries_032): in Samarkand, but the story is the Soviet expedition of June 1941.
4. **Kandinsky** (art_031): Russian-born, worked in Germany and France.

## Also changed

- `ClosestValueTests.TheClosestGuessGetsTheBonusEvenWithZeroDistancePoints` gave up (inconclusive) when the bank dealt it a question whose answer sits near the bottom of its slider. It now places its guesses on whichever side of the answer has room, so it tests every question it can be dealt.
