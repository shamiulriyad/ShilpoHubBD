import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { deliveryRoutesService } from '../services/deliveryRoutesService';

export function useDeliveryRoutes(params = {}) {
  return useQuery({
    queryKey: ['delivery-routes', 'list', params],
    queryFn: () => deliveryRoutesService.list(params),
  });
}

export function useDeliveryRoute(id) {
  return useQuery({
    queryKey: ['delivery-routes', id],
    queryFn: () => deliveryRoutesService.getById(id),
    enabled: Boolean(id),
  });
}

export function useDeliveryRouteMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['delivery-routes'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['delivery-routes', id] });
  };

  const simple = (fn) => useMutation({
    mutationFn: fn,
    onSuccess: (_, args) => invalidate(typeof args === 'object' ? args.id : args),
  });

  return {
    create: useMutation({ mutationFn: (payload) => deliveryRoutesService.create(payload), onSuccess: () => invalidate() }),
    addStop: simple(({ id, payload }) => deliveryRoutesService.addStop(id, payload)),
    removeStop: simple(({ id, stopId }) => deliveryRoutesService.removeStop(id, stopId)),
    optimize: simple(({ id, payload }) => deliveryRoutesService.optimize(id, payload)),
    assign: simple(({ id, payload }) => deliveryRoutesService.assign(id, payload)),
    dispatch: simple((id) => deliveryRoutesService.dispatch(id)),
    start: simple((id) => deliveryRoutesService.start(id)),
    complete: simple((id) => deliveryRoutesService.complete(id)),
    cancel: simple(({ id, payload }) => deliveryRoutesService.cancel(id, payload)),
    arriveStop: simple(({ id, stopId }) => deliveryRoutesService.arriveStop(id, stopId)),
    completeStop: simple(({ id, stopId, payload }) => deliveryRoutesService.completeStop(id, stopId, payload)),
    skipStop: simple(({ id, stopId }) => deliveryRoutesService.skipStop(id, stopId)),
    failStop: simple(({ id, stopId, payload }) => deliveryRoutesService.failStop(id, stopId, payload)),
    remove: useMutation({ mutationFn: (id) => deliveryRoutesService.remove(id), onSuccess: () => invalidate() }),
  };
}
