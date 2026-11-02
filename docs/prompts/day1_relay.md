# Day 1 prompt: relay, phone join, Unity connection (Tuesday)

Copy everything in the box into Claude Code, started in the repo folder.

```
Read CLAUDE.md, docs/13_architecture.md, docs/PROTOCOL.md, docs/06_phone_screens.md, docs/decisions/0002_spec_review.md and docs/19_acceptance.md (section "Day 1") fully before starting.

Goal for today: a phone joins a room by QR link and its taps reach the Unity TV app, with acks back to the phone, through the Cloudflare relay. This proves the riskiest part of the architecture. There is no host phone and there are no bots: every player has the same rights.

Build, in this order, on a branch called day1-relay:

1. relay/: a Cloudflare Worker with one Durable Object per room, in TypeScript, exactly as in PROTOCOL.md section 2: URLs, envelope, room codes, tvSecret, close codes 4001, 4002 and 4004, the TV control message {"close": "<conn>", "code": 4002}, tvAway and tvBack to phones when the TV socket closes and returns, one "connected" message per open phone socket after a TV reconnects, 8 KB limit, 20 messages per second per phone with extra messages dropped (never closed), phone messages dropped (not buffered) while no TV is connected, room closes 10 minutes after the TV leaves. It also serves the built phone page as static assets. Write Vitest tests for: forwarding only within a room, the TV secret rule, the phone rejected when no TV is present, tvAway and tvBack, the close control message, and the rate limit dropping without closing.

2. phone/: a Preact + TypeScript + Vite page. Today it only needs: the join screen (room code from ?room=, name with the rules in PROTOCOL.md section 3, RU/EN toggle defaulting to Russian), the reconnect logic with the token in localStorage including the &tab=<n> key for testing with several tabs (PROTOCOL.md sections 4 and 5), the reconnecting screen on tvAway and rejoin on tvBack, the 4002 "open in another tab" error without reconnecting, the lobby screen, a temporary test screen with four buttons A to D that sends `input` messages of kind `choice` with inputId, phaseId and ms, and shows "Sent" then a check mark on ack. Locale files phone/src/locales/ru.json and en.json. Wake lock after the first tap. Touch feedback on pointerdown.

3. unity/: add the package com.unity.nuget.newtonsoft-json (approved in docs/13_architecture.md). Create the assembly definitions and folders from docs/13_architecture.md (Core with No Engine References and Override References listing only Newtonsoft.Json.dll, Net, Presentation, Editor, Tests). In Net: RelayClient using System.Net.WebSockets.ClientWebSocket on a background task, a thread safe MessageQueue drained on the main thread, and protocol DTOs serialized with Newtonsoft. In Core: just enough to accept join, rejoin, leave and input commands and keep a player list (name rules, nameTaken, roomFull, the first free character on join). When a seat that already has an open socket is rejoined, send the relay the close control message for the older socket. In Presentation (UGUI with TextMeshPro): add the Inter font and its OFL.txt in Assets/Fonts/Inter/ and make a TextMeshPro font asset with Latin and Cyrillic the default TMP font (docs/13_architecture.md, D66). TV texts come from content/ui_strings.json (D65), never from code. A plain lobby view on the TV showing the room code, a QR code for https://<relayHost>/?room=CODE, and the joined players. Show the last input received per player on screen, and send ack back. Read relayHost from Assets/StreamingAssets/local.json (add local.example.json to git; local.json is already in .gitignore).

4. Deploy the relay with `npx wrangler deploy` and put the URL in local.json.

Rules: follow CLAUDE.md. Do not add packages beyond those named in docs/13_architecture.md without asking me. Run the relay and phone tests from the command line. Commit after each green step.

When done, give me: the relay URL, how to start everything, how to open extra browser tabs as players, and a checklist of the manual Day 1 checks from docs/19_acceptance.md for me to try with my phones.
```

## Your manual checks after it finishes

- [ ] Phone opens the QR link and joins; name shows on the TV within 1 s
- [ ] Taps on A to D show on the TV, and the phone shows a check mark quickly
- [ ] Lock the phone for 60 s, unlock: it reconnects and keeps its seat (iPhone and Android)
- [ ] Two phones at once work, and two laptop tabs with `&tab=2` and `&tab=3` join as two more players
- [ ] Stop the TV app: phones show Reconnecting; start it again: they come back

If something fails, paste the error to Claude Code first. If it is stuck after two tries, tell Claude in the Family Quiz chat.
