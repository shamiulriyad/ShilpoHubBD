import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { warehousesService } from '../services/warehousesService';

export function useWarehouses(params = {}) {
  return useQuery({
    queryKey: ['warehouses', 'list', params],
    queryFn: () => warehousesService.list(params),
  });
}

export function useWarehouse(id) {
  return useQuery({
    queryKey: ['warehouses', id],
    queryFn: () => warehousesService.getById(id),
    enabled: Boolean(id),
  });
}

export function useWarehouseMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['warehouses'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['warehouses', id] });
  };

  return {
    create: useMutation({
      mutationFn: (payload) => warehousesService.create(payload),
      onSuccess: () => invalidate(),
    }),
    update: useMutation({
      mutationFn: ({ id, payload }) => warehousesService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({
      mutationFn: (id) => warehousesService.remove(id),
      onSuccess: () => invalidate(),
    }),
    addZone: useMutation({
      mutationFn: ({ id, payload }) => warehousesService.addZone(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateZone: useMutation({
      mutationFn: ({ id, zoneId, payload }) => warehousesService.updateZone(id, zoneId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeZone: useMutation({
      mutationFn: ({ id, zoneId }) => warehousesService.removeZone(id, zoneId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addBin: useMutation({
      mutationFn: ({ id, payload }) => warehousesService.addBin(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateBin: useMutation({
      mutationFn: ({ id, binId, payload }) => warehousesService.updateBin(id, binId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeBin: useMutation({
      mutationFn: ({ id, binId }) => warehousesService.removeBin(id, binId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
