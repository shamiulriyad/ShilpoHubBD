import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { policyComplianceService } from '../services/policyComplianceService';

export function usePolicySimulations(params = {}) {
  return useQuery({ queryKey: ['policy-simulations', params], queryFn: () => policyComplianceService.listSimulations(params) });
}

export function useComplianceRecords(params = {}) {
  return useQuery({ queryKey: ['compliance-records', 'list', params], queryFn: () => policyComplianceService.listComplianceRecords(params) });
}

export function useComplianceRecord(id) {
  return useQuery({
    queryKey: ['compliance-records', id],
    queryFn: () => policyComplianceService.getComplianceRecord(id),
    enabled: Boolean(id),
  });
}

export function usePolicyComplianceMutations() {
  const queryClient = useQueryClient();
  const invalidateCompliance = (id) => {
    queryClient.invalidateQueries({ queryKey: ['compliance-records'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['compliance-records', id] });
  };

  return {
    runSimulation: useMutation({
      mutationFn: (payload) => policyComplianceService.runSimulation(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['policy-simulations'] }),
    }),
    removeSimulation: useMutation({
      mutationFn: (id) => policyComplianceService.removeSimulation(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['policy-simulations'] }),
    }),
    createComplianceRecord: useMutation({
      mutationFn: (payload) => policyComplianceService.createComplianceRecord(payload),
      onSuccess: () => invalidateCompliance(),
    }),
    updateComplianceRecord: useMutation({
      mutationFn: ({ id, payload }) => policyComplianceService.updateComplianceRecord(id, payload),
      onSuccess: (_, { id }) => invalidateCompliance(id),
    }),
    upsertRequirement: useMutation({
      mutationFn: ({ id, payload }) => policyComplianceService.upsertRequirement(id, payload),
      onSuccess: (_, { id }) => invalidateCompliance(id),
    }),
    removeRequirement: useMutation({
      mutationFn: ({ id, requirementId }) => policyComplianceService.removeRequirement(id, requirementId),
      onSuccess: (_, { id }) => invalidateCompliance(id),
    }),
  };
}
