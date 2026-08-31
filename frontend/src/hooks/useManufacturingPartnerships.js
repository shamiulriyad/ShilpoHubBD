import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { manufacturingPartnershipsService } from '../services/manufacturingPartnershipsService';

export function useMyPartnerships(params = {}) {
  return useQuery({ queryKey: ['partnerships', 'mine', params], queryFn: () => manufacturingPartnershipsService.mine(params) });
}

export function useReceivedPartnerships(params = {}) {
  return useQuery({ queryKey: ['partnerships', 'received', params], queryFn: () => manufacturingPartnershipsService.received(params) });
}

export function usePartnership(id) {
  return useQuery({ queryKey: ['partnerships', id], queryFn: () => manufacturingPartnershipsService.getById(id), enabled: Boolean(id) });
}

export function usePartnershipMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['partnerships'] });

  const create = useMutation({ mutationFn: (payload) => manufacturingPartnershipsService.create(payload), onSuccess: invalidate });
  const respond = useMutation({
    mutationFn: ({ id, payload }) => manufacturingPartnershipsService.respond(id, payload),
    onSuccess: invalidate,
  });
  const addMilestone = useMutation({
    mutationFn: ({ id, payload }) => manufacturingPartnershipsService.addMilestone(id, payload),
    onSuccess: invalidate,
  });
  const updateMilestoneStatus = useMutation({
    mutationFn: ({ id, milestoneId, status }) => manufacturingPartnershipsService.updateMilestoneStatus(id, milestoneId, status),
    onSuccess: invalidate,
  });
  const complete = useMutation({ mutationFn: (id) => manufacturingPartnershipsService.complete(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => manufacturingPartnershipsService.cancel(id), onSuccess: invalidate });

  return { create, respond, addMilestone, updateMilestoneStatus, complete, cancel };
}
