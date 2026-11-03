// Screen `answerSlider` (04 closest value, 06): a horizontal slider across the full width from min to max, a big
// number above it with the unit, minus and plus for one step, and Lock. No question text: it is on the TV (D88).
// The slider starts at the grid value nearest the middle and only ever holds grid values (sliderValue.ts). Lock sends
// the value; no changing after Lock.
// The answer clock starts when this screen is first drawn for a phaseId; a last call (same phaseId) keeps both the
// clock and the slider where they are (app.tsx mounts this screen with key = phaseId). The countdown is in the top bar.

import { useLayoutEffect, useMemo, useState } from 'preact/hooks';
import { screenDrawn, type AnswerClock } from '../answerClock';
import { t } from '../i18n';
import type { TrackedInput } from '../net/inputs';
import { answerSliderData, type Lang, type ScreenState } from '../protocol';
import { formatValue, sliderGrid, startIndex, stepIndex, valueAt } from '../sliderValue';
import { StepIcon } from './Icons';
import { InputStatus } from './InputStatus';
import { PressButton } from './PressButton';

interface Props {
  lang: Lang;
  screen: ScreenState;
  clock: AnswerClock;
  /** The value locked on this phaseId, if any. */
  locked: number | null;
  valueInput: TrackedInput | null;
  onLock(value: number): void;
}

export function AnswerSliderScreen(props: Props) {
  const { lang, screen, clock, locked } = props;
  const d = answerSliderData(screen.data);
  const grid = useMemo(() => sliderGrid(d.min, d.max, d.step), [d.min, d.max, d.step]);
  const [index, setIndex] = useState(() => startIndex(grid));

  // The slider is drawn now: start the clock (only the first time for this phaseId).
  useLayoutEffect(() => {
    screenDrawn(clock, screen);
  }, [clock, screen.phaseId, screen.id]);

  const done = locked !== null;
  const value = locked ?? valueAt(grid, index);
  const shown = formatValue(value);
  const unit = d.unit.trim();

  return (
    <div class="body slider-screen">
      <p class="slider-value">
        <span class="slider-number">{shown}</span>
        {unit ? <span class="slider-unit">{unit}</span> : null}
      </p>
      <input
        class="slider"
        type="range"
        min={0}
        max={grid.last}
        step={1}
        value={Math.min(grid.last, index)}
        disabled={done}
        aria-valuetext={unit ? `${shown} ${unit}` : shown}
        onInput={(e) => setIndex(Number((e.currentTarget as HTMLInputElement).value))}
      />
      <div class="slider-ends">
        <span>{formatValue(valueAt(grid, 0))}</span>
        <span>{formatValue(valueAt(grid, grid.last))}</span>
      </div>
      <div class="slider-steps">
        <PressButton
          class="step-button"
          ariaLabel={t(lang, 'slider.minus')}
          disabled={done}
          onPress={() => setIndex((k) => stepIndex(grid, k, -1))}
        >
          <StepIcon kind="minus" />
        </PressButton>
        <PressButton
          class="step-button"
          ariaLabel={t(lang, 'slider.plus')}
          disabled={done}
          onPress={() => setIndex((k) => stepIndex(grid, k, 1))}
        >
          <StepIcon kind="plus" />
        </PressButton>
      </div>
      <div class="bottom-block">
        <PressButton class="primary big" disabled={done} onPress={() => props.onLock(valueAt(grid, index))}>
          {t(lang, 'slider.lock')}
        </PressButton>
        <InputStatus lang={lang} input={props.valueInput} />
      </div>
    </div>
  );
}
