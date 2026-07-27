import { create } from 'zustand';

export const useAppStore = create((set) => ({
  isSidebarOpen: false,
  setSidebarOpen: (isSidebarOpen) => set({ isSidebarOpen }),
}));
