import { useQuery } from '@tanstack/react-query';
import { govProducerReportsService } from '../services/govProducerReportsService';

export function useGovProducerReports(params = {}) {
  return useQuery({
    queryKey: ['gov-producer-reports', 'list', params],
    queryFn: () => govProducerReportsService.list(params),
  });
}

export function useGovProducerReport(id) {
  return useQuery({
    queryKey: ['gov-producer-reports', 'detail', id],
    queryFn: () => govProducerReportsService.getById(id),
    enabled: !!id,
  });
}
