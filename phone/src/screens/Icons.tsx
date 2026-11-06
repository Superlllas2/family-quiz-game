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

/** A cardboard TV set drawn in the board game look (watch screen): the hint to look up at the real one. Colors in style.css. */
export function TvPiece() {
  return (
    <svg class="tv-piece" viewBox="0 0 120 100" aria-hidden="true">
      <path class="tv-line" d="M46 8 L60 22 L76 4" stroke-width="5" />
      <rect class="tv-edge" x="8" y="27" width="104" height="64" rx="12" />
      <rect class="tv-body" x="8" y="22" width="104" height="64" rx="12" stroke-width="5" />
      <rect class="tv-screen" x="18" y="32" width="66" height="44" rx="7" stroke-width="4" />
      <path class="tv-glint" d="M26 42 Q34 38 40 44" stroke-width="4" />
      <circle class="tv-knob" cx="98" cy="42" r="5" stroke-width="3.5" />
      <circle class="tv-knob tv-knob-2" cx="98" cy="60" r="5" stroke-width="3.5" />
    </svg>
  );
}

/** One star of the rating card; filled or empty comes from style.css. */
export function StarIcon() {
  return (
    <svg class="star-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path
        d="M12 2.6l2.8 5.9 6.4.8-4.7 4.5 1.2 6.4L12 17.1l-5.7 3.1 1.2-6.4-4.7-4.5 6.4-.8z"
        stroke-width="2.2"
        stroke-linejoin="round"
      />
    </svg>
  );
}

/** The cross on the rating card's close button. */
export function CloseIcon() {
  return (
    <svg class="close-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d="M6 6l12 12M18 6L6 18" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" />
    </svg>
  );
}

/** Two bars on a round token: the game is paused. */
export function PauseToken() {
  return (
    <svg class="pause-token" viewBox="0 0 100 100" aria-hidden="true">
      <circle class="pause-edge" cx="50" cy="54" r="42" />
      <circle class="pause-face" cx="50" cy="48" r="42" stroke-width="5" />
      <rect class="pause-bar" x="34" y="30" width="11" height="36" rx="3" />
      <rect class="pause-bar" x="55" y="30" width="11" height="36" rx="3" />
    </svg>
  );
}
