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
export interface CharacterMessage { v: 1; type: 'character'; inputId: string; characterId: number }
/** No inputId and no ack (PROTOCOL.md section 3). */
export interface SettingsMessage { v: 1; type: 'settings'; lang: Lang; timerMultiplier: number }
export interface LeaveMessage { v: 1; type: 'leave'; inputId: string }
export interface PingMessage { v: 1; type: 'ping'; t: number }

export type PhoneMessage =
  | JoinMessage | RejoinMessage | ChoiceInputMessage | LobbyMessage | CharacterMessage
  | SettingsMessage | LeaveMessage | PingMessage;

/** A message that is answered with an `ack` and goes through the input sender. */
export type AckedMessage = ChoiceInputMessage | LobbyMessage | CharacterMessage | LeaveMessage;

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

export function characterMessage(inputId: string, characterId: number): CharacterMessage {
  return { v, type: 'character', inputId, characterId };
}

export function settingsMessage(lang: Lang, timerMultiplier: number): SettingsMessage {
  return { v, type: 'settings', lang, timerMultiplier };
}

export function leaveMessage(inputId: string): LeaveMessage {
  return { v, type: 'leave', inputId };
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

// ---- defensive readers: a missing or broken field gets a harmless default, never a crash ----

function str(x: unknown): string {
  return typeof x === 'string' ? x : '';
}

function num(x: unknown, fallback: number): number {
  return typeof x === 'number' && Number.isFinite(x) ? x : fallback;
}

function numOrNull(x: unknown): number | null {
  return typeof x === 'number' && Number.isFinite(x) ? x : null;
}

function list(x: unknown): unknown[] {
  return Array.isArray(x) ? x : [];
}

function lobbyPlayers(x: unknown): LobbyPlayer[] {
  const players: LobbyPlayer[] = [];
  for (const p of list(x)) {
    if (isObj(p) && typeof p.playerId === 'string') {
      players.push({ playerId: p.playerId, name: str(p.name), characterId: num(p.characterId, -1) });
    }
  }
  return players;
}

/** The `player` object (PROTOCOL.md section 4) as the top bar and settings need it. */
export interface PlayerView {
  name: string;
  characterId: number;
  score: number;
  rank: number | null;
  /** 1 when missing: the TV app's default. */
  timerMultiplier: number;
}

export function playerView(p: PlayerInfo): PlayerView {
  return {
    name: p.name,
    characterId: p.characterId,
    score: num(p.score, 0),
    rank: numOrNull(p.rank),
    timerMultiplier: num(p.timerMultiplier, 1),
  };
}

/** Screen `lobby`. */
export function lobbyData(data: Record<string, unknown>): { players: LobbyPlayer[]; canStart: boolean } {
  return { players: lobbyPlayers(data.players), canStart: data.canStart === true };
}

/** Screen `pickCharacter`: ids taken by other players, and this player's character (null if missing). */
export function pickCharacterData(data: Record<string, unknown>): { taken: number[]; current: number | null } {
  const taken = list(data.taken).filter((x): x is number => typeof x === 'number');
  return { taken, current: numOrNull(data.current) };
}

/** Screens `countdown`, `error`, and the text of `getReady`. */
export function textData(data: Record<string, unknown>): { text: string } {
  return { text: str(data.text) };
}

/** Screen `getReady`. `prediction` is read but not drawn yet (Day 2). */
export function getReadyData(data: Record<string, unknown>): { text: string; prediction: LobbyPlayer[] } {
  return { text: str(data.text), prediction: lobbyPlayers(data.prediction) };
}

/** Screen `answerChoice`: up to 4 options in display order; the index of each is the `choice` sent. */
export function answerChoiceData(
  data: Record<string, unknown>,
): { question: string; options: string[]; windowMs: number | null } {
  const options = list(data.options).slice(0, 4).map(str);
  return { question: str(data.question), options, windowMs: numOrNull(data.windowMs) };
}

export interface PendingPlayer { name: string; characterId: number }

/** Screen `waiting`. */
export function waitingData(data: Record<string, unknown>): { locked: string; pending: PendingPlayer[] } {
  const pending: PendingPlayer[] = [];
  for (const p of list(data.pending)) {
    if (isObj(p)) pending.push({ name: str(p.name), characterId: num(p.characterId, -1) });
  }
  return { locked: str(data.locked), pending };
}

export interface PowerUpTarget { playerId: string; name: string; available: boolean }

export interface ResultData {
  correct: boolean;
  points: number;
  correctText: string;
  streak: number;
  rank: number | null;
  powerUp: string | null;
  targets: PowerUpTarget[];
}

/** Screen `result`. `powerUp` and `targets` are read but not drawn yet (Day 2: always null and empty). */
export function resultData(data: Record<string, unknown>): ResultData {
  const targets: PowerUpTarget[] = [];
  for (const p of list(data.targets)) {
    if (isObj(p) && typeof p.playerId === 'string') {
      targets.push({ playerId: p.playerId, name: str(p.name), available: p.available === true });
    }
  }
  return {
    correct: data.correct === true,
    points: num(data.points, 0),
    correctText: str(data.correctText),
    streak: num(data.streak, 0),
    rank: numOrNull(data.rank),
    powerUp: typeof data.powerUp === 'string' ? data.powerUp : null,
    targets,
  };
}

/** Screen `results`. */
export function resultsData(
  data: Record<string, unknown>,
): { place: number | null; correctCount: number; awards: string[] } {
  const awards = list(data.awards).filter((x): x is string => typeof x === 'string' && x !== '');
  return { place: numOrNull(data.place), correctCount: num(data.correctCount, 0), awards };
}

/** Screen `pollDone`. */
export function pollDoneData(data: Record<string, unknown>): { canPlayAgain: boolean } {
  return { canPlayAgain: data.canPlayAgain === true };
}
