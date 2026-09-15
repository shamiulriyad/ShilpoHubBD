<<<<<<< HEAD
import { useQuery } from '@tanstack/react-query';
import { districtsService } from '../services/districtsService';

export function useDistricts() {
  return useQuery({
    queryKey: ['districts'],
    queryFn: () => districtsService.list(),
  });
}
=======
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { districtsService } from '../services/districtsService';

export function useDistricts(params = {}) {
  return useQuery({
    queryKey: ['districts', params],
    queryFn: () => districtsService.list(params),
  });
}

export function useDistrictMutations() {
  const queryClient = useQueryClient();
  return {
    update: useMutation({
      mutationFn: ({ id, payload }) => districtsService.update(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['districts'] }),
    }),
  };
}
>>>>>>> Riyad
