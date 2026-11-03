// What the `roundEnd` screen announces next (06): "Раунд 3 / Round 3 next", or the final race after the last round
// (the TV sends `nextRound` null then).

import type { TextKey } from './i18n';

export function roundEndNext(nextRound: number | null): { key: TextKey; params?: Record<string, number> } {
  return nextRound === null ? { key: 'roundEnd.finalNext' } : { key: 'roundEnd.next', params: { round: nextRound } };
}
