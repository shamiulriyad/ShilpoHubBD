import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { producerOrdersService } from '../services/producerOrdersService';

export function useProducerOrderItems(params = {}) {
  return useQuery({ queryKey: ['producer-orders', params], queryFn: () => producerOrdersService.list(params) });
}

export function useProducerOrderItem(id) {
  return useQuery({ queryKey: ['producer-orders', id], queryFn: () => producerOrdersService.getById(id), enabled: Boolean(id) });
}

export function useProducerCustomers() {
  return useQuery({ queryKey: ['producer-orders', 'customers'], queryFn: () => producerOrdersService.customers() });
}

export function useProducerRevenue(params = {}) {
  return useQuery({ queryKey: ['producer-orders', 'revenue', params], queryFn: () => producerOrdersService.revenue(params) });
}

export function useProducerSales(params = {}) {
  return useQuery({ queryKey: ['producer-orders', 'sales', params], queryFn: () => producerOrdersService.sales(params) });
}

export function useProducerVisitors() {
  return useQuery({ queryKey: ['producer-orders', 'visitors'], queryFn: () => producerOrdersService.visitors() });
}

export function useProducerProductPerformance() {
  return useQuery({ queryKey: ['producer-orders', 'product-performance'], queryFn: () => producerOrdersService.productPerformance() });
}

export function useProducerOrderMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-orders'] });

  const accept = useMutation({ mutationFn: (id) => producerOrdersService.accept(id), onSuccess: invalidate });
  const reject = useMutation({ mutationFn: ({ id, reason }) => producerOrdersService.reject(id, reason), onSuccess: invalidate });
  const startProcessing = useMutation({ mutationFn: (id) => producerOrdersService.startProcessing(id), onSuccess: invalidate });
  const ship = useMutation({ mutationFn: ({ id, payload }) => producerOrdersService.ship(id, payload), onSuccess: invalidate });
  const deliver = useMutation({ mutationFn: (id) => producerOrdersService.deliver(id), onSuccess: invalidate });

  return { accept, reject, startProcessing, ship, deliver };
}
