import { describe, expect, it } from 'vitest';
import { raceTrack } from './raceTrack';

describe('race track cells (raceResult)', () => {
  it('draws one cell per step from the start (0) to the finish, the own step highlighted', () => {
    expect(raceTrack(3, 5)).toEqual(['passed', 'passed', 'passed', 'here', 'ahead', 'ahead']);
  });

  it('a player still at the start is on the first cell', () => {
    expect(raceTrack(0, 3)).toEqual(['here', 'ahead', 'ahead', 'ahead']);
  });

  it('a player on the finish is on the last cell', () => {
    expect(raceTrack(10, 10)).toHaveLength(11);
    expect(raceTrack(10, 10).at(-1)).toBe('here');
  });

  it('a position outside the track is drawn at its nearest end; no finish means no track', () => {
    expect(raceTrack(12, 2)).toEqual(['passed', 'passed', 'here']);
    expect(raceTrack(-1, 2)).toEqual(['here', 'ahead', 'ahead']);
    expect(raceTrack(3, 0)).toEqual([]);
  });
});
