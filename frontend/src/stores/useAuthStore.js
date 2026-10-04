import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import { resolveActiveRole } from '../utils/roles';

const LEGACY_AUTH_STORAGE_KEY = 'shilpohub-auth';
const TAB_AUTH_STORAGE_KEY = 'shilpohub-auth-tab';

const tabSessionStorage = () => {
  // Older builds kept credentials in localStorage, which made every browser tab share
  // one login. Remove that stale browser-wide copy and keep credentials in this tab.
  try { window.localStorage.removeItem(LEGACY_AUTH_STORAGE_KEY); } catch { /* Storage may be disabled. */ }
  return window.sessionStorage;
};

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
      name: TAB_AUTH_STORAGE_KEY,
      storage: createJSONStorage(tabSessionStorage),
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
