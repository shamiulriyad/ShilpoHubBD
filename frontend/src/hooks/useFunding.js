import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fundingService } from '../services/fundingService';

export function useFundingPrograms(params = {}) {
  return useQuery({ queryKey: ['funding-programs', 'list', params], queryFn: () => fundingService.listPrograms(params) });
}

export function useFundingApplications(params = {}) {
  return useQuery({ queryKey: ['funding-applications', 'list', params], queryFn: () => fundingService.listApplications(params) });
}

export function useFundingApplication(id) {
  return useQuery({
    queryKey: ['funding-applications', id],
    queryFn: () => fundingService.getApplication(id),
    enabled: Boolean(id),
  });
}

export function useFundingMutations() {
  const queryClient = useQueryClient();
  const invalidateApplication = (id) => {
    queryClient.invalidateQueries({ queryKey: ['funding-applications'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['funding-applications', id] });
  };

  return {
    createProgram: useMutation({
      mutationFn: (payload) => fundingService.createProgram(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['funding-programs'] }),
    }),
    updateProgram: useMutation({
      mutationFn: ({ id, payload }) => fundingService.updateProgram(id, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['funding-programs'] }),
    }),
    createApplication: useMutation({
      mutationFn: (payload) => fundingService.createApplication(payload),
      onSuccess: () => invalidateApplication(),
    }),
    submitReview: useMutation({
      mutationFn: ({ id, payload }) => fundingService.submitReview(id, payload),
      onSuccess: (_, { id }) => invalidateApplication(id),
    }),
    decideApplication: useMutation({
      mutationFn: ({ id, payload }) => fundingService.decideApplication(id, payload),
      onSuccess: (_, { id }) => invalidateApplication(id),
    }),
    scheduleDisbursement: useMutation({
      mutationFn: ({ id, payload }) => fundingService.scheduleDisbursement(id, payload),
      onSuccess: (_, { id }) => invalidateApplication(id),
    }),
    updateDisbursementStatus: useMutation({
      mutationFn: ({ id, disbursementId, payload }) => fundingService.updateDisbursementStatus(id, disbursementId, payload),
      onSuccess: (_, { id }) => invalidateApplication(id),
    }),
    recordRepayment: useMutation({
      mutationFn: ({ id, payload }) => fundingService.recordRepayment(id, payload),
      onSuccess: (_, { id }) => invalidateApplication(id),
    }),
  };
}
