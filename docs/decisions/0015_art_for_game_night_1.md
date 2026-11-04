# Decision record 0015: art for game night 1

Date: Nov 4, 2026. Decided by Ilia. The AI building the game never reopens one of these on its own; it asks first.

| # | Question | Decision | Replaces |
| --- | --- | --- | --- |
| D89 | How much art game night 1 gets | Option B: a lit set built in Blender (sofa, armchairs, desk, curtain stage, band riser, windows with a city view) with real materials, warm lights and a light film look, plus guests made of a colored capsule and one signature prop each, animated in code. It can grow later (band, more shots) if Ilia decides so | D51's "only a grey box" for game night 1 |
| D90 | Hosts for game night 1 | Two cubes at the desk with a small idle animation and a speaking animation while their bubble shows | D63's realistic hosts are postponed until after game night 1, not dropped |

## Details added while applying this (not yet confirmed by Ilia)

| Detail | Where |
| --- | --- |
| The look is chosen from three test renders of the Wide shot (warm late night, retro 70s, modern night), all original designs (D49) | Blender |
| The art lives on its own branch behind an on/off switch; the current views stay as the fallback. If it is not solid by the evening of Nov 5, the rehearsal and game night run without it | Presentation |
| Staging extras stay 0: the 3D scene only listens, no phase gets longer, the 11 s idle check still holds | Core, Presentation |
| No new packages: models come in as FBX, the film look uses URP's built in post processing, shots use plain cameras, guest and host animation is procedural code (no rigs) | Presentation |
| Reference pictures from Ilia stay out of the repo (D59) | |
