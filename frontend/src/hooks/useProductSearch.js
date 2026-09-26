import { useQuery } from '@tanstack/react-query';
import { productSearchService } from '../services/productSearchService';

export function useProductSearch(q, page = 1, pageSize = 12) {
  const text = (q || '').trim();
  return useQuery({
    queryKey: ['product-search', text, page, pageSize],
    queryFn: ({ signal }) => productSearchService.search({ q: text, page, pageSize }, signal),
    enabled: text.length >= 2,
    placeholderData: (previous) => previous,
    staleTime: 60_000,
    retry: false,
  });
}
