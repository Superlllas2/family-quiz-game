# 20 Playtest plan

Status: Done for game night 1 (Nov 2, 2026; revised after the spec review, `decisions/0002_spec_review.md`). Background and reasons: `research_party_quiz_fun.md`, section "Family game nights prove fun only when you measure behavior".

## Before the game night

- Charge the laptop, test the HDMI connection and the TV sound.
- Run one full game with two of your own phones and extra browser tabs on the laptop (`?room=ABCD&tab=2`, `&tab=3`, ...) on the WiFi you will use.
- Before the first game with friends, press Tab on the laptop and use Reset seen questions, so the rehearsal does not use up the questions.
- Print or open the observation sheet. Ask one friend who is not playing to fill it in, or record audio with everyone's consent.
- Read the same short intro every time: "We are testing the game, not you. Be honest; problems help us more than praise."

## Observation sheet (per round)

| Round | Laughs (what caused them) | Frustration ("that's unfair") | Confusion (what was unclear) | Waiting (phones down, side talk; in which phase) | Does last place still lean in? |
| --- | --- | --- | --- | --- | --- |
| 1 | | | | | |
| 2 | | | | | |
| 3 | | | | | |
| 4 | | | | | |
| Final | | | | | |

When the staging grey box or the set is in the build, also note per round: which shots felt slow, which cut was confusing, and whether everyone could read the answer cards from the sofa. With the 3D set on, also note whether the lights coming up at the start felt like the show beginning or just slow (`staging.intro.lightsUpSeconds`), and whether the host solo shots in the Intro and FinalIntro were fun or felt like waiting. Those values were picked by the AI as first guesses (D94, D97).

## The poll (on the phones, after Results, before any group talk)

Stored in `content/poll.json`. Anonymous: the log stores answers without player ids. The poll closes when every connected phone has submitted, or after 90 s (`poll.maxSeconds`).

| id | Question (RU / EN) | Kind |
| --- | --- | --- |
| `fun` | Самый весёлый момент? / The most fun moment? | text |
| `frustrating` | Что раздражало или было непонятно? / What was frustrating or confusing? | text |
| `waiting` | Когда ты ждал без дела? / When were you waiting with nothing to do? | choice: Никогда / Never; Между вопросами / Between questions; Во время результатов / During results; В финале / In the final |
| `time` | Хватало времени на ответ? / Was there enough time to answer? | choice: Слишком мало / Too little; В самый раз / Just right; Слишком много / Too much |
| `tvMissed` | Пропускал, что происходит на экране, из за телефона? / Did you miss what happened on the TV because of your phone? | choice: Часто / Often; Иногда / Sometimes; Нет / No |
| `unfair` | Была ли атака нечестной? / Was any attack unfair? | choice: Да / Yes; Немного / A bit; Нет / No |
| `again` | Сыграешь ещё в следующий раз? / Play again next game night? | choice: Да / Yes; Может быть / Maybe; Нет / No |

## After the game night

1. Copy the log folder and the poll results into `playtests/2026-11-07/` (or the actual date) in the repo.
2. Run `tools/analyze_log` and write `playtests/<date>/notes.md`: idle share (reported, no target for game night 1), longest idle stretch outside Intro, FinalIntro and Results (starting limit 11 s, D56), phase durations with any staging extras, the observation sheet, poll answers, and the top 3 problems.
3. For each of the three open questions (D15 power ups on the phone, D14 targeting, D16 timer), write keep, change or drop with the evidence.
4. Every timer and staging duration is a starting value (D48). Write which ones to change, with the evidence, and change them only in `content/config.json`.
