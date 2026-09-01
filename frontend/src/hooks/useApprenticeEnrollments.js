import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apprenticeEnrollmentsService } from '../services/apprenticeEnrollmentsService';

export function useMyApprenticeEnrollments() {
  return useQuery({ queryKey: ['apprentice-enrollments', 'mine'], queryFn: () => apprenticeEnrollmentsService.getMine() });
}

export function useApprenticeEnrollment(id) {
  return useQuery({
    queryKey: ['apprentice-enrollments', id],
    queryFn: () => apprenticeEnrollmentsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useApprenticeEnrollmentsByProgram(programId) {
  return useQuery({
    queryKey: ['apprentice-enrollments', 'program', programId],
    queryFn: () => apprenticeEnrollmentsService.getByProgram(programId),
    enabled: Boolean(programId),
  });
}

export function useApprenticeEnrollmentMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['apprentice-enrollments'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['apprentice-enrollments', id] });
  };

  return {
    updateMilestoneProgress: useMutation({
      mutationFn: ({ id, milestoneId, payload }) => apprenticeEnrollmentsService.updateMilestoneProgress(id, milestoneId, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    complete: useMutation({
      mutationFn: (id) => apprenticeEnrollmentsService.complete(id),
      onSuccess: (_, id) => invalidate(id),
    }),
  };
}
