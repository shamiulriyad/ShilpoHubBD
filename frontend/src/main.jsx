import React from 'react';
import ReactDOM from 'react-dom/client';
import { QueryClientProvider } from '@tanstack/react-query';
import App from './App';
import AuthBootstrap from './components/auth/AuthBootstrap';
import GlobalFeedback from './components/ui/GlobalFeedback';
import ConfirmHost from './components/ui/ConfirmHost';
import { ThemeProvider } from './contexts/ThemeContext';
import { queryClient } from './lib/queryClient';
import './styles/index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <GlobalFeedback />
        <ConfirmHost />
        <AuthBootstrap>
          <App />
        </AuthBootstrap>
      </ThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>,
);
