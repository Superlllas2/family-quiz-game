# 01 Vision and pillars

Status: Done (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). Live doc: https://claude.ai/artifact/WMgZzMWcpUEjSvjmDHXaTc

## Pitch

A fast, chaotic family quiz show in the living room: the TV is the stage, every phone is a controller, and two hosts, Konstantin and Ilja, tease the players while they answer. It is built for our family first and made so it can be sold later.

## Players and setting

- 2 to 8 players of mixed ages, kids to grandparents, in one living room.
- Relatives in another city can join through a video call, since phones connect through a cloud relay.
- One laptop runs the game and is connected to the TV over HDMI.

## Session length

In game night 1 a Standard game lasts about 10 to 12 minutes, without mini games. With the 3 mini games planned for later it lasts 15 to 19 minutes. Quick and Long modes exist in config. An evening holds two or three games.

## Look

Stylized real time 3D, not photoreal (D43): an original late night talk show set with matte, textured materials and warm practical lights, never plastic or toy like (D44, D45, D49). The players sit on the set as guests and raise answer cards, the two hosts sit at the desk, and a small live band plays the music cues. The question UI sits over the 3D scene; the phone stays a flat 2D controller. Staging is in `21_staging.md`. Game night 1 uses the flat UI over the 3D camera, with a grey box version of the set as a stretch goal.

## Languages

- Russian and English.
- The game language (TV text, host text) is set in the TV settings panel on the laptop. Default: Russian. No phone has special rights.
- Each player picks their own language for their phone in the phone settings. Default: the game language.
- Host voices, when they exist, are in Russian, with English subtitles on the TV when switched on. Version 1 has text only hosts.

## The five pillars

1. **Never wait:** things happen at the same time, and a phase ends the moment everyone is done.
2. **Everyone can win:** speed counts only a little, and last place can still win the final race.
3. **Every tap feels good:** the phone reacts on touch; celebrations match what the player did.
4. **Hosts react, never block:** host lines never hold up the game and never repeat in an evening.
5. **Fun is measured:** every build logs events, and every game night ends with a phone poll.

## Sabotage is part of the fun

Players may gang up on anyone, including the leader or a relative, the way they do in Knowledge is Power. Fairness comes from scoring and the final race, not from limiting who you can attack. "Aim up only" exists as a setting, off by default.

## Like these, but

- Like Knowledge is Power, but without the waiting between questions
- Like Jackbox, but one continuous TV show with two hosts we know
- Like Kahoot, but made for a sofa and a family, not a classroom

## What it is not (for now)

An online game with accounts, a mobile app to install, a photoreal game, or a game with user generated question packs.
