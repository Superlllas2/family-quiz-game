// The power up pick and the poll views use no hooks, so they are called here as plain functions and their element
// trees are read (the tests run in Node, without a DOM). The pick and poll rules themselves are tested in
// powerUpPick.test.ts and pollAnswers.test.ts.

import type { ComponentChildren, VNode } from 'preact';
import { describe, expect, it, vi } from 'vitest';
import type { TrackedInput } from '../net/inputs';
import { addAttempt, armPick, canTarget, pickFor, pickView, type PickState } from '../powerUpPick';
import type { PollQuestion, PowerUpTarget } from '../protocol';
import { PollScreen } from './PollScreen';
import { ResultScreen } from './ResultScreen';

type Node = VNode<Record<string, unknown>>;

/** Every text in an element tree, in order, function components drawn. */
function texts(node: ComponentChildren): string[] {
  if (node === null || node === undefined || typeof node === 'boolean') return [];
  if (typeof node === 'string' || typeof node === 'number' || typeof node === 'bigint') return [String(node)];
  if (Array.isArray(node)) return node.flatMap((n) => texts(n as ComponentChildren));
  const v = node as VNode<{ children?: ComponentChildren }>;
  if (typeof v.type === 'function') return texts((v.type as (p: unknown) => ComponentChildren)(v.props));
  return texts(v.props.children);
}

/** Every element (or component element) in the tree with this class, without drawing function components. */
function allWithClass(node: ComponentChildren, name: string, out: Node[] = []): Node[] {
  if (node === null || node === undefined || typeof node !== 'object') return out;
  if (Array.isArray(node)) {
    for (const n of node) allWithClass(n as ComponentChildren, name, out);
    return out;
  }
  const v = node as Node;
  if (typeof v.props.class === 'string' && v.props.class.split(' ').includes(name)) out.push(v);
  allWithClass(v.props.children as ComponentChildren, name, out);
  return out;
}

const targets: PowerUpTarget[] = [
  { playerId: 'p2', name: 'Papa', available: true },
  { playerId: 'p3', name: 'Bo', available: false },
  { playerId: 'p4', name: 'Liza', available: true },
];

const tracked = (inputId: string, state: TrackedInput['state'], reason: string | null = null): TrackedInput =>
  ({ inputId, state, roundTripMs: 40, reason, resent: false });

function result(state: PickState, inputs: Record<string, TrackedInput> = {}, lang: 'ru' | 'en' = 'en') {
  return ResultScreen({
    lang,
    powerUp: 'freeze',
    targets,
    armed: state.armed,
    view: pickView(state, inputs),
    canTarget: (t) => canTarget(state, inputs, t),
    onArm: () => {},
    onTarget: () => {},
  });
}

describe('result: the power up pick (06)', () => {
  it('first shows only the big card, named in the player\'s language', () => {
    expect(texts(result(pickFor(null, 30)))).toEqual(['Freeze', 'Tap to use it', 'Or keep it for later']);
    expect(texts(result(pickFor(null, 30), {}, 'ru'))).toEqual(['Заморозка', 'Нажми, чтобы использовать', 'Или оставь на потом']);
    expect(allWithClass(result(pickFor(null, 30)), 'target')).toHaveLength(0);
  });

  it('the card is colored by power up', () => {
    const tree = ResultScreen({
      lang: 'en', powerUp: 'slime', targets, armed: false, view: pickView(pickFor(null, 1), {}),
      canTarget: () => false, onArm: () => {}, onTarget: () => {},
    });
    expect(allWithClass(tree, 'power-card')[0].props.class).toContain('power-slime');
    expect(texts(tree)[0]).toBe('Slime');
  });

  it('tapping the card (on release, D143) arms it', () => {
    const onArm = vi.fn();
    const tree = ResultScreen({
      lang: 'en', powerUp: 'fog', targets, armed: false, view: pickView(pickFor(null, 1), {}),
      canTarget: () => false, onArm, onTarget: () => {},
    });
    (allWithClass(tree, 'power-card')[0].props.onPress as () => void)();
    expect(onArm).toHaveBeenCalledOnce();
  });

  it('armed: the target row, unavailable targets greyed', () => {
    const tree = result(armPick(pickFor(null, 30)));
    expect(texts(tree)).toEqual(['Freeze', 'Who gets it?', 'Papa', 'Bo', 'Liza']);
    const buttons = allWithClass(tree, 'target');
    expect(buttons.map((b) => b.props.disabled)).toEqual([false, true, false]);
  });

  it('after a send the picked target stays lit, the others dim and locked, and the status shows', () => {
    const s = addAttempt(armPick(pickFor(null, 30)), 'u1', 'p4');
    const tree = result(s, { u1: tracked('u1', 'sent') });
    const buttons = allWithClass(tree, 'target');
    expect(buttons.map((b) => b.props.class)).toEqual(['target dim', 'target dim', 'target chosen']);
    expect(buttons.map((b) => b.props.disabled)).toEqual([true, true, false]);
    expect(texts(tree)).toContain('Sent');
  });

  it('immune: a short note, and the other targets can be picked again', () => {
    const s = addAttempt(armPick(pickFor(null, 30)), 'u1', 'p2');
    const tree = result(s, { u1: tracked('u1', 'refused', 'immune') });
    expect(texts(tree)).toContain('They cannot be hit right now. Pick someone else');
    expect(allWithClass(tree, 'target').map((b) => b.props.disabled)).toEqual([true, true, false]);
  });

  it('noPowerUp: a short note, and nothing can be tapped any more', () => {
    const s = addAttempt(armPick(pickFor(null, 30)), 'u1', 'p2');
    const tree = result(s, { u1: tracked('u1', 'refused', 'noPowerUp') });
    expect(texts(tree)).toContain('You have no power up any more');
    expect(allWithClass(tree, 'target').every((b) => b.props.disabled === true)).toBe(true);
    expect(allWithClass(tree, 'power-card')[0].props.disabled).toBe(true);
  });
});

