import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { inventoryService } from '../services/inventoryService';

export function useLowStockProducts() {
  return useQuery({ queryKey: ['inventory', 'low-stock'], queryFn: () => inventoryService.lowStock() });
}

export function useInventoryHistory(productId) {
  return useQuery({
    queryKey: ['inventory', 'history', productId],
    queryFn: () => inventoryService.history(productId),
    enabled: Boolean(productId),
  });
}

export function useAdjustStock() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, payload }) => inventoryService.adjustStock(productId, payload),
    onSuccess: (_, { productId }) => {
      queryClient.invalidateQueries({ queryKey: ['inventory'] });
      queryClient.invalidateQueries({ queryKey: ['inventory', 'history', productId] });
    },
  });
}
