import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quotationsService } from '../services/quotationsService';

export function useMyQuotations(params = {}) {
  return useQuery({ queryKey: ['quotations', 'mine', params], queryFn: () => quotationsService.mine(params) });
}

export function useReceivedQuotations(params = {}) {
  return useQuery({ queryKey: ['quotations', 'received', params], queryFn: () => quotationsService.received(params) });
}

export function useQuotation(id) {
  return useQuery({ queryKey: ['quotations', id], queryFn: () => quotationsService.getById(id), enabled: Boolean(id) });
}

export function useQuotationComparison(id, enabled = true) {
  return useQuery({ queryKey: ['quotations', id, 'compare'], queryFn: () => quotationsService.compare(id), enabled: Boolean(id) && enabled });
}

export function useQuotationMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['quotations'] });

  const create = useMutation({ mutationFn: (payload) => quotationsService.create(payload), onSuccess: invalidate });
  const submitResponse = useMutation({
    mutationFn: ({ id, payload }) => quotationsService.submitResponse(id, payload),
    onSuccess: invalidate,
  });
  const decideResponse = useMutation({
    mutationFn: ({ id, responseId, payload }) => quotationsService.decideResponse(id, responseId, payload),
    onSuccess: invalidate,
  });
  const cancel = useMutation({ mutationFn: (id) => quotationsService.cancel(id), onSuccess: invalidate });

  return { create, submitResponse, decideResponse, cancel };
}
