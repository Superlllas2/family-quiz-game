// The sent/ack helper (06, general rules): show "Sent" with a spinner, then a check mark when the `ack` arrives.
// No ack within 1 s: resend once with the same inputId. Still nothing after 3 s: "No connection".
// Used for every message answered with an `ack` (input, lobby).

import { NO_CONNECTION_MS, RESEND_MS } from '../constants';
import type { AckMessage, AckedMessage } from '../protocol';
import type { TimerHandle, Timers } from './timers';

export type InputState = 'sent' | 'acked' | 'refused' | 'noConnection';

export interface TrackedInput {
  inputId: string;
  state: InputState;
  /** Milliseconds from the first send to the ack; null until the ack arrives. */
  roundTripMs: number | null;
  /** The ack reason (for `refused`, or `duplicate` on an ok ack). */
  reason: string | null;
  resent: boolean;
}

export interface InputSenderDeps {
  /** Sends the message on the socket; false if the socket is not open. */
  transmit(msg: AckedMessage): boolean;
  timers: Timers;
  onChange(input: TrackedInput): void;
  log?: (...args: unknown[]) => void;
}

interface Entry {
  msg: AckedMessage;
  sentAt: number;
  view: TrackedInput;
  resendTimer: TimerHandle | null;
  giveUpTimer: TimerHandle | null;
}

export class InputSender {
  private readonly entries = new Map<string, Entry>();

  constructor(private readonly deps: InputSenderDeps) {}

  send(msg: AckedMessage): TrackedInput {
    const { timers } = this.deps;
    const entry: Entry = {
      msg,
      sentAt: timers.now(),
      view: { inputId: msg.inputId, state: 'sent', roundTripMs: null, reason: null, resent: false },
      resendTimer: null,
      giveUpTimer: null,
    };
    this.entries.set(msg.inputId, entry);
    this.deps.transmit(msg);

    entry.resendTimer = timers.setTimeout(() => {
      entry.resendTimer = null;
      if (entry.view.state !== 'sent') return;
      this.deps.log?.('no ack after 1 s, resending', msg.inputId);
      this.update(entry, { resent: true });
      this.deps.transmit(msg);
    }, RESEND_MS);

    entry.giveUpTimer = timers.setTimeout(() => {
      entry.giveUpTimer = null;
      if (entry.view.state !== 'sent') return;
      this.update(entry, { state: 'noConnection' });
    }, NO_CONNECTION_MS);

    return entry.view;
  }

  /** Handles an `ack`. Returns true if it belonged to an input sent from this page. */
  handleAck(ack: AckMessage): boolean {
    const entry = this.entries.get(ack.inputId);
    if (!entry) return false;
    if (entry.view.state === 'acked' || entry.view.state === 'refused') return true; // duplicate ack
    this.clearTimers(entry);
    const roundTripMs = Math.round(this.deps.timers.now() - entry.sentAt);
    this.deps.log?.(`ack ${ack.inputId} ${ack.ok ? 'ok' : 'not ok'} round trip ${roundTripMs} ms`);
    this.update(entry, {
      state: ack.ok ? 'acked' : 'refused',
      roundTripMs,
      reason: ack.reason ?? null,
    });
    return true;
  }

  /** Stops every timer (e.g. when the page goes back to join). */
  clear(): void {
    for (const entry of this.entries.values()) this.clearTimers(entry);
    this.entries.clear();
  }

  private clearTimers(entry: Entry): void {
    if (entry.resendTimer !== null) this.deps.timers.clearTimeout(entry.resendTimer);
    if (entry.giveUpTimer !== null) this.deps.timers.clearTimeout(entry.giveUpTimer);
    entry.resendTimer = entry.giveUpTimer = null;
  }

  private update(entry: Entry, change: Partial<TrackedInput>): void {
    entry.view = { ...entry.view, ...change };
    this.deps.onChange(entry.view);
  }
}
