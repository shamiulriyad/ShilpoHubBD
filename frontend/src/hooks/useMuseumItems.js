import { useQuery } from '@tanstack/react-query';
import { museumItemsService } from '../services/museumItemsService';

export function useMuseumItems(params = {}) {
  return useQuery({ queryKey: ['museum-items', params], queryFn: () => museumItemsService.list(params) });
}

export function useMuseumItem(id) {
  return useQuery({
    queryKey: ['museum-items', id],
    queryFn: () => museumItemsService.getById(id),
    enabled: Boolean(id),
  });
}
