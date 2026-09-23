import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../../src/contexts/ThemeContext';
import RootLayout from '../../src/layouts/RootLayout';
import HeritageDatabase from '../../src/pages/Research/HeritageDatabase';
import Crafts from '../../src/pages/Explore/Crafts';
import InnovationHubHome from '../../src/pages/Research/InnovationHubHome';
import { useAuthStore } from '../../src/stores/useAuthStore';
import apiClient from '../../src/services/apiClient';
import '../../src/styles/index.css';

// Development-only fixture. No real credentials or backend requests.
const tourist = new URLSearchParams(location.search).get('role') === 'Tourist';
const role = tourist ? 'Tourist' : 'HeritageInnovationHub';
useAuthStore.setState({ accessToken: 'fixture', user: { id: 'heritage-preview', fullName: 'Heritage preview' }, roles: [role], activeRole: role, sessionReady: true });
apiClient.defaults.adapter = async config => ({ data: config.url.startsWith('/notifications') ? { items: [], totalCount: 0, unreadCount: 0 } : [], status: 200, statusText: 'OK', headers: {}, config });
createRoot(document.getElementById('root')).render(<ThemeProvider><QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter initialEntries={[tourist ? '/explore/crafts' : '/research/heritage-database']}><Routes><Route element={<RootLayout />}><Route path="/research/heritage-database" element={<HeritageDatabase />} /><Route path="/explore/crafts" element={<Crafts />} /><Route path="/research" element={<InnovationHubHome />} /><Route path="*" element={<p>Workspace destination</p>} /></Route></Routes></MemoryRouter></QueryClientProvider></ThemeProvider>);
