import React from 'react';
import { createRoot } from 'react-dom/client';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '../../src/contexts/ThemeContext';
import RootLayout from '../../src/layouts/RootLayout';
import TourRoutes from '../../src/pages/Tourism/TourRoutes';
import CulturalEvents from '../../src/pages/Tourism/CulturalEvents';
import { useAuthStore } from '../../src/stores/useAuthStore';
import apiClient from '../../src/services/apiClient';
import '../../src/styles/index.css';
// Synthetic data only; this preview never sends requests to the backend.
useAuthStore.setState({accessToken:'fixture',user:{id:'test',fullName:'Travel preview'},roles:['Tourist'],activeRole:'Tourist',sessionReady:true});
apiClient.defaults.adapter = async config => ({data:{items:[],totalCount:0},status:200,statusText:'OK',headers:{},config});
createRoot(document.getElementById('root')).render(<ThemeProvider><QueryClientProvider client={new QueryClient()}><MemoryRouter initialEntries={['/tourism/routes']}><Routes><Route element={<RootLayout />}><Route path="/tourism/routes" element={<TourRoutes />} /><Route path="/tourism/events" element={<CulturalEvents />} /></Route></Routes></MemoryRouter></QueryClientProvider></ThemeProvider>);
