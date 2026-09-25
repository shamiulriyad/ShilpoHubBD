import { useBlocker } from 'react-router-dom';

// Public entry points a signed-in user reaches by pressing Back out of a workspace. Going Back
// within the workspace is left alone; leaving to these pages asks first, because it ends the session.
const PUBLIC_EXITS = new Set(['/', '/login', '/register', '/forgot-password']);

export function useBackLogoutGuard(enabled = true) {
  return useBlocker(({ currentLocation, nextLocation, historyAction }) =>
    enabled
    && historyAction === 'POP'
    && currentLocation.pathname !== nextLocation.pathname
    && PUBLIC_EXITS.has(nextLocation.pathname),
  );
}
