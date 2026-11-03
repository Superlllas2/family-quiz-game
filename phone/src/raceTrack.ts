// The small race track on the `raceResult` screen (06): a row of step cells from the start (step 0) to the finish,
// the player's own step highlighted. The TV app moves the players; the phone only draws the step it was sent.

export type TrackCell = 'passed' | 'here' | 'ahead';

/** More cells than this would not fit a phone row; the TV's finish is 10 (`race.finishSteps`). */
const MAX_CELLS = 40;

/** Cells for steps 0 to finish; a position outside the track is drawn at its nearest end. No finish: no track. */
export function raceTrack(position: number, finish: number): TrackCell[] {
  const last = Math.min(MAX_CELLS - 1, Math.floor(finish));
  if (!(last > 0)) return [];
  const here = Math.min(last, Math.max(0, Math.round(position)));
  const cells: TrackCell[] = [];
  for (let step = 0; step <= last; step++) cells.push(step < here ? 'passed' : step === here ? 'here' : 'ahead');
  return cells;
}
