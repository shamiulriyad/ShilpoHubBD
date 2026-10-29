import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { producerReturnsService } from '../services/producerReturnsService';
import { mergeDemoArray } from '../data/producerDemoData';

export function useProducerReturns() {
  return useQuery({ queryKey: ['producer-returns'], queryFn: async () => mergeDemoArray(await producerReturnsService.list(), 'returns') });
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
