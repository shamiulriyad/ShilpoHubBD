import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contractsService } from '../services/contractsService';

export function useMyContracts(params = {}) {
  return useQuery({ queryKey: ['contracts', 'mine', params], queryFn: () => contractsService.mine(params) });
}

export function useReceivedContracts(params = {}) {
  return useQuery({ queryKey: ['contracts', 'received', params], queryFn: () => contractsService.received(params) });
}

export function useContract(id) {
  return useQuery({ queryKey: ['contracts', id], queryFn: () => contractsService.getById(id), enabled: Boolean(id) });
}

export function useContractMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['contracts'] });

  const create = useMutation({ mutationFn: (payload) => contractsService.create(payload), onSuccess: invalidate });
  const accept = useMutation({ mutationFn: (id) => contractsService.accept(id), onSuccess: invalidate });
  const reject = useMutation({ mutationFn: ({ id, notes }) => contractsService.reject(id, notes), onSuccess: invalidate });
  const terminate = useMutation({ mutationFn: (id) => contractsService.terminate(id), onSuccess: invalidate });
  const renew = useMutation({ mutationFn: ({ id, payload }) => contractsService.renew(id, payload), onSuccess: invalidate });

  return { create, accept, reject, terminate, renew };
}
