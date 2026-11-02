# Decision record 0006: a realistic world with cartoon guests

Date: Nov 2, 2026. Decided by Ilia. The AI building the game never reopens one of these on its own; it asks first. The staging that uses this look is in `21_staging.md`.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D62 | Art direction | Everything is realistic in style except the guest characters. The guests are cartoonish and may be mixed media: 2D billboards in the 3D space, or funny looking cartoonish 3D models | D43's "stylized, not photoreal" for everything but the guests; D45's "characters stylized with believable proportions and fabrics"; the 0005 detail that kept believable proportions for the hosts only. D45's matte materials, warm practical lights and broadcast feel still hold for the set |
| D63 | Hosts | Realistic, like the set, not cartoonish like the guests | |
| D64 | Medium per character, and how 2D characters are animated | Both decided later. How a 2D character is animated is decided only if a character becomes 2D. Until then testing uses grey 3D boxes (the grey box capsules) | |

## Details added while applying this (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| "Realistic" means physically based materials at real world scale, real lights and soft shadows, and the cinematic broadcast feel of D45, as far as the laptop runs it smoothly (the frame rate target is set in 10). It never means copying a real show's set (D49) | 21, 10 |
| The clash is the joke: the guests look like they walked in from a cartoon, but they are lit by the set's lights and cast shadows on it, so they sit in the scene instead of floating on top of it | 21 |
| Each guest is one of three media, chosen per character: a cartoonish 3D model; a 2D billboard (a flat drawn character on a card in the 3D space that turns to face the active camera); or mixed (for example a 3D body with a drawn 2D face) | 21 |
| 3D guests keep the Unity Humanoid rig, so the shared animations retarget onto them. 2D guests need every shared and signature animation drawn for them (15 per character), as flipbook frames or a cutout rig. A flipbook needs no package; Unity's 2D Animation package would need asking first (D64) | 21 |
| `content/characters.json` gets no new field yet. When Ilia picks a medium per character (D64), a `medium` field (`3d`, `2d` or `mixed`) is added and the schema in 15 updated | 21 ideas for later |
| The grey box for game night 1 does not change (capsules in chairs) | 21 |
