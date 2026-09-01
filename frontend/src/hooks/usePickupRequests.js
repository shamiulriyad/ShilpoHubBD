import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pickupRequestsService } from '../services/pickupRequestsService';

export function usePickupRequests(params = {}) {
  return useQuery({
    queryKey: ['pickup-requests', 'list', params],
    queryFn: () => pickupRequestsService.list(params),
  });
}

export function usePickupRequest(id) {
  return useQuery({
    queryKey: ['pickup-requests', id],
    queryFn: () => pickupRequestsService.getById(id),
    enabled: Boolean(id),
  });
}

export function usePickupRequestMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['pickup-requests'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['pickup-requests', id] });
  };

  return {
    create: useMutation({
      mutationFn: (payload) => pickupRequestsService.create(payload),
      onSuccess: () => invalidate(),
    }),
    update: useMutation({
      mutationFn: ({ id, payload }) => pickupRequestsService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    schedule: useMutation({
      mutationFn: ({ id, payload }) => pickupRequestsService.schedule(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    assign: useMutation({
      mutationFn: ({ id, payload }) => pickupRequestsService.assign(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateStatus: useMutation({
      mutationFn: ({ id, payload }) => pickupRequestsService.updateStatus(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    cancel: useMutation({
      mutationFn: ({ id, payload }) => pickupRequestsService.cancel(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({
      mutationFn: (id) => pickupRequestsService.remove(id),
      onSuccess: () => invalidate(),
    }),
  };
}
