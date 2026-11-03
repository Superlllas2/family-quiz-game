// Screen `answerTrueFalse` (04 true or false, 06): two huge buttons, "Правда / True" in the top half (green) and
// "Ложь / False" in the bottom half (red). No statement text: it is on the TV (D88). One tap locks: the tapped button
// stays highlighted, the other dims, later taps are ignored. The answer clock starts when this screen is first drawn
// for a phaseId. The countdown is in the top bar (app.tsx).

import { useLayoutEffect } from 'preact/hooks';
import { screenDrawn, type AnswerClock } from '../answerClock';
import { t, type TextKey } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import type { Lang, ScreenState } from '../protocol';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

/** Placeholder colors from 06: green (answer D) for True, red (answer A) for False. */
const BUTTONS: ReadonlyArray<{ answer: boolean; text: TextKey; color: string }> = [
  { answer: true, text: 'trueFalse.true', color: '#2ECC71' },
  { answer: false, text: 'trueFalse.false', color: '#E74C3C' },
];

interface Props {
  lang: Lang;
  screen: ScreenState;
  clock: AnswerClock;
  /** The answer locked on this phaseId, if any. */
  chosen: boolean | null;
  answerInput: TrackedInput | null;
  onAnswer(answer: boolean): void;
}

export function AnswerTrueFalseScreen(props: Props) {
  const { lang, screen, clock, chosen } = props;

  useLayoutEffect(() => {
    screenDrawn(clock, screen);
  }, [clock, screen.phaseId, screen.id]);

  return (
    <div class="body true-false">
      <div class="tf-buttons">
        {BUTTONS.map((b) => {
          const state = chosen === null ? '' : chosen === b.answer ? ' chosen' : ' dim';
          return (
            <PressButton
              key={b.text}
              class={`answer tf-button${state}`}
              style={{ background: b.color }}
              onPress={() => props.onAnswer(b.answer)}
            >
              <span class="tf-text">{t(lang, b.text)}</span>
              {chosen === b.answer ? <InputStatus lang={lang} input={props.answerInput} class="answer-status" /> : null}
            </PressButton>
          );
        })}
      </div>
    </div>
  );
}
