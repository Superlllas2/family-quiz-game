# Decision record 0007: TV text and the TV font

Date: Nov 2, 2026. Decided by Ilia, answering the blockers found before Day 1. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D65 | Where the TV app's own Russian and English texts live | `content/ui_strings.json`: every TV label and every text the TV app sends to phones (`countdown.text`, `getReady.text`, `results.awards`, `error.text`) is a key there with `ru` and `en`. Schema in `15_schemas.md`; `tools/validate_content.mjs` checks it. No Unity Localization package | |
| D66 | TV font | Inter (SIL Open Font License 1.1, Latin and Cyrillic) as the placeholder for all TV text in TextMeshPro. **Open:** may change when the visual style guide (10) picks the fonts | TextMeshPro's default font, which has no Cyrillic |

## Details added while applying these (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| Keys are dotted lower camel case: `tv.*` for text drawn on the TV, `screen.*` for text the TV app sends to phones. Placeholders use braces like host lines (`{name}`, `{min}`, `{max}`) and must be the same in both languages | 15, validator |
| The file starts with the keys Days 1 to 3 need. Builders add keys as they need them, in the same commit as the code, and never hard code a player facing text | `content/ui_strings.json`, prompts |
| Texts the phone page draws on its own (button labels, reconnecting, the error messages for `refused` reasons and close code 4002) stay in the phone locale files | 06 |
| Inter's font files and `OFL.txt` go into `unity/Assets/Fonts/Inter/`, downloaded from the official release (github.com/rsms/inter). A TextMeshPro font asset covers Latin and Cyrillic (А to я, Ё, ё) plus digits and punctuation, and is the default TMP font | 13, day 1 prompt |
| The phone page keeps the system font for now | 06 |
