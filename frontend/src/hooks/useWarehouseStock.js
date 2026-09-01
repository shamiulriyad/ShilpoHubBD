import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { warehouseStockService } from '../services/warehouseStockService';

export function useWarehouseStockItems(params = {}) {
  return useQuery({
    queryKey: ['warehouse-stock', 'items', params],
    queryFn: () => warehouseStockService.listItems(params),
  });
}

export function useWarehouseStockMovements(params = {}) {
  return useQuery({
    queryKey: ['warehouse-stock', 'movements', params],
    queryFn: () => warehouseStockService.listMovements(params),
    enabled: Boolean(params.warehouseId || params.warehouseStockItemId),
  });
}

export function useWarehouseStockItem(id) {
  return useQuery({
    queryKey: ['warehouse-stock', id],
    queryFn: () => warehouseStockService.getById(id),
    enabled: Boolean(id),
  });
}

export function useWarehouseStockMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['warehouse-stock'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['warehouse-stock', id] });
  };

  return {
    receive: useMutation({
      mutationFn: (payload) => warehouseStockService.receive(payload),
      onSuccess: () => invalidate(),
    }),
    issue: useMutation({
      mutationFn: ({ id, payload }) => warehouseStockService.issue(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    transfer: useMutation({
      mutationFn: ({ id, payload }) => warehouseStockService.transfer(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    adjust: useMutation({
      mutationFn: ({ id, payload }) => warehouseStockService.adjust(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    reserve: useMutation({
      mutationFn: ({ id, payload }) => warehouseStockService.reserve(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    release: useMutation({
      mutationFn: ({ id, payload }) => warehouseStockService.release(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({
      mutationFn: (id) => warehouseStockService.remove(id),
      onSuccess: () => invalidate(),
    }),
  };
}
