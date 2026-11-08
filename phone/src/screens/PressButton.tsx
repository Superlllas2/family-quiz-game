// A button with touch feedback on pointerdown, never on click (06): the pressed look comes at once, with no wait for
// the network or a re-render.
//
// onPress counts on release (D143, pressRelease.ts): only when the finger lifts inside the button. Sliding off before
// lifting, or a pointercancel, cancels it: the pressed look goes and nothing runs. The answer clock is read inside
// onPress (app.tsx `decide`), so an answer's `ms` is the moment it counts. Enter or Space press at once.
// onClick is for form buttons that may act on any click.
//
// No hooks: the press state lives on the element, so screens stay plain functions that tests can call.

import type { ComponentChildren, CSSProperties } from 'preact';
import { PRESS_IDLE, insideRect, keyboardPress, pressStep, type PressEvent, type PressState } from '../pressRelease';

interface Props {
  class?: string;
  style?: CSSProperties;
  disabled?: boolean;
  type?: 'button' | 'submit';
  ariaPressed?: boolean;
  ariaLabel?: string;
  onPress?: () => void;
  onClick?: () => void;
  children: ComponentChildren;
}

/** The press in progress on each button element. */
const presses = new WeakMap<Element, PressState>();

/** Whether the pointer is inside the button's box. */
function inside(e: PointerEvent): boolean {
  const el = e.currentTarget as HTMLElement;
  return insideRect(el.getBoundingClientRect(), e.clientX, e.clientY);
}

export function PressButton(props: Props) {
  const feed = (e: PointerEvent, event: PressEvent): void => {
    const el = e.currentTarget as HTMLElement;
    const step = pressStep(presses.get(el) ?? PRESS_IDLE, event);
    presses.set(el, step.state);
    el.classList.toggle('pressed', step.pressed);
    if (step.fire) props.onPress?.();
  };
  const disabled = !!props.disabled;

  const down = (e: PointerEvent): void => {
    feed(e, { kind: 'down', pointer: e.pointerId, primary: e.button === 0, disabled });
    // Every pointer is captured, so the lift comes here even outside the button, where the box decides.
    if (presses.get(e.currentTarget as Element)?.pointer === e.pointerId) {
      try {
        (e.currentTarget as HTMLElement).setPointerCapture?.(e.pointerId);
      } catch {
        // The pointer is already gone: its up or cancel follows, or pointerleave covers it.
      }
    }
  };

  // Only the finger holding the button is measured: a mouse moving over it reads no layout.
  const move = (e: PointerEvent): void => {
    if (presses.get(e.currentTarget as Element)?.pointer !== e.pointerId) return;
    feed(e, { kind: 'move', pointer: e.pointerId, inside: inside(e) });
  };

  return (
    <button
      type={props.type ?? 'button'}
      class={props.class ? `btn ${props.class}` : 'btn'}
      style={props.style}
      disabled={props.disabled}
      aria-pressed={props.ariaPressed}
      aria-label={props.ariaLabel}
      onPointerDown={down}
      onPointerMove={move}
      onPointerUp={(e) => feed(e, { kind: 'up', pointer: e.pointerId, inside: inside(e), disabled })}
      onPointerCancel={(e) => feed(e, { kind: 'cancel', pointer: e.pointerId })}
      onPointerLeave={(e) => feed(e, { kind: 'leave', pointer: e.pointerId, inside: inside(e) })}
      onClick={(e) => {
        if (props.onPress && keyboardPress(e.detail, disabled)) props.onPress();
        props.onClick?.();
      }}
    >
      {props.children}
    </button>
  );
}
