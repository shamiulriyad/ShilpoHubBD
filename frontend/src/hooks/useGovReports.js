import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { govReportsService } from '../services/govReportsService';

export function useGovReports(params = {}) {
  return useQuery({ queryKey: ['gov-reports', 'list', params], queryFn: () => govReportsService.listReports(params) });
}

export function useGovForecasts(params = {}) {
  return useQuery({ queryKey: ['gov-forecasts', 'list', params], queryFn: () => govReportsService.listForecasts(params) });
}

export function useGovReportMutations() {
  const queryClient = useQueryClient();
  return {
    generateReport: useMutation({
      mutationFn: (payload) => govReportsService.generateReport(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['gov-reports'] }),
    }),
    updateReport: useMutation({
      mutationFn: ({ id, payload }) => govReportsService.updateReport(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['gov-reports'] }),
    }),
    removeReport: useMutation({
      mutationFn: (id) => govReportsService.removeReport(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['gov-reports'] }),
    }),
    generateForecast: useMutation({
      mutationFn: (payload) => govReportsService.generateForecast(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['gov-forecasts'] }),
    }),
    removeForecast: useMutation({
      mutationFn: (id) => govReportsService.removeForecast(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['gov-forecasts'] }),
    }),
  };
}
