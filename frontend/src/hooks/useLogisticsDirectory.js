import { useQuery } from '@tanstack/react-query';
import apiClient from '../services/apiClient';

// Verified logistics partners that are taking work -- what a producer picks from when shipping.
export function useLogisticsDirectory(enabled = true) {
  return useQuery({
    queryKey: ['logistics', 'directory'],
    queryFn: () => apiClient.get('/logistics/directory').then((res) => res.data),
    enabled,
    staleTime: 60_000,
  });
}

// Routes a chosen partner has planned (from -> to, date) so a producer can pick the one they need.
export function useLogisticsPartnerRoutes(profileId) {
  return useQuery({
    queryKey: ['logistics', 'directory', profileId, 'routes'],
    queryFn: () => apiClient.get(`/logistics/directory/${profileId}/routes`).then((res) => res.data),
    enabled: Boolean(profileId),
    staleTime: 30_000,
  });
}
