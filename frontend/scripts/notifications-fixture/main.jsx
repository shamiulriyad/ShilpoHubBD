import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../../src/contexts/ThemeContext';
import DashboardLayout from '../../src/layouts/DashboardLayout';
import DashboardNotifications from '../../src/pages/Dashboard/DashboardNotifications';
import { useAuthStore } from '../../src/stores/useAuthStore';
import apiClient from '../../src/services/apiClient';
import '../../src/styles/index.css';

// Synthetic UI regression fixture: never accesses a real account or API.
const params = new URLSearchParams(location.search);
const role = params.get('role') || 'Producer';
useAuthStore.setState({ accessToken: 'fixture', user: { id: 'notification-preview', fullName: 'Workspace preview' }, roles: [role], activeRole: role, sessionReady: true });
let items = params.has('empty') ? [] : Array.from({ length: 23 }, (_, i) => ({ id: String(i), title: i % 2 ? 'New message' : 'Product review complete', body: i % 2 ? 'You have a new message. Open your inbox to reply.' : 'Your product is approved and ready for customers to discover.', category: i % 2 ? 'Messages' : 'Approvals', targetPath: i % 2 ? '/dashboard/messages' : '/producer/products', createdAt: new Date(Date.now() - i * 3600000).toISOString(), readAt: i > 2 ? new Date().toISOString() : null }));
apiClient.defaults.adapter = async config => {
  if (params.has('error')) throw new Error('Synthetic offline state');
  const body = config.data ? JSON.parse(config.data) : {};
  if (config.method === 'patch') items = items.map(item => item.id === config.url.split('/')[2] ? { ...item, readAt: body.isRead ? new Date().toISOString() : null } : item);
  if (config.method === 'post') items = items.map(item => new Date(item.createdAt) <= new Date(body.through) ? { ...item, readAt: new Date().toISOString() } : item);
  const page = Number(config.params?.page || 1);
  const filtered = config.params?.unreadOnly ? items.filter(item => !item.readAt) : items;
  return { data: { items: filtered.slice((page - 1) * 20, page * 20), totalCount: filtered.length, page, pageSize: 20, asOf: new Date().toISOString(), unreadCount: items.filter(item => !item.readAt).length }, status: 200, statusText: 'OK', headers: {}, config };
};
createRoot(document.getElementById('root')).render(<ThemeProvider><QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter initialEntries={['/dashboard/notifications']}><Routes><Route element={<DashboardLayout />}><Route path="/dashboard/notifications" element={<DashboardNotifications />} /><Route path="*" element={<p>Activity destination reached</p>} /></Route></Routes></MemoryRouter></QueryClientProvider></ThemeProvider>);

