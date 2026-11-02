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
