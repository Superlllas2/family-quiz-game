// Message types and builders for the phone side of docs/PROTOCOL.md (protocol version 2).
// The phone sends and receives the inner messages only; there is no envelope between relay and phone.

import { PROTOCOL_VERSION } from './constants';

export type Lang = 'ru' | 'en';

export type RefusedReason = 'roomFull' | 'nameTaken' | 'badName' | 'gameStarted' | 'badToken' | 'badVersion';

export type LobbyAction = 'ready' | 'notReady';

export type PhaseId = number;

// ---- Phone to TV app (section 3) ----

export interface JoinMessage { v: 2; type: 'join'; name: string; lang: Lang }
export interface RejoinMessage { v: 2; type: 'rejoin'; token: string }
export interface ChoiceInputMessage {
  v: 2; type: 'input'; inputId: string; phaseId: PhaseId; kind: 'choice'; choice: number; ms: number;
}
export interface VoteInputMessage {
  v: 2; type: 'input'; inputId: string; phaseId: PhaseId; kind: 'vote'; door: number;
}
export interface ValueInputMessage {
  v: 2; type: 'input'; inputId: string; phaseId: PhaseId; kind: 'value'; value: number; ms: number;
}
export interface TrueFalseInputMessage {
  v: 2; type: 'input'; inputId: string; phaseId: PhaseId; kind: 'trueFalse'; answer: boolean; ms: number;
}
/** Every `input` kind the phone sends (PROTOCOL.md section 3, `input` kinds). */
export type InputMessage = ChoiceInputMessage | VoteInputMessage | ValueInputMessage | TrueFalseInputMessage;
export interface LobbyMessage { v: 2; type: 'lobby'; inputId: string; action: LobbyAction }
export interface CharacterMessage { v: 2; type: 'character'; inputId: string; characterId: number }
/** No inputId and no ack (PROTOCOL.md section 3). */
export interface SettingsMessage { v: 2; type: 'settings'; lang: Lang; timerMultiplier: number }
export interface LeaveMessage { v: 2; type: 'leave'; inputId: string }
export interface PingMessage { v: 2; type: 'ping'; t: number }

export type PhoneMessage =
  | JoinMessage | RejoinMessage | InputMessage | LobbyMessage | CharacterMessage
  | SettingsMessage | LeaveMessage | PingMessage;

/** A message that is answered with an `ack` and goes through the input sender. */
export type AckedMessage = InputMessage | LobbyMessage | CharacterMessage | LeaveMessage;

const v = PROTOCOL_VERSION as 2;

export function joinMessage(name: string, lang: Lang): JoinMessage {
  return { v, type: 'join', name, lang };
}

export function rejoinMessage(token: string): RejoinMessage {
  return { v, type: 'rejoin', token };
}

export function choiceInputMessage(inputId: string, phaseId: PhaseId, choice: number, ms: number): ChoiceInputMessage {
  return { v, type: 'input', inputId, phaseId, kind: 'choice', choice, ms };
}

export function voteInputMessage(inputId: string, phaseId: PhaseId, door: number): VoteInputMessage {
  return { v, type: 'input', inputId, phaseId, kind: 'vote', door };
}

export function valueInputMessage(inputId: string, phaseId: PhaseId, value: number, ms: number): ValueInputMessage {
  return { v, type: 'input', inputId, phaseId, kind: 'value', value, ms };
}

