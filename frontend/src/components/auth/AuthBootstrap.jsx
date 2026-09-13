import { useEffect } from 'react';
import { authService } from '../../services/authService';
import { useAuthStore } from '../../stores/useAuthStore';
import { isAccessTokenUsable } from '../../utils/jwt';
import FullPageLoader from '../ui/FullPageLoader';

let bootstrapPromise = null;

function bootstrapSession() {
  if (bootstrapPromise) return bootstrapPromise;

  bootstrapPromise = (async () => {
    const state = useAuthStore.getState();

    if (isAccessTokenUsable(state.accessToken)) return;

    if (!state.refreshToken) {
      state.clearSession();
      return;
    }

    try {
      const data = await authService.refresh(state.refreshToken);
      useAuthStore.getState().setSession(data);
    } catch {
      useAuthStore.getState().clearSession();
    }
  })().finally(() => {
    useAuthStore.getState().finishSessionBootstrap();
  });

  return bootstrapPromise;
}

export default function AuthBootstrap({ children }) {
  const storageHydrated = useAuthStore((state) => state.storageHydrated);
  const sessionReady = useAuthStore((state) => state.sessionReady);

  useEffect(() => {
    if (!storageHydrated && useAuthStore.persist.hasHydrated()) {
      useAuthStore.getState().markStorageHydrated();
      return;
    }

    if (storageHydrated && !sessionReady) bootstrapSession();
  }, [sessionReady, storageHydrated]);

  if (!sessionReady) {
    return <FullPageLoader label="Restoring your session…" />;
  }

  return children;
}