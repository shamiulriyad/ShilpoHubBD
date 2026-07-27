import { create } from 'zustand';

export const useAuthStore = create((set) => ({
  user: null,
  token: null,
  setSession: (session) => set(session),
  clearSession: () => set({ user: null, token: null }),
}));
