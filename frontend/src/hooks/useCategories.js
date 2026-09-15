import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
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

export function useCategoryMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['categories'] });

  return {
    create: useMutation({ mutationFn: (payload) => categoriesService.create(payload), onSuccess: invalidate }),
    update: useMutation({ mutationFn: ({ id, payload }) => categoriesService.update(id, payload), onSuccess: invalidate }),
    remove: useMutation({ mutationFn: (id) => categoriesService.remove(id), onSuccess: invalidate }),
  };
}
