import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { shipmentsService } from '../services/shipmentsService';

export function useShipments(params = {}) {
  return useQuery({
    queryKey: ['shipments', 'list', params],
    queryFn: () => shipmentsService.list(params),
  });
}

export function useShipment(id) {
  return useQuery({
    queryKey: ['shipments', id],
    queryFn: () => shipmentsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useTrackShipment(trackingNumber) {
  return useQuery({
    queryKey: ['shipments', 'track', trackingNumber],
    queryFn: () => shipmentsService.track(trackingNumber),
    enabled: Boolean(trackingNumber),
    retry: false,
  });
}

export function useShipmentMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['shipments'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['shipments', id] });
  };

  return {
    create: useMutation({
      mutationFn: (payload) => shipmentsService.create(payload),
      onSuccess: () => invalidate(),
    }),
    update: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateStatus: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.updateStatus(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addEvent: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.addEvent(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    recordDeliveryAttempt: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.recordDeliveryAttempt(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    markDelivered: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.markDelivered(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    cancel: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.cancel(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addNote: useMutation({
      mutationFn: ({ id, payload }) => shipmentsService.addNote(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({
      mutationFn: (id) => shipmentsService.remove(id),
      onSuccess: () => invalidate(),
    }),
  };
}