const questions: PollQuestion[] = [
  { id: 'fun', text: 'The most fun moment?', kind: 'text', options: [] },
  { id: 'time', text: 'Was there enough time to answer?', kind: 'choice', options: ['Too little', 'Just right', 'Too much'] },
];

function pollTree(page: number, draft: Record<string, number | string> = {}, sent = false, handlers = {}) {
  return PollScreen({
    lang: 'en', questions, page, draft, sent, pollInput: sent ? tracked('q1', 'acked') : null,
    onPage: () => {}, onChoose: () => {}, onText: () => {}, onSubmit: () => {}, ...handlers,
  });
}

describe('poll: one question per page (06, 20_playtest.md)', () => {
  it('a text page: the indicator, the question, a text box of at most 200 characters, Back and Next', () => {
    const tree = pollTree(0, { fun: 'The slime' });
    expect(texts(tree)).toEqual(['1/2', 'The most fun moment?', 'You can skip this one', '9/200', 'Back', 'Next']);
    const box = allWithClass(tree, 'poll-textarea')[0];
    expect(box.props.maxLength).toBe(200);
    expect(box.props.value).toBe('The slime');
    expect(allWithClass(tree, 'poll-back')[0].props.disabled).toBe(true);
  });

  it('the last page: options, the selected one marked, and Submit instead of Next', () => {
    const tree = pollTree(1, { time: 1 });
    expect(texts(tree)).toEqual(['2/2', 'Was there enough time to answer?', 'Too little', 'Just right', 'Too much', 'Back', 'Submit']);
    expect(allWithClass(tree, 'poll-option').map((b) => b.props.class)).toEqual([
      'poll-option', 'poll-option selected', 'poll-option',
    ]);
    expect(allWithClass(tree, 'poll-next')).toHaveLength(0);
  });

  it('a tap on an option chooses it by index', () => {
    const onChoose = vi.fn();
    const tree = pollTree(1, {}, false, { onChoose });
    (allWithClass(tree, 'poll-option')[2].props.onPress as () => void)();
    expect(onChoose).toHaveBeenCalledWith('time', 2);
  });

  it('Back and Next move one page', () => {
    const onPage = vi.fn();
    (allWithClass(pollTree(0, {}, false, { onPage }), 'poll-next')[0].props.onPress as () => void)();
    (allWithClass(pollTree(1, {}, false, { onPage }), 'poll-back')[0].props.onPress as () => void)();
    expect(onPage.mock.calls).toEqual([[1], [0]]);
  });

  it('once sent: Submit and the answers are locked, and the ack status shows', () => {
    const tree = pollTree(1, { time: 0 }, true);
    expect(allWithClass(tree, 'poll-submit')[0].props.disabled).toBe(true);
    expect(allWithClass(tree, 'poll-option').every((b) => b.props.disabled === true)).toBe(true);
    expect(texts(tree)).toContain('40 ms');
  });

  it('a page past the end shows the last question', () => {
    expect(texts(pollTree(9))[0]).toBe('2/2');
  });
});
