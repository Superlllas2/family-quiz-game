import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { choiceInputMessage, lobbyMessage, type AckedMessage } from '../protocol';
import { InputSender, type TrackedInput } from './inputs';
import { globalTimers } from './timers';

describe('input sender (06: sent, ack, resend once, no connection)', () => {
  let sent: AckedMessage[];
  let changes: TrackedInput[];
  let sender: InputSender;

  beforeEach(() => {
    vi.useFakeTimers();
    sent = [];
    changes = [];
    sender = new InputSender({
      transmit: (m) => {
        sent.push(m);
        return true;
      },
      timers: globalTimers(() => Date.now()),
      onChange: (i) => changes.push(i),
    });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  const last = () => changes[changes.length - 1];

  it('sends at once and shows sent', () => {
    const view = sender.send(choiceInputMessage('a1', 3, 2, 500));
    expect(view.state).toBe('sent');
    expect(sent).toHaveLength(1);
  });

  it('resends once after 1 s with the same inputId', () => {
    sender.send(choiceInputMessage('a1', 3, 2, 500));
    vi.advanceTimersByTime(999);
    expect(sent).toHaveLength(1);
    vi.advanceTimersByTime(1);
    expect(sent).toHaveLength(2);
    expect(sent[1]).toStrictEqual(sent[0]);
    vi.advanceTimersByTime(1500);
    expect(sent).toHaveLength(2); // only once
  });

  it('shows no connection at 3 s', () => {
    sender.send(choiceInputMessage('a1', 3, 2, 500));
    vi.advanceTimersByTime(2999);
    expect(last()?.state).not.toBe('noConnection');
    vi.advanceTimersByTime(1);
    expect(last().state).toBe('noConnection');
  });

  it('an ack resolves it with the round trip and stops the resend', () => {
    sender.send(choiceInputMessage('a1', 3, 2, 500));
    vi.advanceTimersByTime(120);
    expect(sender.handleAck({ type: 'ack', inputId: 'a1', ok: true })).toBe(true);
    expect(last()).toMatchObject({ state: 'acked', roundTripMs: 120 });
    vi.advanceTimersByTime(5000);
    expect(sent).toHaveLength(1);
    expect(last().state).toBe('acked');
  });

  it('an ack after the resend still resolves it; a duplicate ack changes nothing', () => {
    sender.send(lobbyMessage('s1', 'ready'));
    vi.advanceTimersByTime(1200);
    sender.handleAck({ type: 'ack', inputId: 's1', ok: true });
    const count = changes.length;
    sender.handleAck({ type: 'ack', inputId: 's1', ok: true, reason: 'duplicate' });
    expect(changes).toHaveLength(count);
    expect(last()).toMatchObject({ state: 'acked', resent: true, roundTripMs: 1200 });
  });

  it('an ack with ok false shows refused with the reason', () => {
    sender.send(lobbyMessage('s1', 'ready'));
    sender.handleAck({ type: 'ack', inputId: 's1', ok: false, reason: 'invalid' });
    expect(last()).toMatchObject({ state: 'refused', reason: 'invalid' });
  });

  it('a late ack after no connection still shows the check mark', () => {
    sender.send(choiceInputMessage('a1', 3, 0, 10));
    vi.advanceTimersByTime(4000);
    sender.handleAck({ type: 'ack', inputId: 'a1', ok: true });
    expect(last().state).toBe('acked');
  });

  it('ignores acks for unknown inputs', () => {
    expect(sender.handleAck({ type: 'ack', inputId: 'zz', ok: true })).toBe(false);
  });
});
