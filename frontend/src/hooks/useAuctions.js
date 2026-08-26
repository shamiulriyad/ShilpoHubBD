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

export function usePlaceBid(id) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (amount) => auctionsService.placeBid(id, amount),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['auctions', id] });
    },
  });
}
