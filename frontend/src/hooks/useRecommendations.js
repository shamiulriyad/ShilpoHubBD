import { useQuery } from '@tanstack/react-query';
import { recommendationsService } from '../services/recommendationsService';

export function useRecommendedForMe(count = 8) {
  return useQuery({
    queryKey: ['recommendations', 'for-me', count],
    queryFn: () => recommendationsService.forMe(count),
  });
}

export function useSimilarProducts(productId, count = 8) {
  return useQuery({
    queryKey: ['recommendations', 'similar', productId, count],
    queryFn: () => recommendationsService.similarTo(productId, count),
    enabled: Boolean(productId),
  });
}
