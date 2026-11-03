// Which text the sent/ack status shows (06, general rules): "Sent" with a spinner, a check mark on the ack,
// "No connection" after 3 s, and for a refused ack a text by reason (PROTOCOL.md section 4, ack reasons).

import type { TextKey } from './i18n';
import type { TrackedInput } from './net/inputs';

export function inputStatusKey(input: TrackedInput): TextKey {
  switch (input.state) {
    case 'sent':
      return 'input.sent';
    case 'acked':
      return 'input.acked';
    case 'noConnection':
      return 'input.noConnection';
    case 'refused':
      if (input.reason === 'closed') return 'input.closed';
      if (input.reason === 'characterTaken') return 'pick.taken';
      if (input.reason === 'immune') return 'powerUp.immune';
      if (input.reason === 'noPowerUp') return 'powerUp.noPowerUp';
      return 'input.refused';
  }
}
