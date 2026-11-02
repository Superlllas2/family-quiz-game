# Day 0: setup (Monday evening, about 1 hour, done by you)

Do these once before Tuesday. Claude Code cannot do steps 1 to 4 for you.

## 1. Tools

- **Unity 6.3 LTS** through Unity Hub, with the modules "Mac Build Support" (on the Mac) or "Windows Build Support" (on Windows). Note the exact version number, for example `6000.3.2f1`.
- **Node.js 20 or newer** from nodejs.org. Check with `node -v`.
- **Git** (you have it).
- **Claude Code** (you have it).

## 2. Unity project

The `unity/` folder in the repo only holds empty placeholder folders. Replace it with a real project:

1. Delete the `unity` folder in the repo.
2. In Unity Hub: New project, template **Universal 3D** (done Nov 2 with Unity 6000.3.23f1), project name `unity`, location: the repo folder (`D:\Unity Projects\family-quiz-game` on Windows). Create.
3. Close Unity once it has opened.
4. Commit: `git add .` then `git commit -m "Create Unity project"`.

Claude Code recreates the `Assets/Game/...` folders on Tuesday.

## 3. Cloudflare

1. Make a free account at cloudflare.com.
2. In a terminal: `npx wrangler login` and approve in the browser.

## 4. Phones

Have one iPhone and one Android phone ready for testing (friends' or family's are fine).

## 5. Read and check the specs (30 minutes)

Read these and write down anything that is wrong or that you want different:

- `docs/02_show_flow.md`: the order of the show
- `docs/03_rules.md`: scoring and power ups
- `docs/06_phone_screens.md`: what the phone shows
- `content/questions.json` and `content/race.json`: skim the questions; fix or replace any you dislike

Then tell Claude (in the Family Quiz chat) what to change before Tuesday.
