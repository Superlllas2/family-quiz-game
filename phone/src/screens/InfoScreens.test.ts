// The screens without a decision use no hooks, so they are called here as plain functions and the texts in their
// element tree are read (the tests run in Node, without a DOM).

import type { ComponentChildren, VNode } from 'preact';
import { describe, expect, it } from 'vitest';
import { characterColor } from '../characters';
import type { ScreenState } from '../protocol';
import { ResultsScreen, WaitingScreen, WatchScreen } from './InfoScreens';

/** Every text in an element tree, in order, function components drawn. */
function texts(node: ComponentChildren): string[] {
  if (node === null || node === undefined || typeof node === 'boolean') return [];
  if (typeof node === 'string' || typeof node === 'number' || typeof node === 'bigint') return [String(node)];
  if (Array.isArray(node)) return node.flatMap((n) => texts(n as ComponentChildren));
  const v = node as VNode<{ children?: ComponentChildren }>;
  if (typeof v.type === 'function') return texts((v.type as (p: unknown) => ComponentChildren)(v.props));
  return texts(v.props.children);
}

/** The first element in the tree with this class, or null. */
function withClass(node: ComponentChildren, name: string): VNode<Record<string, unknown>> | null {
  if (node === null || node === undefined || typeof node !== 'object') return null;
  if (Array.isArray(node)) {
    for (const n of node) {
      const hit = withClass(n as ComponentChildren, name);
      if (hit) return hit;
    }
    return null;
  }
  const v = node as VNode<Record<string, unknown>>;
  if (typeof v.props.class === 'string' && v.props.class.split(' ').includes(name)) return v;
  return withClass(v.props.children as ComponentChildren, name);
}

const screen = (id: string, data: Record<string, unknown>): ScreenState => ({ phaseId: 7, id, endsInMs: 5000, data });

describe('watch', () => {
  it('shows the player\'s character chip and "Eyes on the TV"', () => {
    const tree = WatchScreen({ lang: 'en', player: { name: 'Mama', characterId: 4, timerMultiplier: 1 } });
    expect(texts(tree)).toEqual(['Mama', 'Eyes on the TV']);
    const chip = withClass(tree, 'chip');
    expect(chip?.props.style).toEqual({ background: characterColor(4) });
  });

  it('in Russian', () => {
    const tree = WatchScreen({ lang: 'ru', player: { name: 'Мама', characterId: 1, timerMultiplier: 1 } });
    expect(texts(tree)).toEqual(['Мама', 'Смотри на экран']);
  });

  it('without a chip while the player is not known yet', () => {
    const tree = WatchScreen({ lang: 'en', player: null });
    expect(texts(tree)).toEqual(['Eyes on the TV']);
    expect(withClass(tree, 'chip')).toBeNull();
  });
});

describe('results, the winner only (D87)', () => {
  it('shows "You won!", the prize line and the fact', () => {
    const tree = ResultsScreen({ lang: 'en', screen: screen('results', { fact: 'Octopuses have three hearts.' }) });
    expect(texts(tree)).toEqual(['You won!', 'Your prize: a fun fact', 'Octopuses have three hearts.']);
  });

  it('in Russian', () => {
    const tree = ResultsScreen({ lang: 'ru', screen: screen('results', { fact: 'У осьминога три сердца.' }) });
    expect(texts(tree)).toEqual(['Победа!', 'Твоя награда — интересный факт', 'У осьминога три сердца.']);
  });

  it('shows no place, awards or countdown, and no empty fact line', () => {
    const tree = ResultsScreen({
      lang: 'en', screen: screen('results', { place: 1, correctCount: 9, awards: ['Fastest'] }),
    });
    expect(texts(tree)).toEqual(['You won!', 'Your prize: a fun fact']);
  });
});

describe('waiting', () => {
  it('shows what was locked, no list of who still answers (D88)', () => {
    const tree = WaitingScreen({
      lang: 'en', screen: screen('waiting', { locked: 'B', pending: [{ name: 'Papa', characterId: 3 }] }),
    });
    expect(texts(tree)).toEqual(['Locked: B']);
  });
});
