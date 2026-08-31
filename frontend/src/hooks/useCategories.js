import { useQuery } from '@tanstack/react-query';
import { categoriesService } from '../services/categoriesService';

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesService.list(),
  });
}

export function useCategory(id) {
  return useQuery({
    queryKey: ['categories', id],
    queryFn: () => categoriesService.getById(id),
    enabled: Boolean(id),
  });
}
