import { describe, expect, it } from 'vitest';
import { POLL_TEXT_MAX } from './constants';
import { chooseOption, clampPage, pollAnswers, writeText } from './pollAnswers';
import type { PollQuestion } from './protocol';

const questions: PollQuestion[] = [
  { id: 'fun', text: 'The most fun moment?', kind: 'text', options: [] },
  { id: 'frustrating', text: 'What was frustrating?', kind: 'text', options: [] },
  { id: 'waiting', text: 'When were you waiting?', kind: 'choice', options: ['Never', 'Between questions', 'During results', 'In the final'] },
  { id: 'again', text: 'Play again?', kind: 'choice', options: ['Yes', 'Maybe', 'No'] },
];

describe('poll answers (PROTOCOL.md section 3, `poll` answers)', () => {
  it('nothing answered: an empty object', () => {
    expect(pollAnswers(questions, {})).toStrictEqual({});
  });

  it('choice is the option index, text the string', () => {
    let d = chooseOption({}, 'waiting', 1);
    d = writeText(d, 'fun', 'The freeze on Dad');
    d = chooseOption(d, 'again', 0);
    expect(pollAnswers(questions, d)).toStrictEqual({ fun: 'The freeze on Dad', waiting: 1, again: 0 });
  });

  it('a choice can be changed', () => {
    let d = chooseOption({}, 'again', 0);
    d = chooseOption(d, 'again', 2);
    expect(pollAnswers(questions, d)).toStrictEqual({ again: 2 });
  });

  it('text is trimmed; empty or blank text is left out', () => {
    let d = writeText({}, 'fun', '  The slime  \n');
    d = writeText(d, 'frustrating', '   \n ');
    expect(pollAnswers(questions, d)).toStrictEqual({ fun: 'The slime' });
    expect(pollAnswers(questions, writeText({}, 'fun', ''))).toStrictEqual({});
  });

  it('text is at most 200 characters, while typing and when sent', () => {
    expect(POLL_TEXT_MAX).toBe(200);
    const long = 'я'.repeat(250);
    const d = writeText({}, 'fun', long);
    expect(d.fun).toBe('я'.repeat(200));
    expect(pollAnswers(questions, { fun: `  ${'a'.repeat(205)}` })).toStrictEqual({ fun: 'a'.repeat(200) });
  });

  it('leaves out answers that do not fit their question', () => {
    const d = { waiting: 7, again: -1, fun: 3, frustrating: 'ok', unknown: 'x', other: 1 } as Record<string, number | string>;
    expect(pollAnswers(questions, d)).toStrictEqual({ frustrating: 'ok' });
    expect(pollAnswers(questions, { waiting: 1.5 })).toStrictEqual({});
    expect(pollAnswers(questions, { waiting: 'Never' })).toStrictEqual({});
  });

  it('keeps the question order', () => {
    const d = chooseOption(writeText({}, 'fun', 'x'), 'waiting', 0);
    expect(Object.keys(pollAnswers(questions, d))).toStrictEqual(['fun', 'waiting']);
  });
});

describe('poll pages: one question per page', () => {
  it('clamps the page into the question list', () => {
    expect(clampPage(0, 7)).toBe(0);
    expect(clampPage(6, 7)).toBe(6);
    expect(clampPage(7, 7)).toBe(6);
    expect(clampPage(-1, 7)).toBe(0);
    expect(clampPage(3, 0)).toBe(0);
  });
});
