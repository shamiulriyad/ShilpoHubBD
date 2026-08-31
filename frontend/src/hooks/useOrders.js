import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ordersService } from '../services/ordersService';

export function useOrders(params = {}, enabled = true) {
  return useQuery({
    queryKey: ['orders', params],
    queryFn: () => ordersService.list(params),
    enabled,
  });
}

export function useOrder(id) {
  return useQuery({
    queryKey: ['orders', id],
    queryFn: () => ordersService.getById(id),
    enabled: Boolean(id),
  });
}

export function useOrderTracking(id) {
  return useQuery({
    queryKey: ['orders', id, 'tracking'],
    queryFn: () => ordersService.getTracking(id),
    enabled: Boolean(id),
  });
}

export function useCheckout() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => ordersService.checkout(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
}

export function useOrderMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['orders'] });
    queryClient.invalidateQueries({ queryKey: ['orders', id] });
  };

  const cancel = useMutation({
    mutationFn: ({ id, reason }) => ordersService.cancel(id, reason ? { reason } : {}),
    onSuccess: (_, { id }) => invalidate(id),
  });

  const requestReturn = useMutation({
    mutationFn: ({ id, reason }) => ordersService.requestReturn(id, { reason }),
    onSuccess: (_, { id }) => invalidate(id),
  });

  return { cancel, requestReturn };
}
