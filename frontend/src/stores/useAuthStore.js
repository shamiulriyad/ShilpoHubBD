import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { resolveActiveRole } from '../utils/roles';

export const useAuthStore = create(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      roles: [],
      activeRole: null,
      // Flipped to true once persisted state has been read back from storage,
      // so route guards can wait instead of bouncing to /login on a refresh.
      hasHydrated: false,

      setSession: (authResponse) => {
        const roles = authResponse.roles ?? [];
        set({
          accessToken: authResponse.accessToken,
          refreshToken: authResponse.refreshToken,
          user: {
            id: authResponse.userId,
            email: authResponse.email,
            fullName: authResponse.fullName,
          },
          roles,
          // Persist the *effective* role so every consumer reads one value.
          activeRole: resolveActiveRole(roles, authResponse.activeRole ?? null),
        });
      },

      clearSession: () => {
        set({
          accessToken: null,
          refreshToken: null,
          user: null,
          roles: [],
          activeRole: null,
        });
      },

      // Called once after persisted state is read back: mark hydration complete
      // and normalise a legacy session persisted with activeRole: null.
      finishHydration: () =>
        set((state) => ({
          hasHydrated: true,
          activeRole: resolveActiveRole(state.roles ?? [], state.activeRole),
        })),

      hasRole: (role) => get().roles.includes(role),
      hasAnyRole: (allowed = []) => {
        const roles = get().roles;
        return allowed.length === 0 || allowed.some((role) => roles.includes(role));
      },
    }),
    {
      name: 'shilpohub-auth',
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        user: state.user,
        roles: state.roles,
        activeRole: state.activeRole,
      }),
      // Runs synchronously for localStorage — guards can rely on hasHydrated
      // being true by first render.
      onRehydrateStorage: () => (state) => {
        state?.finishHydration?.();
      },
    },
  ),
);
