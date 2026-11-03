// The character pick (06 `pickCharacter`): a tap only selects a card and Confirm sends it, so a slip of the finger
// never seats a player with the wrong character.

/**
 * The card shown as selected before Confirm: the player's own tap while that card is still free, else the one the
 * TV assigned. Someone else can take the tapped card before Confirm; the refreshed screen then falls back.
 */
export function selectedCard(tapped: number | null, current: number | null, taken: readonly number[]): number | null {
  if (tapped !== null && (tapped === current || !taken.includes(tapped))) return tapped;
  return current;
}
