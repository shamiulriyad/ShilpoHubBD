import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { investmentOpportunitiesService } from '../services/investmentOpportunitiesService';

export function useInvestmentOpportunities(params = {}) {
  return useQuery({ queryKey: ['investments', 'opportunities', params], queryFn: () => investmentOpportunitiesService.list(params) });
}

export function useMyInvestmentOpportunities() {
  return useQuery({ queryKey: ['investments', 'opportunities', 'mine'], queryFn: () => investmentOpportunitiesService.mine() });
}

export function useInvestmentOpportunity(id) {
  return useQuery({
    queryKey: ['investments', 'opportunities', id],
    queryFn: () => investmentOpportunitiesService.getById(id),
    enabled: Boolean(id),
  });
}

export function useInvestmentOpportunityProposals(id) {
  return useQuery({
    queryKey: ['investments', 'opportunities', id, 'proposals'],
    queryFn: () => investmentOpportunitiesService.opportunityProposals(id),
    enabled: Boolean(id),
  });
}

export function useMyInvestmentProposals() {
  return useQuery({ queryKey: ['investments', 'proposals', 'mine'], queryFn: () => investmentOpportunitiesService.myProposals() });
}

export function useInvestmentProposal(id) {
  return useQuery({
    queryKey: ['investments', 'proposals', id],
    queryFn: () => investmentOpportunitiesService.getProposal(id),
    enabled: Boolean(id),
  });
}

export function useInvestmentOpportunityMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['investments'] });

  const create = useMutation({ mutationFn: (payload) => investmentOpportunitiesService.create(payload), onSuccess: invalidate });
  const close = useMutation({ mutationFn: (id) => investmentOpportunitiesService.close(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => investmentOpportunitiesService.cancel(id), onSuccess: invalidate });
  const submitProposal = useMutation({
    mutationFn: ({ id, payload }) => investmentOpportunitiesService.submitProposal(id, payload),
    onSuccess: invalidate,
  });
  const decideProposal = useMutation({
    mutationFn: ({ id, payload }) => investmentOpportunitiesService.decideProposal(id, payload),
    onSuccess: invalidate,
  });
  const addMilestone = useMutation({
    mutationFn: ({ id, payload }) => investmentOpportunitiesService.addMilestone(id, payload),
    onSuccess: invalidate,
  });
  const addDocument = useMutation({
    mutationFn: ({ id, payload }) => investmentOpportunitiesService.addDocument(id, payload),
    onSuccess: invalidate,
  });
  const completeProposal = useMutation({ mutationFn: (id) => investmentOpportunitiesService.completeProposal(id), onSuccess: invalidate });
  const cancelProposal = useMutation({ mutationFn: (id) => investmentOpportunitiesService.cancelProposal(id), onSuccess: invalidate });

  return { create, close, cancel, submitProposal, decideProposal, addMilestone, addDocument, completeProposal, cancelProposal };
}
