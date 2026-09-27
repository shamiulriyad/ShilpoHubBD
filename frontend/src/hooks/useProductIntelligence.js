import { useMutation, useQuery } from '@tanstack/react-query';
import { productIntelligenceService } from '../services/productIntelligenceService';

export function useProductIntelligence(productId, range) {
  return useQuery({
    queryKey: ['product-intelligence', productId, range],
    queryFn: () => productIntelligenceService.get(productId, range),
    enabled: Boolean(productId),
  });
}

export function useProductIntelligenceAiInsights() {
  return useMutation({
    mutationFn: ({ productId, range }) => productIntelligenceService.getAiInsights(productId, range),
  });
}
