import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import App from './App';
import AuthBootstrap from './components/auth/AuthBootstrap';
import GlobalFeedback from './components/ui/GlobalFeedback';
import { ThemeProvider } from './contexts/ThemeContext';
import { queryClient } from './lib/queryClient';
import { useAuthStore } from './stores/useAuthStore';
import './styles/index.css';

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
        <GlobalFeedback />
        <AuthBootstrap>
          <App />
        </AuthBootstrap>
      </ThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>,
);
