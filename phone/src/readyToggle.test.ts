import { describe, expect, it } from 'vitest';
import { shownReady, toggleReady } from './readyToggle';

describe('the Ready toggle (06 lobby and countdown, D76)', () => {
  it('a tap flips what the phone shows and sends the matching action', () => {
    expect(toggleReady(false)).toEqual({ ready: true, action: 'ready' });
    expect(toggleReady(true)).toEqual({ ready: false, action: 'notReady' });
  });

  it("shows the tap at once, until the TV's next screen", () => {
    expect(shownReady(false, null)).toBe(false);
    expect(shownReady(true, null)).toBe(true);
    expect(shownReady(false, true)).toBe(true);
    expect(shownReady(true, false)).toBe(false);
  });

  it('a fast second tap toggles back from what is shown, not from the TV flag', () => {
    const first = toggleReady(shownReady(false, null));
    const second = toggleReady(shownReady(false, first.ready));
    expect(first.action).toBe('ready');
    expect(second).toEqual({ ready: false, action: 'notReady' });
  });
});
