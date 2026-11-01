# Decision record 0001: initial decisions

Date: Nov 1 to 2, 2026. Decided by Ilia. The AI building the game never reopens one of these on its own; it asks first.

| # | Decision | Choice | Reason |
| --- | --- | --- | --- |
| D1 | Audience | Family first, built to be sellable | Test with family, keep licensing clean for a later sale |
| D2 | TV app engine | Unity 6.3 LTS | Ilia's skills; 3D hosts possible later |
| D3 | Languages | Russian and English; game language set by the host phone; each phone picks its own language | Mixed language family and a future market |
| D4 | Host voice | Russian voice, English subtitles on the TV when switched on; text only in version 1 | Hosts are Russian speakers |
| D5 | How phones connect | Cloud relay over HTTPS (WebSocket) | Joins from any WiFi, keeps phones awake, remote relatives can join |
| D6 | Internet | Required for version 1 | Follows from D5; a home WiFi mode can come later behind the same transport interface |
| D7 | Phone page library | Preact with TypeScript, built with Vite | React style code AI tools write well, tiny size |
| D8 | Players per game | 2 to 8 | Tested at 3 and at 6 |
| D9 | Platforms | macOS first, Windows build when needed | Ilia's MacBook first; development also happens on Windows |
| D10 | Host look | 2D portraits later; text bubbles in version 1 | Art comes after the prototype |
| D11 | Host names | Konstantin and Ilja | Written consent before recording or voice cloning |
| D12 | Question base | Own questions built on Wikidata facts | Cleanest licensing for a game that may be sold |
| D13 | Version 1 scope | Technical prototype: full loop, placeholders, real questions | Learn pace and fun before art |
| D14 | Targeting | Free targeting by default; "aim up only" is a setting | Ganging up is part of the fun |
| D15 | Power ups on the phone | Kept for version 1, tested on the first game night | Unsure whether it pulls attention off the TV |
| D16 | Question timer | 15 s answer window, about 25 s per cycle, tested on the first game night | Ilia suspects it is too fast |
| D17 | Question formats in version 1 | Multiple choice and closest value in a range; clue by clue as a stretch goal | Variety in pace from the start |
| D18 | Relay host | Cloudflare Workers with one Durable Object per room | Free plan covers a game night many times over |
| D19 | Game night 1 target | Weekend of Nov 7 and 8, 2026, with friends | Ilia's plan |
| D20 | Game night 1 scope | See `docs/19_acceptance.md`; mini game and clue by clue only if time allows | Six days of build time |
