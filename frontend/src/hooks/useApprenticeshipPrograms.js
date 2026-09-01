import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apprenticeshipProgramsService } from '../services/apprenticeshipProgramsService';

export function useMyApprenticeshipPrograms() {
  return useQuery({
    queryKey: ['apprenticeship-programs', 'mine'],
    queryFn: () => apprenticeshipProgramsService.getMine(),
  });
}

export function useApprenticeshipProgram(id) {
  return useQuery({
    queryKey: ['apprenticeship-programs', id],
    queryFn: () => apprenticeshipProgramsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useApprenticeshipProgramMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['apprenticeship-programs'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['apprenticeship-programs', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => apprenticeshipProgramsService.create(payload), onSuccess: () => invalidate() }),
    update: useMutation({
      mutationFn: ({ id, payload }) => apprenticeshipProgramsService.update(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    publish: useMutation({ mutationFn: (id) => apprenticeshipProgramsService.publish(id), onSuccess: (_, id) => invalidate(id) }),
    close: useMutation({ mutationFn: (id) => apprenticeshipProgramsService.close(id), onSuccess: (_, id) => invalidate(id) }),
    remove: useMutation({ mutationFn: (id) => apprenticeshipProgramsService.remove(id), onSuccess: () => invalidate() }),
    addMilestone: useMutation({
      mutationFn: ({ id, payload }) => apprenticeshipProgramsService.addMilestone(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeMilestone: useMutation({
      mutationFn: ({ id, milestoneId }) => apprenticeshipProgramsService.removeMilestone(id, milestoneId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
