import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { villagesService } from '../services/villagesService';

export function useVillages() {
  return useQuery({ queryKey: ['villages'], queryFn: () => villagesService.list() });
}

export function useFavoriteVillages(enabled = true) {
  return useQuery({ queryKey: ['villages', 'favorites'], queryFn: () => villagesService.favorites(), enabled });
}

export function useVillage(id) {
  return useQuery({ queryKey: ['villages', id], queryFn: () => villagesService.getById(id), enabled: Boolean(id) });
}

export function useVillageFavoriteMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['villages'] });
  };

  const favorite = useMutation({ mutationFn: (id) => villagesService.favorite(id), onSuccess: invalidate });
  const unfavorite = useMutation({ mutationFn: (id) => villagesService.unfavorite(id), onSuccess: invalidate });

  return { favorite, unfavorite };
}

export function useCreateVillage() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => villagesService.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['villages'] }),
  });
}
