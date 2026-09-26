import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { procurementsService } from '../services/procurementsService';

export function useMyProcurements(params = {}) {
  return useQuery({ queryKey: ['procurements', 'mine', params], queryFn: () => procurementsService.mine(params) });
}

export function useIncomingProcurements(params = {}) {
  return useQuery({ queryKey: ['procurements', 'incoming', params], queryFn: () => procurementsService.incoming(params) });
}

export function useProcurementInspections(params = {}) {
  return useQuery({ queryKey: ['procurements', 'inspections', params], queryFn: () => procurementsService.inspections(params) });
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
  const payAdvance = useMutation({ mutationFn: ({ id, payload }) => procurementsService.payAdvance(id, payload), onSuccess: invalidate });
  const inspect = useMutation({ mutationFn: ({ id, payload }) => procurementsService.inspect(id, payload), onSuccess: invalidate });
  const convertToOrder = useMutation({ mutationFn: (id) => procurementsService.convertToOrder(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => procurementsService.cancel(id), onSuccess: invalidate });

  return { create, createFromQuotation, approve, reject, payAdvance, inspect, convertToOrder, cancel };
}
