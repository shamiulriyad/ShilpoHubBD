import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { nationalDashboardService } from '../services/nationalDashboardService';

export function useNationalOverview(params = {}) {
  return useQuery({ queryKey: ['national-dashboard', 'overview', params], queryFn: () => nationalDashboardService.getOverview(params) });
}

export function useDistrictRankings(params = {}) {
  return useQuery({ queryKey: ['national-dashboard', 'rankings', params], queryFn: () => nationalDashboardService.getDistrictRankings(params) });
}

export function useDashboardSnapshots(params = {}) {
  return useQuery({ queryKey: ['national-dashboard', 'snapshots', params], queryFn: () => nationalDashboardService.listSnapshots(params) });
}

export function useDashboardTrend(params) {
  return useQuery({
    queryKey: ['national-dashboard', 'trend', params],
    queryFn: () => nationalDashboardService.getTrend(params),
    enabled: Boolean(params?.metric),
  });
}

export function useNationalDashboardMutations() {
  const queryClient = useQueryClient();
  return {
    captureSnapshot: useMutation({
      mutationFn: (payload) => nationalDashboardService.captureSnapshot(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['national-dashboard', 'snapshots'] }),
    }),
    removeSnapshot: useMutation({
      mutationFn: (id) => nationalDashboardService.removeSnapshot(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['national-dashboard', 'snapshots'] }),
    }),
  };
}
