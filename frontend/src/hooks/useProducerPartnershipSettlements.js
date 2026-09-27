import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { producerPartnershipSettlementService } from '../services/producerPartnershipSettlementService';

export function useProducerPartnershipSettlements(params = {}) {
  return useQuery({ queryKey: ['producer-partnership-settlements', 'list', params], queryFn: () => producerPartnershipSettlementService.list(params) });
}

export function useSettlementsForAgreement(agreementId) {
  return useQuery({
    queryKey: ['producer-partnership-settlements', 'agreement', agreementId],
    queryFn: () => producerPartnershipSettlementService.getForAgreement(agreementId),
    enabled: Boolean(agreementId),
  });
}

export function useProducerPartnershipSettlementMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-partnership-settlements'] });

  const generate = useMutation({
    mutationFn: ({ agreementId, periodStart, periodEnd }) => producerPartnershipSettlementService.generate(agreementId, periodStart, periodEnd),
    onSuccess: invalidate,
  });
  const submitForApproval = useMutation({ mutationFn: (id) => producerPartnershipSettlementService.submitForApproval(id), onSuccess: invalidate });
  const approve = useMutation({ mutationFn: (id) => producerPartnershipSettlementService.approve(id), onSuccess: invalidate });
  const reject = useMutation({ mutationFn: ({ id, reason }) => producerPartnershipSettlementService.reject(id, reason), onSuccess: invalidate });

  return { generate, submitForApproval, approve, reject };
}
