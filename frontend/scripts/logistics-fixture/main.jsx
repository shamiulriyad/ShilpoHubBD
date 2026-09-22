// Synthetic offline test. No backend requests or real account changes.
import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route, useLocation } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../../src/contexts/ThemeContext';
import DashboardLayout from '../../src/layouts/DashboardLayout';
import RootLayout from '../../src/layouts/RootLayout';
import LogisticsWorkspaceGuard from '../../src/components/logistics/LogisticsWorkspaceGuard';
import Profile from '../../src/pages/LogisticsPartner/Profile';
import ExploreHome from '../../src/pages/Explore/ExploreHome';
import { logisticsPartnerSidebarNav } from '../../src/data/navigation';
import { routePaths } from '../../src/routes/routePaths';
import apiClient from '../../src/services/apiClient';
import { useAuthStore } from '../../src/stores/useAuthStore';
import '../../src/styles/index.css';

useAuthStore.setState({ accessToken: 'fixture', user: { id: 'fixture-user', fullName: 'Test partner', email: 'partner@example.test' }, roles: ['LogisticsPartner'], activeRole: 'LogisticsPartner', sessionReady: true });
apiClient.defaults.adapter = async config => {
  if (config.url === '/logistics/partners/me') throw Object.assign(new Error('No profile'), { response: { status: 404, data: { message: 'Logistics partner profile not found.' } }, config });
  if (['/districts', '/categories'].includes(config.url)) return { data: [], status: 200, statusText: 'OK', headers: {}, config };
  throw new Error('Unexpected request: ' + config.url);
};
const client = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: Infinity } } });
function CurrentRoute() { const location = useLocation(); return <p role="status" className="bg-surface p-2 text-sm">Test route: {location.pathname}</p>; }
const operations = logisticsPartnerSidebarNav[0].items.filter(item => item.path !== routePaths.logisticsPartnerProfile);
createRoot(document.getElementById('root')).render(
  <ThemeProvider><QueryClientProvider client={client}><MemoryRouter initialEntries={[routePaths.logisticsPartnerProfile]}>
    <CurrentRoute />
    <Routes>
      <Route element={<DashboardLayout />}><Route element={<LogisticsWorkspaceGuard />}>
        <Route path={routePaths.logisticsPartnerProfile} element={<Profile />} />
        {operations.map(item => <Route key={item.path} path={item.path} element={<p>Operational page ready</p>} />)}
      </Route></Route>
      <Route element={<RootLayout />}><Route path={routePaths.explore} element={<ExploreHome />} /></Route>
    </Routes>
  </MemoryRouter></QueryClientProvider></ThemeProvider>
);
