// Screen `answerChoice` (06): four big buttons 2x2 with the option texts, each a quarter of the space below the top
// bar. No question text: it is on the TV (D88).
// The answer clock starts when this screen is first drawn for a phaseId; a last call (same phaseId, smaller
// `endsInMs`) only moves the countdown. One tap locks: the pressed button stays highlighted, the others dim,
// and later taps are ignored. The countdown lives in the top bar (app.tsx).

import { useLayoutEffect } from 'preact/hooks';
import { screenDrawn, type AnswerClock } from '../answerClock';
import { t, type TextKey } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { answerChoiceData, type Lang, type ScreenState } from '../protocol';
import { AnswerShape, type AnswerShapeKind } from './Icons';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

/**
 * Answer letters and shapes from 06 (general rules), in display order A to D. The colors are the printed inks the TV
 * uses too; they live in one place, the --answer-a to --answer-d variables in style.css (class ans-a to ans-d).
 */
const ANSWERS: ReadonlyArray<{ letter: TextKey; tone: string; shape: AnswerShapeKind }> = [
  { letter: 'answer.a', tone: 'ans-a', shape: 'triangle' },
  { letter: 'answer.b', tone: 'ans-b', shape: 'diamond' },
  { letter: 'answer.c', tone: 'ans-c', shape: 'circle' },
  { letter: 'answer.d', tone: 'ans-d', shape: 'square' },
];

interface Props {
  lang: Lang;
  screen: ScreenState;
  clock: AnswerClock;
  /** The locked choice for this phaseId, if any. */
  chosen: number | null;
  choiceInput: TrackedInput | null;
  onChoose(choice: number): void;
}

export function AnswerChoiceScreen(props: Props) {
  const { lang, screen, clock, chosen } = props;
  const { options } = answerChoiceData(screen.data);

  // The options are drawn now: start the clock (only the first time for this phaseId).
  useLayoutEffect(() => {
    screenDrawn(clock, screen);
  }, [clock, screen.phaseId, screen.id]);

  return (
    <div class="answer-grid">
      {ANSWERS.slice(0, options.length).map((a, i) => {
        const state = chosen === null ? '' : chosen === i ? ' chosen' : ' dim';
        return (
          <PressButton
            key={a.letter}
            class={`answer ${a.tone}${state}`}
            onPress={() => props.onChoose(i)}
          >
            <span class="answer-mark">
              <span class="answer-letter">{t(lang, a.letter)}</span>
              <AnswerShape shape={a.shape} />
            </span>
            <span class="answer-text">{options[i]}</span>
            {chosen === i ? <InputStatus lang={lang} input={props.choiceInput} class="answer-status" /> : null}
          </PressButton>
        );
      })}
    </div>
  );
}
