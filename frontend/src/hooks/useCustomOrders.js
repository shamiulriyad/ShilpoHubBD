import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customOrdersService } from '../services/customOrdersService';

export function useMyCustomOrders(enabled = true) {
  return useQuery({
    queryKey: ['custom-orders', 'mine'],
    queryFn: () => customOrdersService.mine(),
    enabled,
  });
}

export function useCustomOrder(id) {
  return useQuery({
    queryKey: ['custom-orders', id],
    queryFn: () => customOrdersService.getById(id),
    enabled: Boolean(id),
  });
}

export function useCustomOrderMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['custom-orders'] });

  const create = useMutation({
    mutationFn: (payload) => customOrdersService.create(payload),
    onSuccess: invalidate,
  });
  const cancel = useMutation({
    mutationFn: (id) => customOrdersService.cancel(id),
    onSuccess: invalidate,
  });

  return { create, cancel };
}
