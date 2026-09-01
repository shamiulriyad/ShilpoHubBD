import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { logisticsPartnersService } from '../services/logisticsPartnersService';

export function useMyLogisticsPartnerProfile() {
  return useQuery({
    queryKey: ['logistics-partners', 'me'],
    queryFn: () => logisticsPartnersService.getMine(),
    retry: false,
  });
}

export function useLogisticsPartnersList(params = {}) {
  return useQuery({
    queryKey: ['logistics-partners', 'list', params],
    queryFn: () => logisticsPartnersService.list(params),
  });
}

export function useUpsertLogisticsPartnerProfile(userId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => logisticsPartnersService.upsert(userId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['logistics-partners'] }),
  });
}

export function useVerifyLogisticsPartner() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, payload }) => logisticsPartnersService.verify(userId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['logistics-partners'] }),
  });
}

export function useUpsertLogisticsServiceArea(userId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => logisticsPartnersService.upsertServiceArea(userId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['logistics-partners'] }),
  });
}

export function useRemoveLogisticsServiceArea(userId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (serviceAreaId) => logisticsPartnersService.removeServiceArea(userId, serviceAreaId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['logistics-partners'] }),
  });
}
