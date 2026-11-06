// The rating card and the paused screen use no hooks, so they are called here as plain functions and their element
// trees are read (the tests run in Node, without a DOM).

import type { ComponentChildren, VNode } from 'preact';
import { describe, expect, it } from 'vitest';
import type { RateData } from '../protocol';
import { PausedScreen } from './InfoScreens';
import { RatingCard } from './RatingCard';

type Node = VNode<Record<string, unknown>>;

/** Draws function components until only elements are left. */
function draw(node: ComponentChildren): ComponentChildren {
  if (node === null || node === undefined || typeof node !== 'object') return node;
  if (Array.isArray(node)) return node.map((n) => draw(n as ComponentChildren));
  const v = node as Node;
  if (typeof v.type === 'function') {
    // Keep the component's own props (class, onPress) on the element it draws, for the queries below.
    const drawn = draw((v.type as (p: unknown) => ComponentChildren)(v.props)) as Node;
    return drawn && typeof drawn === 'object' ? { ...drawn, props: { ...drawn.props, component: v.props } } : drawn;
  }
  return { ...v, props: { ...v.props, children: draw(v.props.children as ComponentChildren) } } as Node;
}

function texts(node: ComponentChildren): string[] {
  if (node === null || node === undefined || typeof node === 'boolean') return [];
  if (typeof node === 'string' || typeof node === 'number') return [String(node)];
  if (Array.isArray(node)) return node.flatMap((n) => texts(n as ComponentChildren));
  return texts((node as Node).props.children as ComponentChildren);
}

/** Every element in the tree with this class, in order. */
function allWithClass(node: ComponentChildren, name: string): Node[] {
  if (node === null || node === undefined || typeof node !== 'object') return [];
  if (Array.isArray(node)) return node.flatMap((n) => allWithClass(n as ComponentChildren, name));
  const v = node as Node;
  const hit = typeof v.props.class === 'string' && v.props.class.split(' ').includes(name) ? [v] : [];
  return [...hit, ...allWithClass(v.props.children as ComponentChildren, name)];
}

const rate: RateData = {
  questionId: 'films_old_001',
  title: 'Rate the question',
  close: 'Close',
  stars: 5,
  items: [
    { id: 'fun', text: 'Did you like the question?', low: 'Boring', high: 'Loved it', given: 4 },
    { id: 'guess', text: 'Could you work it out without knowing?', low: 'No way', high: 'Easily', given: 0 },
  ],
};

function card(shown: Record<string, number>, taps: [string, number][] = [], closed: number[] = []) {
  return draw(RatingCard({
    lang: 'en',
    rate,
    shown,
    pop: null,
    onStar: (item, stars) => taps.push([item, stars]),
    onClose: () => closed.push(1),
  }));
}

describe('rating card (D109)', () => {
  it('shows the title, then per item its text and the low and high labels', () => {
    expect(texts(card({ fun: 4, guess: 0 }))).toEqual([
      'Rate the question',
      'Did you like the question?', 'Boring', 'Loved it',
      'Could you work it out without knowing?', 'No way', 'Easily',
    ]);
  });

  it('draws `stars` stars per row, filled from the left up to the shown rating', () => {
    const rows = allWithClass(card({ fun: 4, guess: 0 }), 'rate-stars');
    expect(rows).toHaveLength(2);
    const on = rows.map((r) => allWithClass(r, 'star').map((s) => s.props.class === 'btn star on'));
    expect(on).toEqual([[true, true, true, true, false], [false, false, false, false, false]]);
  });

  it('every star says which it is, for screen readers', () => {
    const stars = allWithClass(card({ fun: 4, guess: 0 }), 'star');
    expect(stars.slice(0, 5).map((s) => s.props['aria-label'])).toEqual(['1 of 5', '2 of 5', '3 of 5', '4 of 5', '5 of 5']);
  });

  it('a tap on star k of a row sends that row and k: the leftmost is 1, the rightmost is 5', () => {
    const taps: [string, number][] = [];
    const rows = allWithClass(card({ fun: 0, guess: 0 }, taps), 'rate-stars');
    const press = (row: number, star: number): void =>
      (allWithClass(rows[row], 'star')[star].props.component as { onPress(): void }).onPress();
    press(0, 0);
    press(0, 4);
    press(1, 2);
    expect(taps).toEqual([['fun', 1], ['fun', 5], ['guess', 3]]);
  });

  it('the close button is labelled with the TV\'s text and closes', () => {
    const closed: number[] = [];
    const [button] = allWithClass(card({}, [], closed), 'rate-close');
    expect(button.props['aria-label']).toBe('Close');
    (button.props.component as { onPress(): void }).onPress();
    expect(closed).toEqual([1]);
  });

  it('falls back to the phone\'s own close label', () => {
    const tree = draw(RatingCard({
      lang: 'ru', rate: { ...rate, close: '', title: '' }, shown: {}, pop: null, onStar: () => {}, onClose: () => {},
    }));
    expect(allWithClass(tree, 'rate-close')[0].props['aria-label']).toBe('Закрыть');
    expect(allWithClass(tree, 'rate-title')).toHaveLength(0);
  });

  it('only the tapped star pops', () => {
    const tree = draw(RatingCard({
      lang: 'en', rate, shown: { fun: 3 }, pop: { item: 'fun', stars: 3, seq: 1 }, onStar: () => {}, onClose: () => {},
    }));
    expect(allWithClass(tree, 'pop')).toHaveLength(1);
  });
});

describe('paused (D105)', () => {
  const paused = (data: Record<string, unknown>) => ({ phaseId: 40, id: 'paused', endsInMs: null, data });

  it('shows the TV\'s text and nothing to tap', () => {
    const tree = draw(PausedScreen({ lang: 'en', screen: paused({ text: 'Игра на паузе' }) }));
    expect(texts(tree)).toEqual(['Игра на паузе']);
    expect(allWithClass(tree, 'btn')).toHaveLength(0);
  });

  it('falls back to the phone\'s own text', () => {
    expect(texts(draw(PausedScreen({ lang: 'en', screen: paused({}) })))).toEqual(['The game is paused']);
    expect(texts(draw(PausedScreen({ lang: 'ru', screen: paused({}) })))).toEqual(['Игра на паузе']);
  });
});