export function trueFalseInputMessage(
  inputId: string, phaseId: PhaseId, answer: boolean, ms: number,
): TrueFalseInputMessage {
  return { v, type: 'input', inputId, phaseId, kind: 'trueFalse', answer, ms };
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

/** A player in the `lobby` screen, with the TV's ready mark (D76). */
export interface ReadyPlayer extends LobbyPlayer { ready: boolean }

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

/**
 * The `player` object (PROTOCOL.md section 4) as the top bar and settings need it. Score and rank are not read: they
 * are on the TV only (D88).
 */
export interface PlayerView {
  name: string;
  characterId: number;
  /** 1 when missing: the TV app's default. */
  timerMultiplier: number;
}

export function playerView(p: PlayerInfo): PlayerView {
  return {
    name: p.name,
    characterId: p.characterId,
    timerMultiplier: num(p.timerMultiplier, 1),
  };
}

export interface LobbyData {
  players: ReadyPlayer[];
  /** This player is ready. */
  ready: boolean;
  /** `players.min` or more players are connected. */
  enoughPlayers: boolean;
}

/** Screen `lobby`. A missing ready mark is not ready. */
export function lobbyData(data: Record<string, unknown>): LobbyData {
  const players: ReadyPlayer[] = [];
  for (const p of list(data.players)) {
    if (isObj(p) && typeof p.playerId === 'string') {
      players.push({
        playerId: p.playerId, name: str(p.name), characterId: num(p.characterId, -1), ready: p.ready === true,
      });
    }
  }
  return { players, ready: data.ready === true, enoughPlayers: data.enoughPlayers === true };
}

/** Screen `pickCharacter`: ids taken by other players, and this player's character (null if missing). */
export function pickCharacterData(data: Record<string, unknown>): { taken: number[]; current: number | null } {
  const taken = list(data.taken).filter((x): x is number => typeof x === 'number');
  return { taken, current: numOrNull(data.current) };
}

/** Screens `countdown` and `error`. */
export function textData(data: Record<string, unknown>): { text: string } {
  return { text: str(data.text) };
}

/**
 * Screen `answerChoice`: up to 4 options in display order; the index of each is the `choice` sent. The question is on
 * the TV only (D88).
 */
export function answerChoiceData(data: Record<string, unknown>): { options: string[]; windowMs: number | null } {
  const options = list(data.options).slice(0, 4).map(str);
  return { options, windowMs: numOrNull(data.windowMs) };
}

/** Screen `topicVote`: the door titles in order; the index of each is the `door` sent. */
export function topicVoteData(data: Record<string, unknown>): { doors: string[] } {
  const doors = list(data.doors).slice(0, 4).map((d) => (isObj(d) ? str(d.title) : ''));
  return { doors };
}

export interface AnswerSliderData {
  min: number;
  max: number;
  step: number;
  unit: string;
  windowMs: number | null;
}

/**
 * Screen `answerSlider` (closest value). A missing or broken number reads as 0 (step: 1); the slider guards the range.
 * The question is on the TV only (D88).
 */
export function answerSliderData(data: Record<string, unknown>): AnswerSliderData {
  return {
    min: num(data.min, 0),
    max: num(data.max, 0),
    step: num(data.step, 1),
    unit: str(data.unit),
    windowMs: numOrNull(data.windowMs),
  };
}

/** Screen `answerTrueFalse` (final race). The statement is on the TV only (D88). */
export function answerTrueFalseData(data: Record<string, unknown>): { windowMs: number | null } {
  return { windowMs: numOrNull(data.windowMs) };
}

/**
 * The player's answer window for the top bar's countdown bar, from any answer screen's `windowMs`; null for other
 * screens or when it is missing.
 */
export function answerWindowMs(screen: ScreenState): number | null {
  switch (screen.id) {
    case 'answerChoice':
    case 'answerSlider':
    case 'answerTrueFalse':
      return numOrNull(screen.data.windowMs);
    default:
      return null;
  }
}

/** Screen `waiting`: the text of what was locked. */
export function waitingData(data: Record<string, unknown>): { locked: string } {
  return { locked: str(data.locked) };
}

export interface PowerUpTarget { playerId: string; name: string; available: boolean }

export interface ResultData {
  powerUp: string | null;
  targets: PowerUpTarget[];
}

/**
 * Screen `result`: sent only to a player holding a power up (power ups come on Day 4). Read here but not drawn yet:
 * the phone shows `watch` for it (shownScreen.ts).
 */
export function resultData(data: Record<string, unknown>): ResultData {
  const targets: PowerUpTarget[] = [];
  for (const p of list(data.targets)) {
    if (isObj(p) && typeof p.playerId === 'string') {
      targets.push({ playerId: p.playerId, name: str(p.name), available: p.available === true });
    }
  }
  return { powerUp: typeof data.powerUp === 'string' ? data.powerUp : null, targets };
}

/** Screen `results`, sent only to the winner: the reward fact, already in the player's language (D87). */
export function resultsData(data: Record<string, unknown>): { fact: string } {
  return { fact: str(data.fact) };
}
