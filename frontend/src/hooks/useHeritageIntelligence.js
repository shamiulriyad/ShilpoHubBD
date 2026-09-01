import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { heritageIntelligenceService } from '../services/heritageIntelligenceService';

export function useHeritageIndexRecords(params = {}) {
  return useQuery({ queryKey: ['heritage-index', 'records', params], queryFn: () => heritageIntelligenceService.listRecords(params) });
}

export function useComputeHeritageIndex() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => heritageIntelligenceService.compute(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['heritage-index'] }),
  });
}
