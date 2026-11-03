// The closest value slider (04 `closestValue`, 06 `answerSlider`). The slider works on a whole number index k, so
// every value it can send is min + k * step, inside [min, max] (the TV app refuses a value outside the range).
// No game rules here: the TV app scores the guess.

export interface SliderGrid {
  min: number;
  max: number;
  step: number;
  /** The highest index: the last grid value at or below max. */
  last: number;
  /** Decimals kept when computing a value, so 0.1 steps give 0.3, not 0.30000000000000004. */
  decimals: number;
}

/** Decimal places of a number as written, at most 10. */
function decimalsOf(x: number): number {
  const text = String(x);
  if (text.includes('e')) return 10;
  const dot = text.indexOf('.');
  return dot < 0 ? 0 : Math.min(10, text.length - dot - 1);
}

/** A broken step (0, negative, missing) becomes 1; a range with max below min holds only min. */
export function sliderGrid(min: number, max: number, step: number): SliderGrid {
  const s = Number.isFinite(step) && step > 0 ? step : 1;
  const last = max > min ? Math.floor((max - min) / s + 1e-9) : 0;
  return { min, max, step: s, last, decimals: Math.max(decimalsOf(min), decimalsOf(s)) };
}

function clampIndex(grid: SliderGrid, k: number): number {
  return Math.min(grid.last, Math.max(0, Math.round(k)));
}

/** The value at index k (clamped to the grid). */
export function valueAt(grid: SliderGrid, k: number): number {
  const value = grid.min + clampIndex(grid, k) * grid.step;
  return Number(value.toFixed(grid.decimals));
}

/** Where the slider starts: the grid value nearest the middle of the range; on a tie the upper one. */
export function startIndex(grid: SliderGrid): number {
  return clampIndex(grid, (grid.max - grid.min) / 2 / grid.step);
}

/** Plus (+1) or minus (-1): one step, stopping at both ends. */
export function stepIndex(grid: SliderGrid, k: number, delta: number): number {
  return clampIndex(grid, k + delta);
}

/** How a value is written: no thousands separator, at most 2 decimals (like the TV app). */
export function formatValue(value: number): string {
  const rounded = Number(value.toFixed(2));
  return String(rounded === 0 ? 0 : rounded);
}
