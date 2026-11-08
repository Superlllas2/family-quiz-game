import { describe, expect, it } from 'vitest';
import { PRESS_IDLE, insideRect, keyboardPress, pressStep, type PressEvent, type PressState } from './pressRelease';

/** Runs the events from idle; returns how many presses fired and whether the pressed look is on at the end. */
function run(...events: PressEvent[]): { fired: number; pressed: boolean; state: PressState } {
  let state = PRESS_IDLE;
  let fired = 0;
  let pressed = false;
  for (const e of events) {
    const step = pressStep(state, e);
    state = step.state;
    pressed = step.pressed;
    if (step.fire) fired++;
  }
  return { fired, pressed, state };
}

const down = (pointer = 1, disabled = false): PressEvent => ({ kind: 'down', pointer, primary: true, disabled });
const move = (inside: boolean, pointer = 1): PressEvent => ({ kind: 'move', pointer, inside });
const up = (inside: boolean, pointer = 1, disabled = false): PressEvent => ({ kind: 'up', pointer, inside, disabled });

describe('an answer counts when the finger lifts inside the button (D143)', () => {
  it('the pressed look comes on the touch, before anything is sent', () => {
    const step = pressStep(PRESS_IDLE, down());
    expect(step.pressed).toBe(true);
    expect(step.fire).toBe(false);
  });

  it('down, then up inside: one press, and the pressed look goes', () => {
    expect(run(down(), up(true))).toMatchObject({ fired: 1, pressed: false, state: PRESS_IDLE });
  });

  it('a finger that wobbles but stays inside still presses', () => {
    expect(run(down(), move(true), move(true), up(true)).fired).toBe(1);
  });

  it('down, slide off, up: nothing, and the pressed look goes the moment the finger leaves', () => {
    expect(pressStep(pressStep(PRESS_IDLE, down()).state, move(false)).pressed).toBe(false);
    expect(run(down(), move(false), up(false)).fired).toBe(0);
  });

  it('down, leave, up: nothing (pointerleave outside the button counts as sliding off)', () => {
    expect(run(down(), { kind: 'leave', pointer: 1, inside: false }, up(false)).fired).toBe(0);
  });

  it('a pointerleave while the finger is still inside the box (a browser quirk under capture) changes nothing', () => {
    expect(run(down(), { kind: 'leave', pointer: 1, inside: true }, up(true)).fired).toBe(1);
  });

  it('sliding off cancels for good: coming back before lifting does not press', () => {
    expect(run(down(), move(false), move(true), up(true)).fired).toBe(0);
  });

  it('lifting outside without a move event in between presses nothing', () => {
    expect(run(down(), up(false)).fired).toBe(0);
  });

  it('cancel (the browser took the touch): nothing', () => {
    expect(run(down(), { kind: 'cancel', pointer: 1 }, up(true))).toMatchObject({ fired: 0, pressed: false });
  });

  it('a disabled button never presses and never looks pressed', () => {
    expect(run(down(1, true), up(true, 1, true))).toMatchObject({ fired: 0, pressed: false });
  });

  it('a button disabled between the touch and the lift does not press', () => {
    expect(run(down(), up(true, 1, true)).fired).toBe(0);
  });

  it('only the main button counts (a right click is not a press)', () => {
    expect(run({ kind: 'down', pointer: 1, primary: false, disabled: false }, up(true)).fired).toBe(0);
  });

  it('events from another finger change nothing; the last finger down owns the press', () => {
    expect(run(down(1), move(false, 2), up(true, 2), up(true, 1)).fired).toBe(1);
    expect(run(down(1), down(2), up(true, 1), up(true, 2)).fired).toBe(1);
  });

  it('an up, move or cancel with no finger down does nothing', () => {
    expect(run(up(true), move(true), { kind: 'cancel', pointer: 1 })).toMatchObject({ fired: 0, pressed: false });
  });

  it('a second press after the first works the same way', () => {
    expect(run(down(), up(true), down(), up(true)).fired).toBe(2);
  });
});

describe('where the finger is', () => {
  const rect = { left: 10, top: 20, right: 110, bottom: 70 };

  it('inside the button\'s box, edges included', () => {
    expect(insideRect(rect, 50, 40)).toBe(true);
    expect(insideRect(rect, 10, 20)).toBe(true);
    expect(insideRect(rect, 110, 70)).toBe(true);
  });

  it('outside it', () => {
    expect(insideRect(rect, 9, 40)).toBe(false);
    expect(insideRect(rect, 111, 40)).toBe(false);
    expect(insideRect(rect, 50, 19)).toBe(false);
    expect(insideRect(rect, 50, 71)).toBe(false);
  });
});

describe('the keyboard still works (Enter or Space)', () => {
  it('a click from the keyboard (detail 0) presses at once', () => {
    expect(keyboardPress(0, false)).toBe(true);
  });

  it('the click that follows a finger or mouse lift (detail 1 or more) is ignored: the lift already counted', () => {
    expect(keyboardPress(1, false)).toBe(false);
    expect(keyboardPress(2, false)).toBe(false);
  });

  it('a disabled button never presses', () => {
    expect(keyboardPress(0, true)).toBe(false);
  });
});
