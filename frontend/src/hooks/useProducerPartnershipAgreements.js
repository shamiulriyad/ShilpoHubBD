import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { producerPartnershipAgreementService } from '../services/producerPartnershipAgreementService';

export function useMyPartnershipAgreements(params = {}) {
  return useQuery({ queryKey: ['producer-partnership-agreements', 'mine', params], queryFn: () => producerPartnershipAgreementService.mine(params) });
}

export function useReceivedPartnershipAgreements(params = {}) {
  return useQuery({ queryKey: ['producer-partnership-agreements', 'received', params], queryFn: () => producerPartnershipAgreementService.received(params) });
}

export function usePartnershipAgreement(id) {
  return useQuery({
    queryKey: ['producer-partnership-agreements', id],
    queryFn: () => producerPartnershipAgreementService.getById(id),
    enabled: Boolean(id),
  });
}

export function useSettlementEligibility(id) {
  return useQuery({
    queryKey: ['producer-partnership-agreements', id, 'settlement-eligibility'],
    queryFn: () => producerPartnershipAgreementService.getSettlementEligibility(id),
    enabled: Boolean(id),
  });
}

export function usePartnershipAgreementMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['producer-partnership-agreements'] });

  const create = useMutation({ mutationFn: (payload) => producerPartnershipAgreementService.create(payload), onSuccess: invalidate });
  const updateTerms = useMutation({ mutationFn: ({ id, payload }) => producerPartnershipAgreementService.updateTerms(id, payload), onSuccess: invalidate });
  const submitForConfirmation = useMutation({ mutationFn: (id) => producerPartnershipAgreementService.submitForConfirmation(id), onSuccess: invalidate });
  const confirm = useMutation({ mutationFn: (id) => producerPartnershipAgreementService.confirm(id), onSuccess: invalidate });
  const suspend = useMutation({ mutationFn: ({ id, reason }) => producerPartnershipAgreementService.suspend(id, reason), onSuccess: invalidate });
  const resume = useMutation({ mutationFn: (id) => producerPartnershipAgreementService.resume(id), onSuccess: invalidate });
  const complete = useMutation({ mutationFn: (id) => producerPartnershipAgreementService.complete(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: ({ id, reason }) => producerPartnershipAgreementService.cancel(id, reason), onSuccess: invalidate });

  return { create, updateTerms, submitForConfirmation, confirm, suspend, resume, complete, cancel };
}
