// Message types and builders for the phone side of docs/PROTOCOL.md (protocol version 1).
// The phone sends and receives the inner messages only; there is no envelope between relay and phone.

import { PROTOCOL_VERSION } from './constants';

export type Lang = 'ru' | 'en';

export type RefusedReason = 'roomFull' | 'nameTaken' | 'badName' | 'gameStarted' | 'badToken' | 'badVersion';

export type LobbyAction = 'start' | 'cancelStart' | 'playAgain';

export type PhaseId = number;

// ---- Phone to TV app (section 3) ----

export interface JoinMessage { v: 1; type: 'join'; name: string; lang: Lang }
export interface RejoinMessage { v: 1; type: 'rejoin'; token: string }
export interface ChoiceInputMessage {
  v: 1; type: 'input'; inputId: string; phaseId: PhaseId; kind: 'choice'; choice: number; ms: number;
}
export interface LobbyMessage { v: 1; type: 'lobby'; inputId: string; action: LobbyAction }
export interface PingMessage { v: 1; type: 'ping'; t: number }

export type PhoneMessage = JoinMessage | RejoinMessage | ChoiceInputMessage | LobbyMessage | PingMessage;

/** A message that is answered with an `ack` and goes through the input sender. */
export type AckedMessage = ChoiceInputMessage | LobbyMessage;

const v = PROTOCOL_VERSION as 1;

export function joinMessage(name: string, lang: Lang): JoinMessage {
  return { v, type: 'join', name, lang };
}

export function rejoinMessage(token: string): RejoinMessage {
  return { v, type: 'rejoin', token };
}

export function choiceInputMessage(inputId: string, phaseId: PhaseId, choice: number, ms: number): ChoiceInputMessage {
  return { v, type: 'input', inputId, phaseId, kind: 'choice', choice, ms };
}

export function lobbyMessage(inputId: string, action: LobbyAction): LobbyMessage {
  return { v, type: 'lobby', inputId, action };
}

export function pingMessage(t: number): PingMessage {
  return { v, type: 'ping', t };
}

// ---- TV app (and relay) to phone (sections 2 and 4) ----

export interface LobbyPlayer { playerId: string; name: string; characterId: number }

export interface ScreenState {
  phaseId: PhaseId;
  id: string;
  endsInMs: number | null;
  data: Record<string, unknown>;
}

export interface PlayerInfo {
  playerId: string;
  name: string;
  characterId: number;
  [field: string]: unknown;
}

export interface WelcomeMessage { type: 'welcome'; playerId: string; token: string; room: string }
export interface RefusedMessage { type: 'refused'; reason: string }
export interface SnapshotMessage { type: 'snapshot'; player: PlayerInfo | null; screen: ScreenState | null }
export interface ScreenMessage extends ScreenState { type: 'screen' }
export interface AckMessage { type: 'ack'; inputId: string; ok: boolean; reason?: string }
export interface PlayerMessage { type: 'player'; player: PlayerInfo }
export interface PongMessage { type: 'pong'; t: number; tv?: number }
export interface TvAwayMessage { type: 'tvAway' }
export interface TvBackMessage { type: 'tvBack' }

export type IncomingMessage =
  | WelcomeMessage | RefusedMessage | SnapshotMessage | ScreenMessage | AckMessage
  | PlayerMessage | PongMessage | TvAwayMessage | TvBackMessage;

type Obj = Record<string, unknown>;

function isObj(x: unknown): x is Obj {
  return typeof x === 'object' && x !== null && !Array.isArray(x);
}

function parseScreen(x: unknown): ScreenState | null {
  if (!isObj(x) || typeof x.id !== 'string' || typeof x.phaseId !== 'number') return null;
  return {
    phaseId: x.phaseId,
    id: x.id,
    endsInMs: typeof x.endsInMs === 'number' ? x.endsInMs : null,
    data: isObj(x.data) ? x.data : {},
  };
}

function parsePlayer(x: unknown): PlayerInfo | null {
  if (!isObj(x) || typeof x.playerId !== 'string') return null;
  return {
    ...x,
    playerId: x.playerId,
    name: typeof x.name === 'string' ? x.name : '',
    characterId: typeof x.characterId === 'number' ? x.characterId : -1,
  };
}

/**
 * Parses a text frame. Returns null for anything that is not a known, well formed message:
 * unknown types are ignored by the caller, never a crash.
 */
export function parseIncoming(text: unknown): IncomingMessage | null {
  if (typeof text !== 'string') return null;
  let m: unknown;
  try {
    m = JSON.parse(text);
  } catch {
    return null;
  }
  if (!isObj(m) || typeof m.type !== 'string') return null;
  switch (m.type) {
    case 'welcome':
      if (typeof m.token !== 'string' || m.token === '') return null;
      return {
        type: 'welcome',
        token: m.token,
        playerId: typeof m.playerId === 'string' ? m.playerId : '',
        room: typeof m.room === 'string' ? m.room : '',
      };
    case 'refused':
      return { type: 'refused', reason: typeof m.reason === 'string' ? m.reason : '' };
    case 'snapshot':
      return { type: 'snapshot', player: parsePlayer(m.player), screen: parseScreen(m.screen) };
    case 'screen': {
      const s = parseScreen(m);
      return s ? { type: 'screen', ...s } : null;
    }
    case 'ack':
      if (typeof m.inputId !== 'string') return null;
      return {
        type: 'ack',
        inputId: m.inputId,
        ok: m.ok === true,
        ...(typeof m.reason === 'string' ? { reason: m.reason } : {}),
      };
    case 'player': {
      const p = parsePlayer(m.player);
      return p ? { type: 'player', player: p } : null;
    }
    case 'pong':
      if (typeof m.t !== 'number') return null;
      return { type: 'pong', t: m.t, ...(typeof m.tv === 'number' ? { tv: m.tv } : {}) };
    case 'tvAway':
      return { type: 'tvAway' };
    case 'tvBack':
      return { type: 'tvBack' };
    default:
      return null;
  }
}

/** Reads the lobby screen data defensively. */
export function lobbyData(data: Record<string, unknown>): { players: LobbyPlayer[]; canStart: boolean } {
  const raw = Array.isArray(data.players) ? data.players : [];
  const players: LobbyPlayer[] = [];
  for (const p of raw) {
    if (isObj(p) && typeof p.playerId === 'string') {
      players.push({
        playerId: p.playerId,
        name: typeof p.name === 'string' ? p.name : '',
        characterId: typeof p.characterId === 'number' ? p.characterId : -1,
      });
    }
  }
  return { players, canStart: data.canStart === true };
}
