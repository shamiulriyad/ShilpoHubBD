import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import App from './App';
import { ThemeProvider } from './contexts/ThemeContext';
import { useAuthStore } from './stores/useAuthStore';
import './styles/index.css';

const queryClient = new QueryClient();

// Safety net: zustand's persist rehydrates synchronously for localStorage and
// flips `hasHydrated` via onRehydrateStorage, but if storage is unavailable or
// the callback never fires we still release the route guards here.
if (!useAuthStore.getState().hasHydrated) {
  useAuthStore.getState().finishHydration();
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <App />
      </ThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>,
);
