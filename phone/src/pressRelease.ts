// Answers count on release (D143, 06 "General rules"): the button looks pressed the moment a finger touches it
// (pillar 3), but the press counts only when that finger lifts inside the button. Sliding off before lifting cancels
// it for good (the pressed look goes at once, nothing is sent), and so does a pointercancel. PressButton feeds this the
// pointer events and decides "inside" from the button's box: on phones the touch is captured by the button, so
// pointerleave is not reliable and the finger's position is what counts.
//
// The keyboard (Enter or Space) still works: it gives a click with detail 0, which presses at once. The click that
// follows a finger or mouse lift (detail 1 or more) is ignored, because the lift already counted.

export interface PressState {
  /** The pointer (finger or mouse) holding the button down, or null. */
  readonly pointer: number | null;
}

export const PRESS_IDLE: PressState = { pointer: null };

export type PressEvent =
  /** pointerdown; `primary`: the main button (a finger, a pen, a left click). */
  | { kind: 'down'; pointer: number; primary: boolean; disabled: boolean }
  /** pointermove, with whether the finger is still inside the button. */
  | { kind: 'move'; pointer: number; inside: boolean }
  /** pointerleave, with whether the finger is still inside the box (only outside counts as sliding off). */
  | { kind: 'leave'; pointer: number; inside: boolean }
  /** pointerup, with whether the finger lifted inside the button. */
  | { kind: 'up'; pointer: number; inside: boolean; disabled: boolean }
  /** pointercancel: the browser took the touch. */
  | { kind: 'cancel'; pointer: number };

export interface PressStep {
  state: PressState;
  /** Whether the button looks pressed after this event. */
  pressed: boolean;
  /** True on the one event where the press counts. */
  fire: boolean;
}

export function pressStep(s: PressState, e: PressEvent): PressStep {
  const held = s.pointer !== null;
  const unchanged: PressStep = { state: s, pressed: held, fire: false };
  const idle: PressStep = { state: PRESS_IDLE, pressed: false, fire: false };
  switch (e.kind) {
    case 'down':
      // The last finger down owns the press: a lost up from an earlier one never leaves the button stuck.
      if (e.disabled || !e.primary) return held ? unchanged : idle;
      return { state: { pointer: e.pointer }, pressed: true, fire: false };
    case 'move':
    case 'leave':
      if (e.pointer !== s.pointer) return unchanged;
      return e.inside ? unchanged : idle;
    case 'cancel':
      return e.pointer === s.pointer ? idle : unchanged;
    case 'up':
      if (e.pointer !== s.pointer) return unchanged;
      return { state: PRESS_IDLE, pressed: false, fire: e.inside && !e.disabled };
  }
}

export interface Box { left: number; top: number; right: number; bottom: number }

/** Whether the point (client px) is inside the box, edges included. */
export function insideRect(box: Box, x: number, y: number): boolean {
  return x >= box.left && x <= box.right && y >= box.top && y <= box.bottom;
}

/** Whether a click presses: only one from the keyboard (detail 0) on a button that is not disabled. */
export function keyboardPress(detail: number, disabled: boolean): boolean {
  return detail === 0 && !disabled;
}
