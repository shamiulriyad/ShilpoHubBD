import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { producerReturnsService } from '../services/producerReturnsService';

export function useProducerReturns() {
  return useQuery({ queryKey: ['producer-returns'], queryFn: () => producerReturnsService.list() });
}

export function useProducerReturnMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['producer-returns'] });
    queryClient.invalidateQueries({ queryKey: ['producer-orders'] });
  };
  return {
    accept: useMutation({ mutationFn: (orderId) => producerReturnsService.accept(orderId), onSuccess: invalidate }),
    reject: useMutation({ mutationFn: ({ orderId, note }) => producerReturnsService.reject(orderId, note), onSuccess: invalidate }),
  };
}
