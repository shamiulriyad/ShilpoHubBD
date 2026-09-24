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
