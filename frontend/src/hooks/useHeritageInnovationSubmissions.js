import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { heritageInnovationSubmissionsService } from '../services/heritageInnovationSubmissionsService';

export function useHeritageInnovationSubmissions(params = {}) {
  return useQuery({
    queryKey: ['heritage-innovation-submissions', 'list', params],
    queryFn: () => heritageInnovationSubmissionsService.list(params),
  });
}

export function useHeritageInnovationSubmission(id) {
  return useQuery({
    queryKey: ['heritage-innovation-submissions', id],
    queryFn: () => heritageInnovationSubmissionsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useHeritageInnovationSubmissionMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['heritage-innovation-submissions'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['heritage-innovation-submissions', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => heritageInnovationSubmissionsService.create(payload), onSuccess: () => invalidate() }),
    submit: useMutation({ mutationFn: (id) => heritageInnovationSubmissionsService.submit(id), onSuccess: (_, id) => invalidate(id) }),
    withdraw: useMutation({ mutationFn: (id) => heritageInnovationSubmissionsService.withdraw(id), onSuccess: (_, id) => invalidate(id) }),
    remove: useMutation({ mutationFn: (id) => heritageInnovationSubmissionsService.remove(id), onSuccess: () => invalidate() }),
    addTeamMember: useMutation({
      mutationFn: ({ id, payload }) => heritageInnovationSubmissionsService.addTeamMember(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeTeamMember: useMutation({
      mutationFn: ({ id, memberId }) => heritageInnovationSubmissionsService.removeTeamMember(id, memberId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    addReview: useMutation({
      mutationFn: ({ id, payload }) => heritageInnovationSubmissionsService.addReview(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}
