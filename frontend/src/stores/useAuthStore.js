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
      storageHydrated: false,
      sessionReady: false,

      setSession: (authResponse) => {
        const roles = authResponse?.roles ?? [];
        set({
          accessToken: authResponse?.accessToken ?? null,
          refreshToken: authResponse?.refreshToken ?? null,
          user: authResponse
            ? {
                id: authResponse.userId,
                email: authResponse.email,
                fullName: authResponse.fullName,
              }
            : null,
          roles,
          activeRole: resolveActiveRole(roles, authResponse?.activeRole ?? null),
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

      markStorageHydrated: () =>
        set((state) => ({
          storageHydrated: true,
          activeRole: resolveActiveRole(state.roles ?? [], state.activeRole),
        })),

      finishSessionBootstrap: () => set({ sessionReady: true }),

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
      onRehydrateStorage: () => (state) => {
        state?.markStorageHydrated?.();
      },
    },
  ),
);
