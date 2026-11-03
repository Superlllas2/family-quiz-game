// The power up pick on `result` (03 "Using", 06 `result`): two taps, the power up card and then a target. Reveal and
// Scores share one phaseId, and a refreshed `result` with the same phaseId (someone else hit a target) keeps the armed
// card. One send per phaseId, except that an `immune` refusal lets the player pick someone else; `noPowerUp`, any other
// refusal and no connection stop the pick. After an ok ack the TV app sends the next screen itself.

import type { TextKey } from './i18n';
import type { TrackedInput } from './net/inputs';
import type { PhaseId, PowerUpKind, PowerUpTarget } from './protocol';

export interface PickAttempt { inputId: string; target: string }

export interface PickState {
  phaseId: PhaseId;
  /** The card was tapped: the target row shows. */
  armed: boolean;
  /** Every `powerUp` input sent on this phaseId, in order (more than one only after `immune`). */
  attempts: PickAttempt[];
}

export type PickStage = 'open' | 'sending' | 'done' | 'stopped';

export interface PickView {
  stage: PickStage;
  /** The last input sent, for its status line; null before the first send (or before the sender reported it). */
  last: TrackedInput | null;
  lastTarget: string | null;
  /** Targets the TV refused as immune on this phaseId: greyed here too. */
  immune: string[];
}

/** The pick state for the `result` screen with this phaseId: kept for the same phaseId, else a fresh one. */
export function pickFor(state: PickState | null, phaseId: PhaseId): PickState {
  return state && state.phaseId === phaseId ? state : { phaseId, armed: false, attempts: [] };
}

export function armPick(state: PickState): PickState {
  return state.armed ? state : { ...state, armed: true };
}

export function addAttempt(state: PickState, inputId: string, target: string): PickState {
  return { ...state, attempts: [...state.attempts, { inputId, target }] };
}

function refusedImmune(input: TrackedInput | undefined): boolean {
  return input?.state === 'refused' && input.reason === 'immune';
}

export function pickView(state: PickState, inputs: Readonly<Record<string, TrackedInput>>): PickView {
  const immune = state.attempts.filter((a) => refusedImmune(inputs[a.inputId])).map((a) => a.target);
  const lastAttempt = state.attempts[state.attempts.length - 1];
  if (!lastAttempt) return { stage: 'open', last: null, lastTarget: null, immune };
  const last = inputs[lastAttempt.inputId] ?? null;
  let stage: PickStage;
  if (!last || last.state === 'sent') stage = 'sending';
  else if (last.state === 'acked') stage = 'done';
  else if (refusedImmune(last)) stage = 'open';
  else stage = 'stopped';
  return { stage, last, lastTarget: lastAttempt.target, immune };
}

/** True if tapping this target now sends the power up. */
export function canTarget(
  state: PickState, inputs: Readonly<Record<string, TrackedInput>>, target: PowerUpTarget,
): boolean {
  if (!state.armed || !target.available) return false;
  const view = pickView(state, inputs);
  return view.stage === 'open' && !view.immune.includes(target.playerId);
}

/** The power up's name in the player's language: Заморозка / Freeze and the others. */
export function powerUpNameKey(powerUp: PowerUpKind): TextKey {
  switch (powerUp) {
    case 'freeze':
      return 'powerUp.freeze';
    case 'slime':
      return 'powerUp.slime';
    case 'fog':
      return 'powerUp.fog';
  }
}
