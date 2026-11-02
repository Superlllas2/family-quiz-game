// A button with touch feedback on pointerdown, never on click (06): the pressed look comes at once,
// with no wait for the network or a re-render.
//
// onPress runs on pointerdown (answer buttons send at once). onClick is for form buttons that may act on click.

import type { ComponentChildren, CSSProperties } from 'preact';

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

function release(e: PointerEvent): void {
  (e.currentTarget as HTMLElement).classList.remove('pressed');
}

export function PressButton(props: Props) {
  const down = (e: PointerEvent): void => {
    if (props.disabled || e.button !== 0) return;
    (e.currentTarget as HTMLElement).classList.add('pressed');
    props.onPress?.();
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
      onPointerUp={release}
      onPointerCancel={release}
      onPointerLeave={release}
      onClick={props.onClick}
    >
      {props.children}
    </button>
  );
}
