import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { complaintsMonitoringService } from '../services/complaintsMonitoringService';

export function useComplaints(params = {}) {
  return useQuery({ queryKey: ['complaints', 'list', params], queryFn: () => complaintsMonitoringService.listComplaints(params) });
}

export function useMonitoringFlags(params = {}) {
  return useQuery({ queryKey: ['monitoring-flags', 'list', params], queryFn: () => complaintsMonitoringService.listMonitoringFlags(params) });
}

export function useQrMonitoringOverview(params = {}) {
  return useQuery({ queryKey: ['qr-monitoring-overview', params], queryFn: () => complaintsMonitoringService.getQrOverview(params) });
}

export function useComplaintsMonitoringMutations() {
  const queryClient = useQueryClient();
  return {
    createComplaint: useMutation({
      mutationFn: (payload) => complaintsMonitoringService.createComplaint(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['complaints'] }),
    }),
    assignComplaint: useMutation({
      mutationFn: ({ id, payload }) => complaintsMonitoringService.assignComplaint(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['complaints'] }),
    }),
    resolveComplaint: useMutation({
      mutationFn: ({ id, payload }) => complaintsMonitoringService.resolveComplaint(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['complaints'] }),
    }),
    runMonitoringScan: useMutation({
      mutationFn: (payload) => complaintsMonitoringService.runMonitoringScan(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['monitoring-flags'] }),
    }),
    updateMonitoringFlagStatus: useMutation({
      mutationFn: ({ id, payload }) => complaintsMonitoringService.updateMonitoringFlagStatus(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['monitoring-flags'] }),
    }),
  };
}
