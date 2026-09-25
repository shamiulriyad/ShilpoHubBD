import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { orderComplaintsService } from '../services/orderComplaintsService';

export function useMyComplaints() {
  return useQuery({ queryKey: ['order-complaints', 'mine'], queryFn: () => orderComplaintsService.mine() });
}

export function useReceivedComplaints() {
  return useQuery({ queryKey: ['order-complaints', 'received'], queryFn: () => orderComplaintsService.received() });
}

export function useComplaintMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['order-complaints'] });
  return {
    create: useMutation({ mutationFn: (payload) => orderComplaintsService.create(payload), onSuccess: invalidate }),
    respond: useMutation({ mutationFn: ({ id, message }) => orderComplaintsService.respond(id, message), onSuccess: invalidate }),
    satisfied: useMutation({ mutationFn: ({ id, note }) => orderComplaintsService.satisfied(id, note), onSuccess: invalidate }),
    reopen: useMutation({ mutationFn: ({ id, note }) => orderComplaintsService.reopen(id, note), onSuccess: invalidate }),
    withdraw: useMutation({ mutationFn: (id) => orderComplaintsService.withdraw(id), onSuccess: invalidate }),
  };
}
