import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { producerFollowsService } from '../services/producerFollowsService';

export function useFollowedProducers() {
  return useQuery({
    queryKey: ['producer-follows'],
    queryFn: () => producerFollowsService.list(),
  });
}

export function useProducerFollowMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-follows'] });

  const follow = useMutation({
    mutationFn: (producerId) => producerFollowsService.follow(producerId),
    onSuccess: invalidate,
  });

  const unfollow = useMutation({
    mutationFn: (producerId) => producerFollowsService.unfollow(producerId),
    onSuccess: invalidate,
  });

  return { follow, unfollow };
}
