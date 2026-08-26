import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { procurementsService } from '../services/procurementsService';

export function useMyProcurements(params = {}) {
  return useQuery({ queryKey: ['procurements', 'mine', params], queryFn: () => procurementsService.mine(params) });
}

export function useProcurement(id) {
  return useQuery({ queryKey: ['procurements', id], queryFn: () => procurementsService.getById(id), enabled: Boolean(id) });
}

export function useProcurementMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['procurements'] });

  const create = useMutation({ mutationFn: (payload) => procurementsService.create(payload), onSuccess: invalidate });
  const createFromQuotation = useMutation({
    mutationFn: ({ quotationResponseId, payload }) => procurementsService.createFromQuotation(quotationResponseId, payload),
    onSuccess: invalidate,
  });
  const approve = useMutation({ mutationFn: ({ id, notes }) => procurementsService.approve(id, notes), onSuccess: invalidate });
  const reject = useMutation({ mutationFn: ({ id, notes }) => procurementsService.reject(id, notes), onSuccess: invalidate });
  const convertToOrder = useMutation({ mutationFn: (id) => procurementsService.convertToOrder(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => procurementsService.cancel(id), onSuccess: invalidate });

  return { create, createFromQuotation, approve, reject, convertToOrder, cancel };
}
