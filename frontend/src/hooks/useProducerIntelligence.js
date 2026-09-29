import { useQuery } from '@tanstack/react-query';
import { producerMonthlyReportsService } from '../services/producerMonthlyReportsService';

export function useProducerIntelligenceList(params) {
  return useQuery({
    queryKey: ['producer-intelligence', 'list', params],
    queryFn: () => producerMonthlyReportsService.intelligenceList(params),
  });
}

export function useProducerIntelligenceDashboard(params) {
  return useQuery({
    queryKey: ['producer-intelligence', 'dashboard', params],
    queryFn: () => producerMonthlyReportsService.intelligenceDashboard(params),
  });
}

export function useProducerMonthlyHistory(producerId, params = {}) {
  return useQuery({
    queryKey: ['producer-monthly-reports', 'history', producerId, params],
    queryFn: () => producerMonthlyReportsService.history(producerId, params),
    enabled: !!producerId,
  });
}

export function useProducerMonthlyComparison(producerId, params = {}) {
  return useQuery({
    queryKey: ['producer-monthly-reports', 'compare', producerId, params],
    queryFn: () => producerMonthlyReportsService.compare(producerId, params),
    enabled: !!producerId,
  });
}
