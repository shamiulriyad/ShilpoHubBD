import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export const useAuthStore = create(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      roles: [],
      activeRole: null,

      setSession: (authResponse) => {
        set({
          accessToken: authResponse.accessToken,
          refreshToken: authResponse.refreshToken,
          user: {
            id: authResponse.userId,
            email: authResponse.email,
            fullName: authResponse.fullName,
          },
          roles: authResponse.roles ?? [],
          activeRole: authResponse.activeRole ?? null,
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

      hasRole: (role) => get().roles.includes(role),
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
    },
  ),
);
