import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { heritageIdentityService } from '../services/heritageIdentityService';

export function useHeritageIdentity(producerId) {
  return useQuery({
    queryKey: ['heritage-identity', producerId],
    queryFn: () => heritageIdentityService.getByProducer(producerId),
    enabled: Boolean(producerId),
    retry: false,
  });
}

export function useVerifiedHeritageIdentities(params = {}) {
  return useQuery({ queryKey: ['heritage-identity', 'verified', params], queryFn: () => heritageIdentityService.getVerified(params) });
}

export function useVerifyHeritageIdentity() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ producerId, payload }) => heritageIdentityService.verify(producerId, payload),
    onSuccess: (_, { producerId }) => queryClient.invalidateQueries({ queryKey: ['heritage-identity', producerId] }),
  });
}
