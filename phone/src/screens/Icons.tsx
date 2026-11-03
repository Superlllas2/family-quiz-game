// Drawn marks (no emoji, no characters standing in for icons).

export function Spinner(props: { large?: boolean }) {
  return <span class={props.large ? 'spinner spinner-large' : 'spinner'} aria-hidden="true" />;
}

export function CheckMark(props: { label: string }) {
  return (
    <svg class="check" viewBox="0 0 24 24" role="img" aria-label={props.label}>
      <path d="M4 12.5l5 5L20 6.5" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />
    </svg>
  );
}

export function GearIcon() {
  return (
    <svg class="gear" viewBox="0 0 24 24" aria-hidden="true">
      <path
        fill="currentColor"
        fill-rule="evenodd"
        d="M10.3 2h3.4l.5 2.6a7.6 7.6 0 0 1 1.9 1.1l2.5-.9 1.7 2.9-2 1.7a7.7 7.7 0 0 1 0 2.2l2 1.7-1.7 2.9-2.5-.9a7.6 7.6 0 0 1-1.9 1.1l-.5 2.6h-3.4l-.5-2.6a7.6 7.6 0 0 1-1.9-1.1l-2.5.9-1.7-2.9 2-1.7a7.7 7.7 0 0 1 0-2.2l-2-1.7 1.7-2.9 2.5.9a7.6 7.6 0 0 1 1.9-1.1zM12 8.6a3.4 3.4 0 1 0 0 6.8 3.4 3.4 0 0 0 0-6.8z"
      />
    </svg>
  );
}

export type AnswerShapeKind = 'triangle' | 'diamond' | 'circle' | 'square';

/** The shape that goes with each answer letter (06): A triangle, B diamond, C circle, D square. */
export function AnswerShape(props: { shape: AnswerShapeKind }) {
  return (
    <svg class="answer-shape" viewBox="0 0 24 24" aria-hidden="true">
      {props.shape === 'triangle' && <polygon points="12,3 22,21 2,21" />}
      {props.shape === 'diamond' && <polygon points="12,1.5 22.5,12 12,22.5 1.5,12" />}
      {props.shape === 'circle' && <circle cx="12" cy="12" r="10" />}
      {props.shape === 'square' && <rect x="3" y="3" width="18" height="18" rx="1.5" />}
    </svg>
  );
}

/** Minus and plus on the slider's fine step buttons. */
export function StepIcon(props: { kind: 'minus' | 'plus' }) {
  return (
    <svg class="step-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path
        d={props.kind === 'plus' ? 'M12 4v16M4 12h16' : 'M4 12h16'}
        fill="none"
        stroke="currentColor"
        stroke-width="3"
        stroke-linecap="round"
      />
    </svg>
  );
}
