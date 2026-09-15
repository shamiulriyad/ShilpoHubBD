import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { heritageFestivalsService } from '../services/heritageFestivalsService';

export function useHeritageFestivals(params = {}) {
  return useQuery({ queryKey: ['heritage-festivals', params], queryFn: () => heritageFestivalsService.list(params) });
}

export function useHeritageFestival(id) {
  return useQuery({
    queryKey: ['heritage-festivals', id],
    queryFn: () => heritageFestivalsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useHeritageFestivalMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['heritage-festivals'] });

  return {
    create: useMutation({ mutationFn: (payload) => heritageFestivalsService.create(payload), onSuccess: invalidate }),
    update: useMutation({ mutationFn: ({ id, payload }) => heritageFestivalsService.update(id, payload), onSuccess: invalidate }),
    remove: useMutation({ mutationFn: (id) => heritageFestivalsService.remove(id), onSuccess: invalidate }),
  };
}
