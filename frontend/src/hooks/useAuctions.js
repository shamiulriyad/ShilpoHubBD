import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { auctionsService } from '../services/auctionsService';

export function useAuctions(params = {}) {
  return useQuery({
    queryKey: ['auctions', params],
    queryFn: () => auctionsService.list(params),
  });
}

export function useAuction(id) {
  return useQuery({
    queryKey: ['auctions', id],
    queryFn: () => auctionsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useMyAuctions(params = {}) {
  return useQuery({
    queryKey: ['auctions', 'mine', params],
    queryFn: () => auctionsService.mine(params),
  });
}

export function useProducerAuctionMutations() {
  const queryClient = useQueryClient();
  // Both the producer's own list and the public (customer) lists change.
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['auctions'] });
  const create = useMutation({ mutationFn: (payload) => auctionsService.create(payload), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => auctionsService.cancel(id), onSuccess: invalidate });
  return { create, cancel };
}

export function usePlaceBid(id) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (amount) => auctionsService.placeBid(id, amount),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auctions', id] });
    },
  });
}
