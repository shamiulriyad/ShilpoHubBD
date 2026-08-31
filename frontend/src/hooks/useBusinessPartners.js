import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { businessPartnersService } from '../services/businessPartnersService';

export function useBusinessPartnerProfile(userId) {
  return useQuery({
    queryKey: ['business-partners', userId],
    queryFn: () => businessPartnersService.getById(userId),
    enabled: Boolean(userId),
    retry: false,
  });
}

export function useUpsertBusinessPartnerProfile(userId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => businessPartnersService.upsert(userId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['business-partners', userId] }),
  });
}

export function useBusinessPartnersList(params = {}) {
  return useQuery({ queryKey: ['business-partners', 'list', params], queryFn: () => businessPartnersService.list(params) });
}

export function useVerifyBusinessPartner() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, payload }) => businessPartnersService.verify(userId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['business-partners'] }),
  });
}
