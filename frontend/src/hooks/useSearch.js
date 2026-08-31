import { useQuery } from '@tanstack/react-query';
import { searchService } from '../services/searchService';

export function useSearch(query, params = {}) {
  const q = (query || '').trim();
  return useQuery({
    queryKey: ['search', q, params],
    queryFn: () => searchService.search(q, params),
    enabled: q.length >= 2,
    placeholderData: (previousData) => previousData,
  });
}
