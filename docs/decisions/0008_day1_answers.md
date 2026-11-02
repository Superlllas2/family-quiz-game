# Decision record 0008: answers after Day 1

Date: Nov 2, 2026. Decided by Ilia, answering the open questions from the Day 1 build (relay, phone join, TV lobby). The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D67 | A tab closed with 4002 ("Game open in another tab"): does it keep the seat token? | No. The tab forgets the token for itself only (kept in its `sessionStorage`, so a reload does not bring it back), without deleting it from `localStorage`, because the newer tab holds the same token. Back leads to a fresh join; the old tab can never take the seat back on its own | |
| D68 | A phone connects while the TV app is away (TV socket closed, room not yet closed) | The relay closes it with 4004, the literal reading of `PROTOCOL.md` section 2. A phone that holds a token for the room shows reconnecting and retries every 2 s; one without a token shows "room not found" | |
| D69 | Where the phone timings (1 s resend, 3 s no connection, 2 s retry, 5 s ping) and the protocol limits (8 KB, 20 messages per second, 10 minute room close, 16 character names, 24 character tokens) live | Named constants in code, each citing its spec (`phone/src/constants.ts`, `relay/src/protocol.ts`, Core `ProtocolLimits`), not in `content/config.json`. They are protocol rules, not tuning values | |
| D70 | Which refusal wins when several apply to one `join` | `badVersion`, then `badName`, then `nameTaken`, then `roomFull` | |
| D71 | A `join` with no reply | Not resent automatically. After 3 s the join screen shows "Нет связи / No connection" and Join works again; a retry may get `nameTaken` if the first join did arrive | |
| D72 | The Retry button when the relay is down at start (`18_edge_cases.md`) | No button: the TV shows the relay down text and reconnects on its own every `net.tvReconnectSeconds` | The Retry button in `18_edge_cases.md` |
