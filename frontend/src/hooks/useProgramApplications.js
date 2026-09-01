import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { programApplicationsService } from '../services/programApplicationsService';

export function useMyProgramApplications() {
  return useQuery({ queryKey: ['program-applications', 'mine'], queryFn: () => programApplicationsService.getMine() });
}

export function useProgramApplicationsByProgram(programId) {
  return useQuery({
    queryKey: ['program-applications', 'program', programId],
    queryFn: () => programApplicationsService.getByProgram(programId),
    enabled: Boolean(programId),
  });
}

export function useProgramApplicationMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['program-applications'] });

  return {
    apply: useMutation({ mutationFn: (payload) => programApplicationsService.apply(payload), onSuccess: invalidate }),
    accept: useMutation({ mutationFn: ({ id, payload }) => programApplicationsService.accept(id, payload), onSuccess: invalidate }),
    reject: useMutation({ mutationFn: ({ id, payload }) => programApplicationsService.reject(id, payload), onSuccess: invalidate }),
    withdraw: useMutation({ mutationFn: (id) => programApplicationsService.withdraw(id), onSuccess: invalidate }),
  };
}
