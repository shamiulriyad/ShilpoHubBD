import { useQuery } from '@tanstack/react-query';
import { traceabilityService } from '../services/traceabilityService';

export function useProductTraceability(productId) {
  return useQuery({
    queryKey: ['traceability', productId],
    queryFn: () => traceabilityService.getByProduct(productId),
    enabled: Boolean(productId),
    retry: false,
  });
}
