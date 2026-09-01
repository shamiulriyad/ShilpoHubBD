import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { logisticsReturnsService } from '../services/logisticsReturnsService';

export function useLogisticsReturns(params = {}) {
  return useQuery({
    queryKey: ['logistics-returns', 'list', params],
    queryFn: () => logisticsReturnsService.list(params),
  });
}

export function useLogisticsReturn(id) {
  return useQuery({
    queryKey: ['logistics-returns', id],
    queryFn: () => logisticsReturnsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useLogisticsReturnMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['logistics-returns'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['logistics-returns', id] });
  };

  return {
    create: useMutation({
      mutationFn: (payload) => logisticsReturnsService.create(payload),
      onSuccess: () => invalidate(),
    }),
    approve: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.approve(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    reject: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.reject(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    schedulePickup: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.schedulePickup(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateStatus: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.updateStatus(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    recordInspection: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.recordInspection(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    restock: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.restock(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    recordRefund: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.recordRefund(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    close: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.close(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    cancel: useMutation({
      mutationFn: ({ id, payload }) => logisticsReturnsService.cancel(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
