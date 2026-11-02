// Screen `lobby` (06): "Ты в игре! / You're in!", the players as character chips, Start on every phone.
// Below it, the TEMPORARY Day 1 test pad (A to D).

import { useLayoutEffect } from 'preact/hooks';
import type { AnswerClock } from '../answerClock';
import { characterColor } from '../characters';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { lobbyData, type Lang, type ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  clock: AnswerClock;
  startInput: TrackedInput | null;
  padInput: TrackedInput | null;
  onStart(): void;
  onChoice(choice: number): void;
}

// TEMPORARY (Day 1 test pad): the answer colors from 06. Remove when answerChoice exists (Day 2).
const PAD = [
  { key: 'answer.a', color: '#E74C3C' },
  { key: 'answer.b', color: '#3498DB' },
  { key: 'answer.c', color: '#F1C40F' },
  { key: 'answer.d', color: '#2ECC71' },
] as const;

export function LobbyScreen(props: Props) {
  const { lang, screen, clock } = props;
  const { players, canStart } = lobbyData(screen.data);

  // TEMPORARY (Day 1 test pad): the options appear with this screen; the clock starts once per phaseId.
  useLayoutEffect(() => {
    clock.shown(screen.phaseId);
  }, [clock, screen.phaseId]);

  return (
    <div class="screen lobby">
      <h1>{t(lang, 'lobby.title')}</h1>

      <ul class="chips">
        {players.map((p) => (
          <li key={p.playerId} class="chip" style={{ background: characterColor(p.characterId) }}>
            {p.name}
          </li>
        ))}
      </ul>

      <div class="start-block">
        <PressButton class="primary big" disabled={!canStart} onClick={props.onStart}>
          {t(lang, 'lobby.start')}
        </PressButton>
        {canStart ? (
          <InputStatus lang={lang} input={props.startInput} />
        ) : (
          <div class="input-status muted">{t(lang, 'lobby.waitingPlayers')}</div>
        )}
      </div>

      {/* TEMPORARY Day 1 test pad: sends `input` kind `choice` on pointerdown. */}
      <section class="test-pad">
        <p class="test-pad-label">{t(lang, 'lobby.testPad')}</p>
        <div class="pad-grid">
          {PAD.map((b, i) => (
            <PressButton key={b.key} class="pad" style={{ background: b.color }} onPress={() => props.onChoice(i)}>
              {t(lang, b.key)}
            </PressButton>
          ))}
        </div>
        <InputStatus lang={lang} input={props.padInput} />
      </section>
    </div>
  );
}
