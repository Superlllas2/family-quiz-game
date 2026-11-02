// The Ready toggle (06 `lobby` and `countdown`, D76). The TV app decides who is ready; the phone shows a tap at
// once (feedback within 0.1 s) and follows the TV's flag again from its next screen.

import type { LobbyAction } from './protocol';

/** What the button shows: the last tap while it is newer than the TV's screen, else the TV's own flag. */
export function shownReady(tvReady: boolean, tapped: boolean | null): boolean {
  return tapped ?? tvReady;
}

/** A tap flips what is shown, so a fast second tap undoes the first instead of sending the same action twice. */
export function toggleReady(shown: boolean): { ready: boolean; action: LobbyAction } {
  return shown ? { ready: false, action: 'notReady' } : { ready: true, action: 'ready' };
}
