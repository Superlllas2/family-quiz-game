# Decision record 0028: a second language on the TV

Date: Nov 12, 2026. Ilia compared four layouts for showing Russian and English together on the TV (artifact "Two-Language Question Card": A under the line, B mirror, C subtitles, D import sticker) and picked A. He then decided the settings and the host lines in the same conversation. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D163 | The language settings on the TV | Two rows in the TV settings panel: **the main language** (RU or EN, the game language as before, default Russian) and a **second language** toggle, on or off. The second language is the other one of the two. Both take effect at once | The single "Game language" row of `02_show_flow.md` |
| D164 | How the second language looks ("Under the line") | With the second language on, the question card shows the main language on top in the display face as now, then a dashed line with a small language chip (`EN` or `RU`), then the second language under it in Nunito Black, smaller and in a dark cobalt. The same on the "Why?" fact, the race statement and the topic ribbon (the second name after a dot, smaller and lighter). At Reveal every answer tile shows the main text on one line and the second text on a smaller line under it. With the second language off, everything looks as before | |
| D165 | Topic names | Topic doors in the vote and the topic ribbon show the second name too when the second language is on | |
| D166 | Host lines | With the second language **off**, host lines show in the main language. With it **on**, host lines show in the **second** language: main Russian with the second language on gives English host lines; main English with the second language on gives Russian host lines | "Lines show in the game language" (`15_schemas.md`, host lines) |

**Phones are unchanged:** each phone keeps its own language for its buttons (D3, D88). The phones never show the question (D162).

**Not part of this record:** host voices (D4: Russian voices with English subtitles, later); a layout check for the longest texts in the bank at the new sizes (part of building it).

## Answers (Ilia, Nov 12, 2026)

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D167 | The second language's default and memory | Off by default, from `language.secondLanguage` (false) in `content/config.json`. A change in the settings panel holds for every following game while the TV app runs, and is reset to the config default when the app starts again (not saved to disk) | |
| D168 | TV labels | TV labels (`tv.*` in `ui_strings.json`: "Почему?", "Верно!", ribbons, round titles) show in the main language only. To revisit after playtest feedback | |
