import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { csrSponsorshipService } from '../services/csrSponsorshipService';

export function useCsrOpportunities(params = {}) {
  return useQuery({ queryKey: ['csr', 'opportunities', params], queryFn: () => csrSponsorshipService.listOpportunities(params) });
}

export function useMyCsrOpportunities() {
  return useQuery({ queryKey: ['csr', 'opportunities', 'mine'], queryFn: () => csrSponsorshipService.myOpportunities() });
}

export function useCsrOpportunity(id) {
  return useQuery({ queryKey: ['csr', 'opportunities', id], queryFn: () => csrSponsorshipService.getOpportunity(id), enabled: Boolean(id) });
}

export function useCsrOpportunityProposals(id) {
  return useQuery({
    queryKey: ['csr', 'opportunities', id, 'proposals'],
    queryFn: () => csrSponsorshipService.opportunityProposals(id),
    enabled: Boolean(id),
  });
}

export function useMyCsrProposals(params = {}) {
  return useQuery({ queryKey: ['csr', 'proposals', 'mine', params], queryFn: () => csrSponsorshipService.myProposals(params) });
}

export function useCsrProposal(id) {
  return useQuery({ queryKey: ['csr', 'proposals', id], queryFn: () => csrSponsorshipService.getProposal(id), enabled: Boolean(id) });
}

export function useCsrSponsorshipMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['csr'] });

  const createOpportunity = useMutation({ mutationFn: (payload) => csrSponsorshipService.createOpportunity(payload), onSuccess: invalidate });
  const closeOpportunity = useMutation({ mutationFn: (id) => csrSponsorshipService.closeOpportunity(id), onSuccess: invalidate });
  const cancelOpportunity = useMutation({ mutationFn: (id) => csrSponsorshipService.cancelOpportunity(id), onSuccess: invalidate });
  const submitProposal = useMutation({
    mutationFn: ({ id, payload }) => csrSponsorshipService.submitProposal(id, payload),
    onSuccess: invalidate,
  });
  const decideProposal = useMutation({
    mutationFn: ({ id, payload }) => csrSponsorshipService.decideProposal(id, payload),
    onSuccess: invalidate,
  });
  const addMilestone = useMutation({
    mutationFn: ({ id, payload }) => csrSponsorshipService.addMilestone(id, payload),
    onSuccess: invalidate,
  });
  const addProgressUpdate = useMutation({
    mutationFn: ({ id, content }) => csrSponsorshipService.addProgressUpdate(id, content),
    onSuccess: invalidate,
  });
  const addImpactRecord = useMutation({
    mutationFn: ({ id, payload }) => csrSponsorshipService.addImpactRecord(id, payload),
    onSuccess: invalidate,
  });
  const completeProposal = useMutation({ mutationFn: (id) => csrSponsorshipService.completeProposal(id), onSuccess: invalidate });
  const cancelProposal = useMutation({ mutationFn: (id) => csrSponsorshipService.cancelProposal(id), onSuccess: invalidate });

  return {
    createOpportunity,
    closeOpportunity,
    cancelOpportunity,
    submitProposal,
    decideProposal,
    addMilestone,
    addProgressUpdate,
    addImpactRecord,
    completeProposal,
    cancelProposal,
  };
}
